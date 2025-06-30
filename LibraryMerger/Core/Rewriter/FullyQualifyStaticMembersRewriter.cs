using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

public class FullyQualifyStaticMembersRewriter : CSharpSyntaxRewriter
{
    private readonly SemanticModel _semanticModel;
    private readonly IReadOnlyCollection<ITypeSymbol> _types;

    public FullyQualifyStaticMembersRewriter(SemanticModel semanticModel, IReadOnlyCollection<ITypeSymbol> types)
    {
        // 書き換えにはセマンティックモデルが必須
        _semanticModel = semanticModel;
        // 'using static' で収集した型のセット
        _types = types;
    }

    public override SyntaxNode? VisitIdentifierName(IdentifierNameSyntax node)
    {
        if (node.Parent is QualifiedNameSyntax || node.Parent is MemberAccessExpressionSyntax)
        {
            return node;
        }

        var symbolInfo = _semanticModel.GetSymbolInfo(node);
        var symbol = symbolInfo.Symbol;

        // シンボルが静的なプロパティまたはフィールドであるかチェック
        if (IsStaticMemberOf(symbol, _types))
        {
            // 完全修飾名を持つ MemberAccessExpression を生成して返す
            var typeName = SyntaxFactory.ParseName(symbol.ContainingType.ToDisplayString());
            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                typeName,
                node.WithoutTrivia())
                .WithTriviaFrom(node);
        }

        return base.VisitIdentifierName(node);
    }

    public override SyntaxNode? VisitQualifiedName(QualifiedNameSyntax node)
    {
        var symbolInfo = _semanticModel.GetSymbolInfo(node);
        var symbol = symbolInfo.Symbol;

        if (IsStaticMemberOf(symbol, _types))
        {
            return SyntaxFactory.ParseName(symbol.ToDisplayString());
        }
        return base.VisitQualifiedName(node);
    }

    public override SyntaxNode? VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
    {
        if (node.Expression is not IdentifierNameSyntax) return base.VisitMemberAccessExpression(node);

        var symbolInfo = _semanticModel.GetSymbolInfo(node);
        var symbol = symbolInfo.Symbol;
        // シンボルが静的なメンバーであるかチェック
        if (IsStaticMemberOf(symbol, _types))
        {
            // 完全修飾名を持つ MemberAccessExpression を生成して返す
            var typeName = SyntaxFactory.ParseName(symbol.ToDisplayString());
            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                typeName,
                node.Name.WithoutTrivia())
                .WithTriviaFrom(node);
        }
        return base.VisitMemberAccessExpression(node);
    }

    

    /// <summary>
    /// 指定されたシンボルが、特定の型の静的メンバーであるかどうかを判断します。
    /// </summary>
    /// <param name="memberSymbol">チェック対象のメンバーシンボル。</param>
    /// <param name="typeSymbol">所属を確認する型のシンボル。</param>
    /// <returns>memberSymbol が typeSymbol の静的メンバーである場合は true。それ以外の場合は false。</returns>
    private static bool IsStaticMemberOf(ISymbol memberSymbol, ITypeSymbol typeSymbol)
    {
        // 1. 引数の妥当性をチェック (nullガード)
        if (memberSymbol == null || typeSymbol == null)
        {
            return false;
        }

        if(!memberSymbol.IsStatic && memberSymbol is not ITypeSymbol)
        {
            return false;
        }

        // 3. メンバーシンボルの所属する型 (ContainingType) が、
        //    指定された型 (typeSymbol) と一致するかを堅牢な方法で比較
        return SymbolEqualityComparer.Default.Equals(memberSymbol.ContainingType, typeSymbol);
    }

    private static bool IsStaticMemberOf(ISymbol memberSymbol, IEnumerable<ITypeSymbol> typeSymbols)
    {
        foreach(var typeSymbol in typeSymbols)
        {
            if (IsStaticMemberOf(memberSymbol, typeSymbol))
            {
                return true;
            }
        }
        return false;
    }
}