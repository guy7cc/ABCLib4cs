using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Collections.Generic;
using System.Diagnostics;

namespace LibraryMerger.Core.Walker;

/// <summary>
/// C#の構文木を走査し、'using static' で指定されたすべての型のITypeSymbolを収集するビジター。
/// </summary>
public class UsingStaticTypeCollector : CSharpSyntaxWalker
{
    private readonly SemanticModel _semanticModel;
    private readonly HashSet<ITypeSymbol> _collectedTypes;

    /// <summary>
    /// 収集された 'using static' の型のシンボル（重複なし）。
    /// </summary>
    public IReadOnlyCollection<ITypeSymbol> CollectedTypes => _collectedTypes;

    /// <summary>
    /// UsingStaticTypeCollectorの新しいインスタンスを初期化します。
    /// </summary>
    /// <param name="semanticModel">シンボル情報を解決するためのセマンティックモデル。</param>
    public UsingStaticTypeCollector(SemanticModel semanticModel)
    {
        _semanticModel = semanticModel;
        // シンボルの比較には SymbolEqualityComparer.Default を使用し、重複を排除する
        _collectedTypes = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
    }

    /// <summary>
    /// usingディレクティブノードに到達したときに呼び出されます。
    /// </summary>
    /// <param name="node">現在のusingディレクティブノード。</param>
    public override void VisitUsingDirective(UsingDirectiveSyntax node)
    {
        // 1. このusingディレクティブが 'static' キーワードを持っているかチェック
        if (node.StaticKeyword.IsKind(SyntaxKind.StaticKeyword))
        {
            // 2. usingされている名前（例: System.Console）からシンボル情報を取得
            var symbolInfo = _semanticModel.GetSymbolInfo(node.Name);

            // 3. シンボルがITypeSymbolとして解決できるか確認
            if (symbolInfo.Symbol is ITypeSymbol typeSymbol)
            {
                // 4. HashSetにITypeSymbolを追加（重複は自動的に排除される）
                _collectedTypes.Add(typeSymbol);
            }
        }

        // ビジターの探索を継続するためにベースメソッドを呼び出す
        base.VisitUsingDirective(node);
    }
}