using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LibraryMerger.Core.Rewriter;

public static class InvocationRewriterHelper
{
    /// <summary>
    ///     書き換え済みのノードと、元のノードから得たシンボルを基に、完全修飾された呼び出し式を生成します。
    /// </summary>
    public static SyntaxNode RewriteInvocation(InvocationExpressionSyntax visitedInvocation, IMethodSymbol methodSymbol)
    {
        var methodNameSyntax = GetMethodNameSyntaxFromExpression(visitedInvocation.Expression);
        if (methodNameSyntax == null) return visitedInvocation; // 形式が異なれば何もしない

        // 拡張メソッドかどうかで処理を分岐
        if (methodSymbol.IsExtensionMethod && visitedInvocation.Expression is MemberAccessExpressionSyntax)
            return RewriteExtensionMethodInvocation(visitedInvocation, methodSymbol, methodNameSyntax);

        return RewriteStandardMethodInvocation(visitedInvocation, methodSymbol.OriginalDefinition, methodNameSyntax);
    }

    // 以下は、以前の実装から移動させたヘルパーメソッドです。

    public static SimpleNameSyntax? GetMethodNameSyntaxFromExpression(ExpressionSyntax expression)
    {
        return expression switch
        {
            MemberAccessExpressionSyntax memberAccess => memberAccess.Name,
            SimpleNameSyntax simpleName => simpleName,
            _ => null
        };
    }

    public static InvocationExpressionSyntax RewriteStandardMethodInvocation(
        InvocationExpressionSyntax originalInvocation,
        IMethodSymbol methodSymbol,
        SimpleNameSyntax methodNameSyntax)
    {
        // 型の完全修飾名を生成
        var containingTypeFullName = methodSymbol.ContainingType
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", "");
        var qualifiedTypeName = SyntaxFactory.ParseName(containingTypeFullName);

        // TypeName.MethodName の形式の MemberAccessExpression を生成
        var newExpression = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            qualifiedTypeName,
            methodNameSyntax.WithoutTrivia() // メソッド名自体のトリビアは新しい式に持ち越さない
        ).WithTriviaFrom(methodNameSyntax); // ただし、トリビアは新しい式全体に引き継ぐ

        return originalInvocation.WithExpression(newExpression);
    }

    public static InvocationExpressionSyntax RewriteExtensionMethodInvocation(
        InvocationExpressionSyntax originalInvocation,
        IMethodSymbol extensionMethodSymbol,
        SimpleNameSyntax methodNameSyntax)
    {
        // 拡張メソッドの元の静的メソッド定義を取得
        var originalDefinition = extensionMethodSymbol.ReducedFrom ?? extensionMethodSymbol;

        // 静的クラスの完全修飾名を生成
        var containingTypeFullName = originalDefinition.ContainingType
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", "");
        var qualifiedTypeName = SyntaxFactory.ParseName(containingTypeFullName);

        // StaticClassName.MethodName の MemberAccessExpression を生成
        var newMemberAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            qualifiedTypeName,
            methodNameSyntax);

        // 拡張メソッドの第1引数となるインスタンス部分を取得
        var instanceExpression = ((MemberAccessExpressionSyntax)originalInvocation.Expression).Expression;

        // 新しい引数リストの先頭にインスタンスを追加
        var newArguments =
            originalInvocation.ArgumentList.Arguments.Insert(0, SyntaxFactory.Argument(instanceExpression));
        var newArgumentList = SyntaxFactory.ArgumentList(newArguments);

        // 新しい呼び出し式を生成して返す
        return SyntaxFactory.InvocationExpression(newMemberAccess, newArgumentList)
            .WithTriviaFrom(originalInvocation); // 元のノード全体のトリビアを引き継ぐ
    }
}