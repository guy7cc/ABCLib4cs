using LibraryMerger;
using LibraryMerger.Core;
using Microsoft.Build.Locator;
using Microsoft.Build.Tasks;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.MSBuild;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

class Program
{
    static async Task Main(string[] args)
    {
        string startupClass = args.Length >= 1 ? args[0] : "SubmissionCodes.Program";
        string mergedFileName = args.Length >= 2 ? args[1] : "ProgramMerged.txt";
        ProgramMerger merger = new ProgramMerger();
        var cu = await merger.MergeLibrariesFor(startupClass);
        Console.WriteLine("The whole text is following: \n");
        Console.WriteLine(cu.ToFullString());
        File.WriteAllText(@"..\..\..\..\SubmissionCodes\" + mergedFileName, cu.ToFullString());
    }
}