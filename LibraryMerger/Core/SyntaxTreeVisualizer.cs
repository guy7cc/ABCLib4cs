using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMerger.Core
{
    public class SyntaxTreeVisualizer
    {
        /// <summary>
        /// 指定されたシンタックスツリーのルートノードから構造を可視化します。
        /// </summary>
        /// <param name="root">可視化するシンタックスツリーのルートノード。</param>
        public static void Visualize(SyntaxNode root)
        {
            if (root == null)
            {
                Console.WriteLine("Root node is null.");
                return;
            }
            // 内部の再帰メソッドを呼び出してツリー描画を開始
            PrintNode(root);
        }

        /// <summary>
        /// シンタックスノードを再帰的に走査してコンソールに出力します。
        /// </summary>
        /// <param name="node">走査の起点となるノード</param>
        /// <param name="indent">インデント用の文字列</param>
        /// <param name="isLast">兄弟要素の中で最後の要素であるか</param>
        private static void PrintNode(SyntaxNode node, string indent = "", bool isLast = true)
        {
            var marker = isLast ? "└──" : "├──";
            Console.Write($"{indent}{marker}");
            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.Write(node.Kind());
            Console.ResetColor();
            Console.WriteLine($" [{node.Span}]");

            var newIndent = indent + (isLast ? "    " : "│   ");

            // 子要素（ノードとトークン）を取得
            var children = node.ChildNodesAndTokens();
            var lastChild = children.LastOrDefault();

            foreach (var child in children)
            {
                if (child.IsNode)
                {
                    // 子要素がノードの場合、再帰的に処理
                    PrintNode(child.AsNode()!, newIndent, child.Equals(lastChild));
                }
                else if (child.IsToken)
                {
                    // 子要素がトークンの場合、情報を出力
                    PrintToken(child.AsToken(), newIndent, child.Equals(lastChild));
                }
            }
        }

        /// <summary>
        /// シンタックストークンとそのトリビアをコンソールに出力します。
        /// </summary>
        private static void PrintToken(SyntaxToken token, string indent, bool isLast)
        {
            var marker = isLast ? "└──" : "├──";
            Console.Write($"{indent}{marker}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Token: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write(token.Kind());
            Console.ResetColor();

            if (!string.IsNullOrEmpty(token.Text) && token.Text.Trim().Length > 0)
            {
                Console.Write($" \"{token.Text}\"");
            }
            Console.WriteLine($" [{token.Span}]");

            var newIndent = indent + (isLast ? "    " : "│   ");

            // 先行トリビア
            if (token.HasLeadingTrivia)
            {
                foreach (var trivia in token.LeadingTrivia)
                {
                    PrintTrivia(trivia, newIndent, false);
                }
            }

            // 後続トリビア
            if (token.HasTrailingTrivia)
            {
                var lastTrivia = token.TrailingTrivia.LastOrDefault();
                foreach (var trivia in token.TrailingTrivia)
                {
                    PrintTrivia(trivia, newIndent, trivia.Equals(lastTrivia));
                }
            }
        }

        /// <summary>
        /// シンタックストリビアをコンソールに出力します。
        /// </summary>
        private static void PrintTrivia(SyntaxTrivia trivia, string indent, bool isLast)
        {
            var marker = isLast ? "└──" : "├──";
            Console.Write($"{indent}{marker}");
            Console.ForegroundColor = ConsoleColor.Gray;
            Console.Write("Trivia: ");
            Console.Write(trivia.Kind());
            Console.ResetColor();
            Console.WriteLine($" \"{trivia.ToFullString().Trim()}\"");
        }
    }
}
