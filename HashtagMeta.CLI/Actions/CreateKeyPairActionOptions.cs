using CommandLine;

namespace HashtagMeta.CLI.Actions;

[Verb("createkeypair", aliases: ["k", "kp", "createkey"], HelpText = "Create a new Hashtag public and private key, optionally in an output file.")]
public class CreateKeyPairActionOptions : ActionOptionsBase {
    [Option('o', "output", HelpText = "Optional output public/private keypair file.")]
    public string? OutputFile { get; set; }
}
