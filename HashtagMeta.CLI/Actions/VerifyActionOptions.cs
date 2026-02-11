using CommandLine;

namespace HashtagMeta.CLI.Actions;

[Verb("verify", aliases: ["v"], HelpText = "Verify a Hashtag metadata file using public key.")]
public class VerifyActionOptions : ActionOptionsBase {
    [Value(0, MetaName = "Hashtag metadata input file", HelpText = "Input file [hashtag_meta.json] to sign with the provided private/public key pair.")]
    public string InputFile { get; set; } = "hashtag_meta.json";

    [Option('k', "public-key", Required = true, HelpText = "Public key (Multibase syntax)")]
    public string PublicKey { get; set; } = string.Empty;

    [Option('o', "output", Default = "hashtag_verify.txt", HelpText = "Output file [hashtag_verify.txt]")]
    public string OutputFile { get; set; } = "hashtag_verify.txt";
}
