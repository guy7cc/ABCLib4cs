using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMerger.Core
{
    public class FQNHelper
    {
        public static string GetFullyQualifiedMetadataName(ISymbol symbol)
        {
            if (symbol == null || IsRootNamespace(symbol))
            {
                return string.Empty;
            }

            // ContainingSymbolを再帰的にたどり、親の名前を構築する
            var parentName = GetFullyQualifiedMetadataName(symbol.ContainingSymbol);

            // 親の名前があれば、セパレータを挟んで自身のメタデータ名を追加する
            if (!string.IsNullOrEmpty(parentName))
            {
                // 親が型（入れ子クラス）ならセパレータは '+'、名前空間なら '.'
                char separator = symbol.ContainingSymbol is ITypeSymbol ? '+' : '.';
                return parentName + separator + symbol.MetadataName;
            }

            return symbol.MetadataName;
        }

        private static bool IsRootNamespace(ISymbol symbol)
        {
            return symbol is INamespaceSymbol ns && ns.IsGlobalNamespace;
        }
    }
}
