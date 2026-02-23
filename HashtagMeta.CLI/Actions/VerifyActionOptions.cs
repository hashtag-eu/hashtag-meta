using CommandLine;

namespace HashtagMeta.CLI.Actions;

[Verb("verify", aliases: ["v"], HelpText = "Verify a Hashtag metadata file using public key.")]
public class VerifyActionOptions : ActionOptionsBase {
    [Value(0, MetaName = "Hashtag metadata file", HelpText = "File name of the hashtag metadata file [hashtag_meta.json]", Required = false)]
    public string MetadataFile { get; set; } = "hashtag_meta.json";

    [Option('i', "input", Separator = ' ', Required = true, HelpText = "Provide a folder name, compressed file name or list of files to verify the Hashtag metadata for.")]
    public IEnumerable<string> InputFiles { get; set; } = [];

    [Option('t', "type", Default = InputFileType.Auto, HelpText = "Specify the type of input: 'Files', 'Folder' or 'Zip' (Default 'Auto' based on input)")]
    public InputFileType InputFileType { get; set; }

    [Option('k', "public-key", Required = true, HelpText = "Public key (Multibase syntax)")]
    public string PublicKey { get; set; } = string.Empty;
}
