// See https://aka.ms/new-console-template for more information
using System.CommandLine;
using System.Diagnostics;
using System.Text.Json;
using Loretta.CodeAnalysis;
using Loretta.Retargeting.Host;
using Loretta.Retargeting.Host.Model;
using Microsoft.Extensions.FileSystemGlobbing;
using Tsu.Numerics;

var threadsOption = new Option<int>(new[] { "-j", "-t", "--threads" }, () => 0, "The amount of threads to use (if 0 defaults to the amount of cores in the system).");
threadsOption.AddValidator((result) =>
{
    var threads = result.GetValueForOption(threadsOption);
    if (threads < 0)
        result.ErrorMessage = "The number of threads must be equal to or greater than 0.";
});
var quietOption = new Option<bool>(new[] { "-q", "-s", "--quiet", "--silent" }, () => false, "Whether no console output should be emitted apart from errors in stderr (and errors will be emitted in short form, without highlighting it in the code).");
var pathArgument = new Argument<FileInfo>("path", "The path to the json project file.")
    .LegalFilePathsOnly()
    .ExistingOnly();

var rootCommand = new RootCommand("Process the file list according to the project specification.")
{
    pathArgument,
    threadsOption,
    quietOption
};
rootCommand.SetHandler(async (ctx) =>
{
    var path = ctx.ParseResult.GetValueForArgument(pathArgument);
    var threads = ctx.ParseResult.GetValueForOption(threadsOption);
    var quiet = ctx.ParseResult.GetValueForOption(quietOption);
    ctx.ExitCode = await MainCommand(path, threads, quiet);
});
await rootCommand.InvokeAsync(args);

static async Task<int> MainCommand(FileInfo projectFileInfo, int threads, bool quiet)
{
    var sw = Stopwatch.StartNew();

    if (threads <= 0)
        threads = Environment.ProcessorCount;

    var projectRoot = projectFileInfo.DirectoryName! + Path.DirectorySeparatorChar;

    if (!quiet)
        await Console.Out.WriteLineAsync("Loading project file...");

    var projectFile = await LoadProjectFile(projectFileInfo) ?? throw new InvalidOperationException("Unable to load the project file.");

    if (!quiet)
        await Console.Out.WriteLineAsync("Listing files to pack...");
    var matcher = new Matcher();
    matcher.AddIncludePatterns(projectFile.Files.Where(f => !f.StartsWith('!')));
    matcher.AddExcludePatterns(projectFile.Files.Where(f => f.StartsWith('!')).Select(f => f[1..]));
    _ = matcher.AddExclude(projectFile.OutputPath);
    var files = matcher.GetResultsInFullPath(projectRoot);

    if (!quiet)
        await Console.Out.WriteLineAsync("Loading files...");

    if (threads == 1)
    {
        foreach (var file in files)
        {
            // TODO: File loading and parsing.
            throw new NotImplementedException();
        }
    }
    else
    {
        Parallel.ForEach(
            files,
            new ParallelOptions { MaxDegreeOfParallelism = threads },
            file =>
                // TODO: File loading and parsing.
                throw new NotImplementedException());
    }

    if (!quiet)
        await Console.Out.WriteLineAsync("Checking for errors...");

    // TODO: Error handling.
    throw new NotImplementedException();

    if (!quiet)
        await Console.Out.WriteLineAsync("Generating output file...");
    var outputPath = new DirectoryInfo(Path.GetFullPath(projectFile.OutputPath, projectRoot));
    outputPath.Create();

    // TODO: Result outputting.
    throw new NotImplementedException();

    sw.Stop();
    var delta = sw.Elapsed.Ticks; // ElapsedTicks returns the raw, os-specific number of ticks.

    await Console.Out.WriteLineAsync($"Done in {Duration.Format(delta)}!");

    return 0;
}

static async Task WriteDiagnostic(string root, Diagnostic diagnostic, bool simplified)
{
    if (simplified || diagnostic.Location.Kind != LocationKind.SourceFile)
    {
        await Console.Error.WriteLineAsync(diagnostic.ToString());
    }
    else
    {
        await DiagnosticWriter.WriteDiagnostic(Console.Error, root, diagnostic);
    }
}

static async Task<ProjectFile?> LoadProjectFile(FileInfo projectFileInfo)
{
    using var stream = projectFileInfo.OpenRead();
    return await JsonSerializer.DeserializeAsync(stream, ProjectJsonContext.Default.ProjectFile);
}
