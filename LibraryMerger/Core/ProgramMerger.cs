using LibraryMerger.Core.Comparer;
using LibraryMerger.Core.Rewriter;
using LibraryMerger.Core.Walker;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMerger.Core
{
    public class ProgramMerger
    {
        private Compilation _compilation;
        private SortedSet<UsingDirectiveSyntax> _usings = new SortedSet<UsingDirectiveSyntax>(new UsingDirectiveSyntaxComparer());
        private SortedSet<ExternAliasDirectiveSyntax> _externAliases = new SortedSet<ExternAliasDirectiveSyntax>(new ExternAliasDirectiveComparer());
        private SyntaxList<MemberDeclarationSyntax> _members = new SyntaxList<MemberDeclarationSyntax>();

        private Queue<CompilationUnitSyntax> queue = new Queue<CompilationUnitSyntax>(256);
        private SortedSet<CompilationUnitSyntax> cuset = new SortedSet<CompilationUnitSyntax>();

        public async Task<CompilationUnitSyntax?> MergeLibrariesFor(string mainClassFQN)
        {
            MSBuildLocator.RegisterDefaults();
            var workspace = MSBuildWorkspace.Create();

            var solutionPath = @"..\..\..\..\ABCLib4cs.sln";
            var solution = await workspace.OpenSolutionAsync(solutionPath);

            var syntaxTrees = new List<SyntaxTree>();
            foreach (var project in solution.Projects)
            {
                if (project.Name == "LibraryMerger") continue;
                foreach (var document in project.Documents)
                {
                    if (document.SourceCodeKind == SourceCodeKind.Regular)
                    {
                        var syntaxTree = await document.GetSyntaxTreeAsync();
                        if (syntaxTree != null)
                        {
                            syntaxTrees.Add(syntaxTree);
                        }
                    }
                }
            }

            var metadataReferences = new HashSet<MetadataReference>();
            foreach (var project in solution.Projects)
            {
                if (project.Name == "LibraryMerger") continue;
                var compilation = await project.GetCompilationAsync();
                if (compilation != null)
                {
                    foreach (var reference in compilation.References)
                    {
                        metadataReferences.Add(reference);
                    }
                }
            }

            _compilation = CSharpCompilation.Create(
                "MergedCompilation",
                syntaxTrees: syntaxTrees,
                references: metadataReferences,
                options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            );

            var syntaxTreeReferences = _compilation.GetTypeByMetadataName(mainClassFQN).DeclaringSyntaxReferences;
            if (syntaxTreeReferences.IsEmpty)
            {
                Console.WriteLine($"{mainClassFQN} was not found.");
                return null;
            }
            var typeSymbols = new HashSet<ITypeSymbol>(SymbolEqualityComparer.Default);
            foreach (var syntaxRef in syntaxTreeReferences)
            {
                var classDeclaration = syntaxRef.GetSyntax();
                var cu = classDeclaration.SyntaxTree.GetRoot() as CompilationUnitSyntax;
                queue.Enqueue(cu);
            }
            while (queue.Count > 0)
            {
                ProcessQueue();
            }
            CompilationUnitSyntax root = SyntaxFactory.CompilationUnit()
                .WithUsings(SyntaxFactory.List(_usings))
                .WithExterns(SyntaxFactory.List(_externAliases))
                .WithMembers(_members)
                .NormalizeWhitespace();
            root = NamespaceMerger.RefactorAndMergeNamespaces(root);
            Console.WriteLine($"Merged {root.Members.Count} members, {root.Usings.Count} usings, and {root.Externs.Count} extern aliases.");
            return root;
        }

        private void ProcessQueue()
        {
            if (queue.Count == 0) return;
            SyntaxNode root = queue.Dequeue();
            Console.WriteLine($"Merging {root.SyntaxTree.FilePath}");
            // 1. Collect type symbols
            var collector = new TypeSymbolCollector(_compilation.GetSemanticModel(root.SyntaxTree));
            ApplyWalker(root, collector);
            // 2. Remove the using static directives
            var usingStaticTypeCollector = new UsingStaticTypeCollector(_compilation.GetSemanticModel(root.SyntaxTree));
            ApplyWalker(root, usingStaticTypeCollector);
            root = ApplyRewriter(root, new FullyQualifyStaticMembersRewriter(_compilation.GetSemanticModel(root.SyntaxTree), usingStaticTypeCollector.CollectedTypes));
            root = ApplyRewriter(root, new UsingStaticRemover());
            // 3. Fully qualify the ABC types
            root = ApplyRewriter(root, new FullyQualifyABCTypesRewriter(_compilation.GetSemanticModel(root.SyntaxTree)));
            // 4. Remove the using directives that uses ABCLib4cs types
            root = ApplyRewriter(root, new UsingABCTypesRemover());
            // 5. Collect usings, extern aliases, and members
            foreach (var item in ((CompilationUnitSyntax)root).Usings)
            {
                _usings.Add(item);
            }
            foreach (var item in ((CompilationUnitSyntax)root).Externs)
            {
                _externAliases.Add(item);
            }
            foreach (var item in ((CompilationUnitSyntax)root).Members)
            {
                _members = _members.Add(item);
            }
            // 6. Add the type symbols to the queue for further processing
            foreach (var typeSymbol in collector.CollectedTypeSymbols)
            {
                foreach(var syntaxRef in typeSymbol.DeclaringSyntaxReferences)
                {
                    var classDeclaration = syntaxRef.GetSyntax();
                    var cu = classDeclaration.SyntaxTree.GetRoot() as CompilationUnitSyntax;
                    if (!cuset.Contains(cu))
                    {
                        cuset.Add(cu);
                        queue.Enqueue(cu);
                    }
                }
            }
        }

        private void ApplyWalker(SyntaxNode node, CSharpSyntaxWalker walker)
        {
            walker.Visit(node);
        }

        private SyntaxNode ApplyRewriter(SyntaxNode node, CSharpSyntaxRewriter rewriter)
        {
            var syntaxTree = node.SyntaxTree;
            var newNode = rewriter.Visit(node);
            SyntaxTree rewrittenTree = CSharpSyntaxTree.Create(
                newNode as CSharpSyntaxNode,
                options: syntaxTree.Options as CSharpParseOptions,
                path: syntaxTree.FilePath,
                encoding: syntaxTree.Encoding
            );
            _compilation = _compilation.ReplaceSyntaxTree(syntaxTree, rewrittenTree);
            return rewrittenTree.GetRoot();
        }
    }
}
