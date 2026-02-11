using CommandLine;

namespace HashtagMeta.CLI.Actions;

[Verb("create", aliases: ["c"], HelpText = "Create a new Hashtag metadata file from input files or folder.")]
public class CreateActionOptions: ActionOptionsBase {

    [Value(0, MetaName = "Hashtag metadata input file", HelpText = "Input file [hashtag_meta.json] to sign with the provided private/public key pair.", Required = false)]
    public string MetadataFile { get; set; } = "hashtag_meta.json";

    [Option('i', "input", Separator = ' ', Required = true, HelpText = "Provide a folder name, compressed file name or list of files to create the Hashtag metadata for.")]
    public IEnumerable<string> InputFiles { get; set; } = [];

    [Option('t', "type", Default = InputFileType.Files, HelpText = "Specify the type of input file.")]
    public InputFileType InputFileType { get; set; } = InputFileType.Files;

    [Option('f', "force", Default = false, HelpText = "Force creation of the hashtag metadata file, overwrite existing file.")]
    public bool Force { get; set; } = false;
}
