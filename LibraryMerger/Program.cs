using System.Windows;
using LibraryMerger.Core;
using Microsoft.CodeAnalysis;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var startupClass = args.Length >= 1 ? args[0] : "SubmissionCodes.Program";
        var mergedFileName = args.Length >= 2 ? args[1] : "ProgramMerged";
        var merger = new ProgramMerger();
        var cu = await merger.MergeLibrariesFor(startupClass);
        await File.WriteAllTextAsync(@"..\..\..\..\SubmissionCodes\" + mergedFileName + ".txt", cu.ToFullString());
        await File.WriteAllTextAsync(@"..\..\..\..\SubmissionCodes\" + mergedFileName + ".min.txt", cu.NormalizeWhitespace("", "", true).ToFullString());
    }
}