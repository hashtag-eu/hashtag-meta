using CommandLine;

namespace HashtagMeta.CLI.Actions;

[Verb("create", aliases: ["c"], HelpText = "Create a new Hashtag metadata file from input files or folder.")]
public class CreateActionOptions: ActionOptionsBase {

    [Option('i', "input", Separator = ' ', Required = true, HelpText = "Provide a folder name, compressed file name or list of files to create the Hashtag metadata for.")]
    public IEnumerable<string> InputFiles { get; set; } = [];

    [Option('t', "type", Default = InputFileType.Files, HelpText = "Specify the type of input file.")]
    public InputFileType InputFileType { get; set; } = InputFileType.Files;
}
