using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMerger.Core.Rewriter
{
    public class UsingABCTypesRemover : CSharpSyntaxRewriter
    {
        public override SyntaxNode? VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (node.Name.ToFullString().StartsWith("ABCLib4cs"))
            {
                return null;
            }
            return base.VisitUsingDirective(node);
        }
    }
}
