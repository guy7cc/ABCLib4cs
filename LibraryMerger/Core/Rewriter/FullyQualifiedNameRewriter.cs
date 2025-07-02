using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LibraryMerger.Core.Rewriter;

public class FullyQualifiedNameRewriter : CSharpSyntaxRewriter
{
    private readonly SemanticModel _semanticModel;

    public FullyQualifiedNameRewriter(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
    }

    /// <summary>
    ///     IdentifierNameSyntaxを完全修飾されたQualifiedNameSyntaxに書き換え
    /// </summary>
    public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
    {
        var symbolInfo = _semanticModel.GetSymbolInfo(node);
        if (symbolInfo.Symbol != null)
        {
            var fullyQualifiedName = GetFullyQualifiedName(symbolInfo.Symbol);
            if (fullyQualifiedName != null && fullyQualifiedName != node.Identifier.ValueText)
                return CreateQualifiedNameSyntax(fullyQualifiedName);
        }

        return base.VisitIdentifierName(node);
    }

    /// <summary>
    ///     QualifiedNameSyntaxを完全修飾されたQualifiedNameSyntaxに書き換え
    /// </summary>
    public override SyntaxNode VisitQualifiedName(QualifiedNameSyntax node)
    {
        var symbolInfo = _semanticModel.GetSymbolInfo(node);
        if (symbolInfo.Symbol != null)
        {
            var fullyQualifiedName = GetFullyQualifiedName(symbolInfo.Symbol);
            if (fullyQualifiedName != null)
            {
                var newQualifiedName = CreateQualifiedNameSyntax(fullyQualifiedName);
                // TypeArgumentListがある場合は再帰的に処理
                if (node.Right is GenericNameSyntax genericRight)
                {
                    var processedTypeArgs = (TypeArgumentListSyntax)Visit(genericRight.TypeArgumentList);
                    var newGenericName = SyntaxFactory.GenericName(
                        ((QualifiedNameSyntax)newQualifiedName).Right.Identifier,
                        processedTypeArgs);
                    return ((QualifiedNameSyntax)newQualifiedName).WithRight(newGenericName);
                }

                return newQualifiedName;
            }
        }

        return base.VisitQualifiedName(node);
    }

    /// <summary>
    ///     AliasQualifiedNameSyntaxを完全修飾されたQualifiedNameSyntaxに書き換え
    /// </summary>
    public override SyntaxNode VisitAliasQualifiedName(AliasQualifiedNameSyntax node)
    {
        var symbolInfo = _semanticModel.GetSymbolInfo(node);
        if (symbolInfo.Symbol != null)
        {
            var fullyQualifiedName = GetFullyQualifiedName(symbolInfo.Symbol);
            if (fullyQualifiedName != null)
            {
                var newQualifiedName = CreateQualifiedNameSyntax(fullyQualifiedName);
                // TypeArgumentListがある場合は再帰的に処理
                if (node.Name is GenericNameSyntax genericName)
                {
                    var processedTypeArgs = (TypeArgumentListSyntax)Visit(genericName.TypeArgumentList);
                    var newGenericName = SyntaxFactory.GenericName(
                        ((QualifiedNameSyntax)newQualifiedName).Right.Identifier,
                        processedTypeArgs);
                    return ((QualifiedNameSyntax)newQualifiedName).WithRight(newGenericName);
                }

                return newQualifiedName;
            }
        }

        return base.VisitAliasQualifiedName(node);
    }

