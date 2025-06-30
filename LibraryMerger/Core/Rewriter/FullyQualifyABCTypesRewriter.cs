using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMerger.Core.Rewriter
{
    internal class FullyQualifyABCTypesRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel _semanticModel;
        public FullyQualifyABCTypesRewriter(SemanticModel semanticModel)
        {
            _semanticModel = semanticModel;
        }

        public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Parent is QualifiedNameSyntax || node.Parent is MemberAccessExpressionSyntax)
            {
                return base.VisitIdentifierName(node);
            }
            var symbolInfo = _semanticModel.GetSymbolInfo(node);
            var symbol = symbolInfo.Symbol;

            if (IsFromABCLib(symbol))
            {
                var typeName = SyntaxFactory.ParseName(symbol.ToDisplayString());
                return typeName.WithTriviaFrom(node);
            }
            return base.VisitIdentifierName(node);
        }

        public override SyntaxNode? VisitQualifiedName(QualifiedNameSyntax node)
        {
            var symbolInfo = _semanticModel.GetSymbolInfo(node);
            var symbol = symbolInfo.Symbol;

            if (IsFromABCLib(symbol))
            {
                return SyntaxFactory.ParseName(symbol.ToDisplayString());
            }
            return base.VisitQualifiedName(node);
        }

        public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if(node.Expression is not IdentifierNameSyntax) return base.VisitMemberAccessExpression(node);

            var symbolInfo = _semanticModel.GetSymbolInfo(node.Expression);
            var symbol = symbolInfo.Symbol;

            if (IsFromABCLib(symbol))
            {
                var typeName = SyntaxFactory.ParseName(symbol.ToDisplayString());
                return SyntaxFactory.MemberAccessExpression(
                    SyntaxKind.SimpleMemberAccessExpression,
                    typeName,
                    node.Name.WithoutTrivia())
                    .WithTriviaFrom(node);
            }
            return base.VisitMemberAccessExpression(node);
        }

        private bool IsFromABCLib(ISymbol? symbol)
        {
            if (symbol is ITypeSymbol typeSymbol)
            {
                return typeSymbol.ToDisplayString().StartsWith("ABCLib4cs.");
            }
            return false;
        }
    }
}
