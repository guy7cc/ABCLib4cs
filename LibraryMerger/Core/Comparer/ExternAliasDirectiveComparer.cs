using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace LibraryMerger.Core.Comparer
{
    /// <summary>
    /// ExternAliasDirectiveSyntaxを、そのエイリアス識別子の文字列で比較します。
    /// SortedSet<ExternAliasDirectiveSyntax>などで使用できます。
    /// </summary>
    public class ExternAliasDirectiveComparer : IComparer<ExternAliasDirectiveSyntax>
    {
        /// <summary>
        /// シングルトンインスタンスを提供します。
        /// </summary>
        public static readonly ExternAliasDirectiveComparer Default = new ExternAliasDirectiveComparer();

        /// <summary>
        /// 2つのExternAliasDirectiveSyntaxノードを比較します。
        /// </summary>
        /// <param name="x">比較対象の1つ目のextern aliasディレクティブ。</param>
        /// <param name="y">比較対象の2つ目のextern aliasディレクティブ。</param>
        /// <returns>比較結果を示す整数。</returns>
        public int Compare(ExternAliasDirectiveSyntax? x, ExternAliasDirectiveSyntax? y)
        {
            // nullの基本的なハンドリング
            if (ReferenceEquals(x, y)) return 0;
            if (y is null) return 1;
            if (x is null) return -1;

            // Identifierプロパティ（SyntaxToken）からエイリアス名の文字列を取得
            string xName = x.Identifier.ValueText;
            string yName = y.Identifier.ValueText;

            // 文字列の比較結果を返す
            return string.Compare(xName, yName, StringComparison.Ordinal);
        }
    }
}
