using CommandLine;

namespace HashtagMeta.CLI.Actions;

[Verb("sign", aliases: ["s"], HelpText = "Sign a Hashtag metadata file using public and private key pair.")]
public class SignActionOptions : ActionOptionsBase {

    [Option('i', "input", Default = "hashtag_meta.json", HelpText = "Input file [hashtag_meta.json]")]
    public string InputFile { get; set; } = "hashtag_meta.json";

    [Option('o', "output", Default = "hashtag_meta.json", HelpText = "Output file [hashtag_meta.json]")]
    public string OutputFile { get; set; } = "hashtag_meta.json";

    [Option('p', "private-key", Required = true, HelpText = "Private key")]
    public string PrivateKey { get; set; } = string.Empty;

    [Option('k', "public-key", Required = true, HelpText = "Public key")]
    public string PublicKey { get; set; } = string.Empty;

}
