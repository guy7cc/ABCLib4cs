using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMerger.Core
{
    public static class NamespaceMerger
    {
        /// <summary>
        /// 名前空間の階層構造を表現するための内部クラス。
        /// </summary>
        private class NamespaceNode
        {
            public string Name { get; set; } // Nameプロパティを書き換え可能にする
            public List<MemberDeclarationSyntax> Members { get; } = new();
            public Dictionary<string, NamespaceNode> Children { get; set; } = new(); // Childrenプロパティを書き換え可能にする

            public NamespaceNode(string name)
            {
                Name = name;
            }
        }

        /// <summary>
        /// 指定されたCompilationUnitSyntax内の複数の名前空間を、最適化されたネスト階層に統合します。
        /// </summary>
        public static CompilationUnitSyntax RefactorAndMergeNamespaces(CompilationUnitSyntax root)
        {
            // 1. 名前空間に属さないメンバーとusingを保持
            var nonNamespaceMembers = root.Members.Where(m => m is not NamespaceDeclarationSyntax);
            var usings = root.Usings;

            // 2. すべての名前空間からメンバーを収集
            var namespaceMembersMap = root.Members
                .OfType<NamespaceDeclarationSyntax>()
                .GroupBy(ns => ns.Name.ToString())
                .ToDictionary(
                    g => g.Key,
                    g => g.SelectMany(ns => ns.Members).ToList()
                );

            // 3. 名前空間の階層ツリーを構築
            var rootNodes = new Dictionary<string, NamespaceNode>();
            foreach (var (fullName, members) in namespaceMembersMap)
            {
                var nameParts = fullName.Split('.');
                var currentNodeMap = rootNodes;
                NamespaceNode lastNode = null;

                foreach (var part in nameParts)
                {
                    if (!currentNodeMap.ContainsKey(part))
                    {
                        currentNodeMap[part] = new NamespaceNode(part);
                    }
                    lastNode = currentNodeMap[part];
                    currentNodeMap = lastNode.Children;
                }

                if (lastNode != null)
                {
                    lastNode.Members.AddRange(members);
                }
            }

            // 4. ★★★ 新機能: 名前空間ツリーを最適化して不要なネストを結合する ★★★
            var optimizedRootNodes = OptimizeNamespaceTree(rootNodes);

            // 5. 最適化されたツリーから、再帰的にNamespaceDeclarationSyntaxを生成する
            var topLevelNamespaces = optimizedRootNodes.Values
                .Select(BuildNamespaceSyntax)
                .ToList();

            // 6. 最終的なCompilationUnitSyntaxを構築する
            var newMembers = new List<MemberDeclarationSyntax>();
            newMembers.AddRange(nonNamespaceMembers);
            newMembers.AddRange(topLevelNamespaces);

            var newRoot = SyntaxFactory.CompilationUnit()
                .WithUsings(usings)
                .WithMembers(SyntaxFactory.List(newMembers))
                .NormalizeWhitespace();

            return newRoot;
        }

        /// <summary>
        /// 名前空間ツリーを最適化し、中間ノードを結合するトップレベルのメソッド。
        /// </summary>
        private static Dictionary<string, NamespaceNode> OptimizeNamespaceTree(Dictionary<string, NamespaceNode> rootNodes)
        {
            // ダミーのルートノードを作成し、トップレベルのノードも同じロジックで扱えるようにする
            var pseudoRoot = new NamespaceNode("") { Children = rootNodes };
            OptimizeNode(pseudoRoot);
            return pseudoRoot.Children;
        }

        /// <summary>
        /// ノードを再帰的に巡回し、中間ノードを結合するヘルパーメソッド。
        /// </summary>
        private static void OptimizeNode(NamespaceNode node)
        {
            // まず子のツリーを再帰的に最適化（ボトムアップ処理）
            foreach (var child in node.Children.Values)
            {
                OptimizeNode(child);
            }

            var newChildren = new Dictionary<string, NamespaceNode>();
            foreach (var child in node.Children.Values)
            {
                // 子が中間ノード（メンバーがなく、子が1つだけ）の場合
                if (child.Members.Count == 0 && child.Children.Count == 1)
                {
                    var grandchild = child.Children.Values.Single();

                    // 子をバイパスし、孫を直接このノードの子にする
                    // その際、名前を結合する (例: "Core" と "Models" -> "Core.Models")
                    grandchild.Name = $"{child.Name}.{grandchild.Name}";
                    newChildren.Add(grandchild.Name, grandchild);
                }
                else
                {
                    // 中間ノードでなければ、そのまま維持する
                    newChildren.Add(child.Name, child);
                }
            }
            node.Children = newChildren;
        }

        /// <summary>
        /// NamespaceNodeツリーから、再帰的にNamespaceDeclarationSyntaxを構築する。
        /// </summary>
        private static NamespaceDeclarationSyntax BuildNamespaceSyntax(NamespaceNode node)
        {
            var childNamespaceDeclarations = node.Children.Values
                .Select(BuildNamespaceSyntax)
                .ToArray<MemberDeclarationSyntax>();

            return SyntaxFactory.NamespaceDeclaration(SyntaxFactory.ParseName(node.Name))
                .AddMembers(node.Members.ToArray())
                .AddMembers(childNamespaceDeclarations);
        }
    }
}
