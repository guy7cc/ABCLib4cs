using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LibraryMerger.Core;

/// <summary>
///     InvocationExpressionSyntax の拡張メソッドを提供します。
/// </summary>
public static class CSharpSyntaxExtensions
{
    /// <summary>
    ///     InvocationExpressionSyntax を、メソッド呼び出しが完全修飾された新しい InvocationExpressionSyntax に変換します。
    ///     このメソッドは、静的メソッド、インスタンスメソッド、拡張メソッドの呼び出しを処理します。
    /// </summary>
    /// <param name="invocation">変換対象の InvocationExpressionSyntax。</param>
    /// <param name="semanticModel">構文ツリーのセマンティックモデル。</param>
    /// <returns>完全修飾された新しい InvocationExpressionSyntax。変換できない場合は元のインスタンスを返します。</returns>
    public static InvocationExpressionSyntax ToFullyQualifiedInvocationExpression(
        this InvocationExpressionSyntax invocation,
        SemanticModel semanticModel)
    {
        // 1. セマンティックモデルからメソッドのシンボル情報を取得します。
        if (semanticModel.GetSymbolInfo(invocation).Symbol is not IMethodSymbol methodSymbol)
            // シンボルが解決できない場合は変換せず、元のノードを返します。
            return invocation;

        // 2. メソッド名(ジェネリック引数を含む)の部分を取得します。
        var methodNameSyntax = GetMethodNameSyntaxFromExpression(invocation.Expression);
        if (methodNameSyntax == null)
            // 'M()' や 'obj.M()' の形式でなければ対象外とします。
            return invocation;

        // 3. 拡張メソッドとして呼び出されているか判定します。
        //    (例: list.ToList() は Enumerable.ToList(list) の糖衣構文)
        if (methodSymbol.IsExtensionMethod && invocation.Expression is MemberAccessExpressionSyntax)
            // 拡張メソッドの場合: StaticClass.Method(this, ...args) の形式に書き換えます。
            return RewriteExtensionMethodInvocation(invocation, methodSymbol, methodNameSyntax);

        // 通常のインスタンス/静的メソッドの場合: Namespace.Type.Method(...args) 形式に書き換えます。
        return RewriteStandardMethodInvocation(invocation, methodSymbol.OriginalDefinition, methodNameSyntax);
    }

    private static SimpleNameSyntax? GetMethodNameSyntaxFromExpression(ExpressionSyntax expression)
    {
        // obj.M<T>() の場合は MemberAccessExpressionSyntax
        if (expression is MemberAccessExpressionSyntax memberAccess)
            return memberAccess.Name;
        // M<T>() の場合は SimpleNameSyntax (実際にはIdentifierNameSyntax or GenericNameSyntax)
        if (expression is SimpleNameSyntax simpleName)
            return simpleName;

        return null;
    }

    private static InvocationExpressionSyntax RewriteStandardMethodInvocation(
        InvocationExpressionSyntax originalInvocation,
        IMethodSymbol methodSymbol,
        SimpleNameSyntax methodNameSyntax)
    {
        // メソッドが定義されている型の完全修飾名を取得します。
        // "global::" プレフィックスは冗長なため削除します。
        var containingTypeFullName = methodSymbol.ContainingType
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", "");

        // 完全修飾名を表す `NameSyntax` を生成します。
        var qualifiedTypeName = SyntaxFactory.ParseName(containingTypeFullName);

        // `TypeName.MethodName` という `MemberAccessExpression` を生成します。
        var newExpression = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            qualifiedTypeName,
            methodNameSyntax);

        // 元の呼び出し式の Expression 部分だけを置き換えます。
        return originalInvocation.WithExpression(newExpression);
    }

    private static InvocationExpressionSyntax RewriteExtensionMethodInvocation(
        InvocationExpressionSyntax originalInvocation,
        IMethodSymbol extensionMethodSymbol,
        SimpleNameSyntax methodNameSyntax)
    {
        // 拡張メソッドの元の定義 (静的メソッド) を取得します。
        var originalDefinition = extensionMethodSymbol.ReducedFrom ?? extensionMethodSymbol;

        // メソッドが定義されている静的クラスの完全修飾名を取得します。
        var containingTypeFullName = originalDefinition.ContainingType
            .ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat)
            .Replace("global::", "");

        // 完全修飾名を表す `NameSyntax` を生成します。
        var qualifiedTypeName = SyntaxFactory.ParseName(containingTypeFullName);

        // `StaticTypeName.MethodName` という `MemberAccessExpression` を生成します。
        var newMemberAccess = SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            qualifiedTypeName,
            methodNameSyntax);

        // 呼び出しのインスタンス部分 (`instance.Method()` の `instance`) を取得します。
        var instanceExpression = ((MemberAccessExpressionSyntax)originalInvocation.Expression).Expression;

        // 新しい引数リストの先頭にインスタンスを追加します。
        var newArguments =
            originalInvocation.ArgumentList.Arguments.Insert(0, SyntaxFactory.Argument(instanceExpression));
        var newArgumentList = SyntaxFactory.ArgumentList(newArguments);

        // 新しい呼び出し式を生成して返します。
        return SyntaxFactory.InvocationExpression(newMemberAccess, newArgumentList)
            .WithTriviaFrom(originalInvocation); // 元のトリビア(コメントや空白)を引き継ぎます。
    }
}