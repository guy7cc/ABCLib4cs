using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMerger.Core.Comparer
{
    /// <summary>
    /// UsingDirectiveSyntaxを、その名前空間名の文字列で比較します。
    /// SortedSet<UsingDirectiveSyntax>などで使用できます。
    /// </summary>
    public class UsingDirectiveSyntaxComparer : IComparer<UsingDirectiveSyntax>
    {
        /// <summary>
        /// シングルトンインスタンスを提供します。
        /// </summary>
        public static readonly UsingDirectiveSyntaxComparer Default = new UsingDirectiveSyntaxComparer();

        /// <summary>
        /// 2つのUsingDirectiveSyntaxノードを比較します。
        /// </summary>
        /// <param name="x">比較対象の1つ目のusingディレクティブ。</param>
        /// <param name="y">比較対象の2つ目のusingディレクティブ。</param>
        /// <returns>比較結果を示す整数。</returns>
        public int Compare(UsingDirectiveSyntax? x, UsingDirectiveSyntax? y)
        {
            // nullの基本的なハンドリング
            if (ReferenceEquals(x, y)) return 0;
            if (y is null) return 1;
            if (x is null) return -1;

            // Nameプロパティ（NameSyntax）を文字列に変換して比較する
            // .ToString() はノードのテキスト表現を返すため、"System.Collections.Generic"のような文字列が得られる
            string xName = x.Name.ToString();
            string yName = y.Name.ToString();

            // 文字列の比較結果を返す
            return string.Compare(xName, yName, StringComparison.Ordinal);
        }
    }
}