    /// <summary>
    ///     GenericNameSyntaxを完全修飾されたQualifiedNameSyntaxに書き換え
    ///     TypeArgumentList内についても再帰的にすべてのシンタックスについて書き換え
    /// </summary>
    public override SyntaxNode VisitGenericName(GenericNameSyntax node)
    {
        var symbolInfo = _semanticModel.GetSymbolInfo(node);
        if (symbolInfo.Symbol != null)
        {
            var fullyQualifiedName = GetFullyQualifiedName(symbolInfo.Symbol);
            if (fullyQualifiedName != null)
            {
                // TypeArgumentListを再帰的に処理
                var processedTypeArgs = (TypeArgumentListSyntax)Visit(node.TypeArgumentList);

                var baseQualifiedName = CreateQualifiedNameSyntax(fullyQualifiedName);
                var newGenericName = SyntaxFactory.GenericName(
                    ((QualifiedNameSyntax)baseQualifiedName).Right.Identifier,
                    processedTypeArgs);

                return ((QualifiedNameSyntax)baseQualifiedName).WithRight(newGenericName);
            }
        }

        // シンボル情報が取得できない場合でも、TypeArgumentListは再帰的に処理
        var visitedTypeArgs = (TypeArgumentListSyntax)Visit(node.TypeArgumentList);
        if (visitedTypeArgs != node.TypeArgumentList) return node.WithTypeArgumentList(visitedTypeArgs);

        return base.VisitGenericName(node);
    }

    /// <summary>
    ///     MemberAccessExpressionをルート名前空間からのMemberAccessExpressionに書き換え
    /// </summary>
    public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        var symbolInfo = _semanticModel.GetSymbolInfo(node);
        if (symbolInfo.Symbol != null)
        {
            var fullyQualifiedName = GetFullyQualifiedName(symbolInfo.Symbol);
            if (fullyQualifiedName != null) return CreateMemberAccessExpression(fullyQualifiedName);
        }

        return base.VisitMemberAccessExpression(node);
    }

    /// <summary>
    ///     TypeArgumentListを再帰的に処理
    /// </summary>
    public override SyntaxNode VisitTypeArgumentList(TypeArgumentListSyntax node)
    {
        var visitedArgs = node.Arguments.Select(arg => (TypeSyntax)Visit(arg)).ToArray();
        var hasChanges = visitedArgs.Zip(node.Arguments, (visited, original) => !ReferenceEquals(visited, original))
            .Any(changed => changed);

        if (hasChanges) return SyntaxFactory.TypeArgumentList(SyntaxFactory.SeparatedList(visitedArgs));

        return base.VisitTypeArgumentList(node);
    }

    /// <summary>
    ///     シンボルから完全修飾名を取得
    /// </summary>
    private string GetFullyQualifiedName(ISymbol symbol)
    {
        if (symbol == null) return null;

        // 組み込み型の場合はそのまま返す
        if (symbol is ITypeSymbol typeSymbol && typeSymbol.SpecialType != SpecialType.None) return null; // 組み込み型は変換しない

        var parts = new List<string>();
        var current = symbol;

        while (current != null && !string.IsNullOrEmpty(current.Name))
        {
            if (current is INamespaceSymbol namespaceSymbol && namespaceSymbol.IsGlobalNamespace)
                break;

            parts.Insert(0, current.Name);
            current = current.ContainingSymbol;
        }

        return parts.Count > 1 ? string.Join(".", parts) : null;
    }

    /// <summary>
    ///     完全修飾名からQualifiedNameSyntaxを作成
    /// </summary>
    private QualifiedNameSyntax CreateQualifiedNameSyntax(string fullyQualifiedName)
    {
        var parts = fullyQualifiedName.Split('.');
        if (parts.Length < 2) return null;

        NameSyntax result = SyntaxFactory.IdentifierName(parts[0]);
        for (var i = 1; i < parts.Length; i++)
            result = SyntaxFactory.QualifiedName(result, SyntaxFactory.IdentifierName(parts[i]));

        return (QualifiedNameSyntax)result;
    }

    /// <summary>
    ///     完全修飾名からMemberAccessExpressionを作成
    /// </summary>
    private MemberAccessExpressionSyntax CreateMemberAccessExpression(string fullyQualifiedName)
    {
        var parts = fullyQualifiedName.Split('.');
        if (parts.Length < 2) return null;

        ExpressionSyntax result = SyntaxFactory.IdentifierName(parts[0]);
        for (var i = 1; i < parts.Length; i++)
            result = SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                result,
                SyntaxFactory.IdentifierName(parts[i]));

        return (MemberAccessExpressionSyntax)result;
    }
}