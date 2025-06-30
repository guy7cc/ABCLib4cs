using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMerger.Core.Walker
{
    public class TypeSymbolCollector : CSharpSyntaxWalker
    {
        private readonly SemanticModel _semanticModel;
        private readonly List<ITypeSymbol> _collectedTypeSymbols = new List<ITypeSymbol>();

        public IReadOnlyCollection<ITypeSymbol> CollectedTypeSymbols => _collectedTypeSymbols;
        public TypeSymbolCollector(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        private void HandleTypeSyntax(TypeSyntax node)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node);
            var symbol = symbolInfo.Symbol;

            if (symbol is not ITypeSymbol typeSymbol) return;

            HandleTypeSymbol(typeSymbol);
        }

        private void HandleTypeSymbol(ITypeSymbol? typeSymbol)
        {
            if (typeSymbol == null) return;

            _collectedTypeSymbols.Add(typeSymbol);
        }   

        public override void Visit(SyntaxNode? node)
        {
            if (node is ArrayTypeSyntax arrayTypeSyntax)
            {
                HandleTypeSyntax(arrayTypeSyntax.ElementType);
            }
            else if (node is NullableTypeSyntax nullableTypeSyntax)
            {
                HandleTypeSyntax(nullableTypeSyntax.ElementType);
            }
            else if (node is PointerTypeSyntax pointerTypeSyntax)
            {
                HandleTypeSyntax(pointerTypeSyntax.ElementType);
            }
            else if (node is PredefinedTypeSyntax predefinedTypeSyntax)
            {
                HandleTypeSyntax(predefinedTypeSyntax);
            }
            else if (node is QualifiedNameSyntax qualifiedNameSyntax)
            {
                HandleTypeSyntax(qualifiedNameSyntax.Right);
            }
            else if (node is GenericNameSyntax genericNameSyntax)
            {
                INamedTypeSymbol namedTypeSymbol = _semanticModel.GetSymbolInfo(genericNameSyntax).Symbol as INamedTypeSymbol;
                HandleTypeSymbol(namedTypeSymbol.ConstructUnboundGenericType());
                foreach(var typeArgument in genericNameSyntax.TypeArgumentList.Arguments)
                {
                    HandleTypeSyntax(typeArgument);
                }
            }
            else if (node is TypeSyntax typeSyntax)
            {
                HandleTypeSyntax(typeSyntax);
            }
            base.Visit(node);
        }
    }
}
