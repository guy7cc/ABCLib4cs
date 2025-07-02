using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;

/// <summary>
/// CompilationUnitSyntaxをそのSyntaxTreeのFilePathに基づいて比較（ソート）します。
/// これにより、SortedSet内でファイルパスのアルファベット順に格納できます。
/// </summary>
public class CompilationUnitSyntaxComparer : IComparer<CompilationUnitSyntax>
{
    public int Compare(CompilationUnitSyntax? x, CompilationUnitSyntax? y)
    {
        // nullの基本的な扱い
        if (x is null && y is null) return 0; // 両方nullなら等しい
        if (x is null) return -1; // xだけnullならxが先
        if (y is null) return 1;  // yだけnullならyが先

        // FilePathを取得
        var pathX = x.SyntaxTree.FilePath;
        var pathY = y.SyntaxTree.FilePath;

        // string.Compareを使用してファイルパスを辞書順で比較する
        // これにより、-1, 0, 1 のいずれかが返され、ソート順が決定される
        return string.Compare(pathX, pathY, System.StringComparison.Ordinal);
    }
}