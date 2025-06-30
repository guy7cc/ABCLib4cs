using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LibraryMerger.Core.Walker
{
    public class UsingCollector : CSharpSyntaxWalker
    {
        private readonly HashSet<string> _uniqueUsingNames = new HashSet<string>();
        private readonly List<UsingDirectiveSyntax> _usings = new List<UsingDirectiveSyntax>();

        public IReadOnlyList<UsingDirectiveSyntax> Usings => _usings;

        public override void VisitUsingDirective(UsingDirectiveSyntax node)
        {
            if (node.Name != null)
            {
                string fullyQualifiedName = node.Name.ToString();

                if (_uniqueUsingNames.Add(fullyQualifiedName))
                {
                    _usings.Add(node);
                }
            }
        }
    }
}
