using LibraryMerger.Core;

internal class Program
{
    private static async Task Main(string[] args)
    {
        var startupClass = args.Length >= 1 ? args[0] : "SubmissionCodes.Program";
        var mergedFileName = args.Length >= 2 ? args[1] : "ProgramMerged.txt";
        var merger = new ProgramMerger();
        var cu = await merger.MergeLibrariesFor(startupClass);
        await File.WriteAllTextAsync(@"..\..\..\..\SubmissionCodes\" + mergedFileName, cu.ToFullString());
    }
}