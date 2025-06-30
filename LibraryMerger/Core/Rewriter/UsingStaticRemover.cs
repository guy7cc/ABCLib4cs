using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

public class UsingStaticRemover : CSharpSyntaxRewriter
{
    // using static ディレクティブを削除する
    public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node)
    {
        // 'static' キーワードがあれば、その using ディレクティブを削除 (null を返す)
        if (node.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
        {
            return null;
        }
        return base.VisitUsingDirective(node);
    }
}