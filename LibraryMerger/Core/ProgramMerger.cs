using LibraryMerger.Core.Comparer;
using LibraryMerger.Core.Rewriter;
using LibraryMerger.Core.Walker;
using Microsoft.Build.Locator;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;

namespace LibraryMerger.Core;

public class ProgramMerger
{
    private Compilation _compilation;
    private readonly SortedSet<ExternAliasDirectiveSyntax> _externAliases = new(new ExternAliasDirectiveComparer());
    private SyntaxList<MemberDeclarationSyntax> _members;
    private readonly SortedSet<UsingDirectiveSyntax> _usings = new(new UsingDirectiveSyntaxComparer());
    private readonly SortedSet<CompilationUnitSyntax> cuset = new(new CompilationUnitSyntaxComparer());

    private readonly Queue<CompilationUnitSyntax> queue = new(256);

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
                if (document.SourceCodeKind == SourceCodeKind.Regular)
                {
                    var syntaxTree = await document.GetSyntaxTreeAsync();
                    if (syntaxTree != null) syntaxTrees.Add(syntaxTree);
                }
        }

        var metadataReferences = new HashSet<MetadataReference>();
        foreach (var project in solution.Projects)
        {
            if (project.Name == "LibraryMerger") continue;
            var compilation = await project.GetCompilationAsync();
            if (compilation != null)
                foreach (var reference in compilation.References)
                    metadataReferences.Add(reference);
        }

        _compilation = CSharpCompilation.Create(
            "MergedCompilation",
            syntaxTrees,
            metadataReferences,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
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
            var classDeclaration = await syntaxRef.GetSyntaxAsync();
            var cu = await classDeclaration.SyntaxTree.GetRootAsync() as CompilationUnitSyntax;
            cuset.Add(cu);
            queue.Enqueue(cu);
        }

        while (queue.Count > 0) ProcessQueue();

        var root = SyntaxFactory.CompilationUnit()
            .WithUsings(SyntaxFactory.List(_usings))
            .WithExterns(SyntaxFactory.List(_externAliases))
            .WithMembers(_members)
            .NormalizeWhitespace();
        root = NamespaceMerger.RefactorAndMergeNamespaces(root);
        Console.WriteLine(
            $"Merged {root.Members.Count} members, {root.Usings.Count} usings, and {root.Externs.Count} extern aliases.");
        return root;
    }

    private void ProcessQueue()
    {
        if (queue.Count == 0) return;
        SyntaxNode root = queue.Dequeue();
        Console.WriteLine($"Merging {root.SyntaxTree.FilePath}");
#if DEBUG
        //SyntaxTreeVisualizer.Visualize(root);
#endif
        // Collect type symbols
        var collector = new TypeSymbolCollector(_compilation.GetSemanticModel(root.SyntaxTree));
        ApplyWalker(root, collector);
        // Remove the using static directives
        var usingStaticTypeCollector = new UsingStaticTypeCollector(_compilation.GetSemanticModel(root.SyntaxTree));
        ApplyWalker(root, usingStaticTypeCollector);
        //root = ApplyRewriter(root, new FullyQualifiedNameRewriter(_compilation.GetSemanticModel(root.SyntaxTree)).Visit);
        root = ApplyRewriter(root,
            new FullyQualifyStaticMembersRewriter(_compilation.GetSemanticModel(root.SyntaxTree),
                usingStaticTypeCollector.CollectedTypes).Visit);
        root = ApplyRewriter(root, new UsingStaticRemover().Visit);
        // Fully qualify the ABC types
        root = ApplyRewriter(root, new FullyQualifyABCTypesRewriter(_compilation.GetSemanticModel(root.SyntaxTree)).Visit);
        // Remove the using directives that uses ABCLib4cs types
        root = ApplyRewriter(root, new UsingABCTypesRemover().Visit);
        // Collect usings, extern aliases, and members
        foreach (var item in ((CompilationUnitSyntax)root).Usings) _usings.Add(item.WithoutTrivia());
        foreach (var item in ((CompilationUnitSyntax)root).Externs) _externAliases.Add(item.WithoutTrivia());
        foreach (var item in ((CompilationUnitSyntax)root).Members) _members = _members.Add(item);
        // 6. Add the type symbols to the queue for further processing
        foreach (var typeSymbol in collector.CollectedTypeSymbols)
        foreach (var syntaxRef in typeSymbol.DeclaringSyntaxReferences)
        {
            var classDeclaration = syntaxRef.GetSyntax();
            var cu = classDeclaration.SyntaxTree.GetRoot() as CompilationUnitSyntax;
            if (cuset.Add(cu))
            {
                queue.Enqueue(cu);
            }
        }
    }

    private void ApplyWalker(SyntaxNode node, CSharpSyntaxWalker walker)
    {
        walker.Visit(node);
    }

    private SyntaxNode ApplyRewriter(SyntaxNode node, Func<SyntaxNode, SyntaxNode> rewriter)
    {
        var syntaxTree = node.SyntaxTree;
        var newNode = rewriter(node);
        var rewrittenTree = CSharpSyntaxTree.Create(
            newNode as CSharpSyntaxNode,
            syntaxTree.Options as CSharpParseOptions,
            syntaxTree.FilePath,
            syntaxTree.Encoding
        );
        _compilation = _compilation.ReplaceSyntaxTree(syntaxTree, rewrittenTree);
        return rewrittenTree.GetRoot();
    }
}