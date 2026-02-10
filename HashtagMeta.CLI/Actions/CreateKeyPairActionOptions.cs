using CommandLine;

namespace HashtagMeta.CLI.Actions;

[Verb("createkeypair", aliases: ["k", "kp"], HelpText = "Create a new Hashtag metadata file from input files or folder.")]
public class CreateKeyPairActionOptions : ActionOptionsBase {
    [Option('o', "output", Default = "hashtag_keys.txt", HelpText = "Output public/private keypair file.")]
    public string? OutputFile { get; set; }
}
