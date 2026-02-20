using CommandLine;

namespace HashtagMeta.CLI.Actions;

[Verb("sign", aliases: ["s"], HelpText = "Sign a Hashtag metadata file using public and private key pair.")]
public class SignActionOptions : ActionOptionsBase {

    [Value(0, MetaName = "Hashtag metadata input file", HelpText = "Input file [hashtag_meta.json] to sign with the provided private/public key pair.", Required = false)]
    public string MetadataFile { get; set; } = "hashtag_meta.json";

    [Option('p', "private-key", Required = true, HelpText = "Private key (Multibase syntax)")]
    public string PrivateKey { get; set; } = string.Empty;

    [Option('k', "public-key", Required = true, HelpText = "Public key (Multibase syntax)")]
    public string PublicKey { get; set; } = string.Empty;

}
