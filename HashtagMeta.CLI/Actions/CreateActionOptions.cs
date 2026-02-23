using CommandLine;

namespace HashtagMeta.CLI.Actions;

[Verb("create", aliases: ["c"], HelpText = "Create a new Hashtag metadata file from input files or folder.")]
public class CreateActionOptions: ActionOptionsBase {

    [Value(0, MetaName = "Hashtag metadata file", HelpText = "File name of the hashtag metadata file [hashtag_meta.json]", Required = false)]
    public string MetadataFile { get; set; } = "hashtag_meta.json";

    [Option('i', "input", Separator = ' ', Required = true, HelpText = "Provide a folder name, compressed file name or list of files to create the Hashtag metadata for.")]
    public IEnumerable<string> InputFiles { get; set; } = [];

    [Option('t', "type", Default = InputFileType.Auto, HelpText = "Specify the type of input: 'Files', 'Folder' or 'Zip' (Default 'Auto' based on input)")]
    public InputFileType InputFileType { get; set; }

    [Option('f', "force", Default = false, HelpText = "Force creation of the hashtag metadata or zip file, overwrite existing file.")]
    public bool Force { get; set; } = false;

    [Option('d', "data", HelpText = "Initial data block contents for the metadata file.")]
    public string? Data { get; set; }

    [Option('p', "private-key", Required = false, HelpText = "Private key (Multibase syntax)")]
    public string? PrivateKey { get; set; } = null;

    [Option('k', "public-key", Required = false, HelpText = "Public key (Multibase syntax)")]
    public string? PublicKey { get; set; } = null;

    [Option('z', "create-zip", Required = false, HelpText = "Create signed output zip file of the contents including the metadata file.")]
    public string? ZipOutputFile { get; set; } = null;

}
