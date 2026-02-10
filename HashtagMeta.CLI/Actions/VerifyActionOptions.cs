using CommandLine;

namespace HashtagMeta.CLI.Actions;

[Verb("verify", aliases: ["v"], HelpText = "Verify a Hashtag metadata file using public key.")]
public class VerifyActionOptions : ActionOptionsBase {
    [Option('i', "input", Default = "hashtag_meta.json", HelpText = "Input file [hashtag_meta.json]")]
    public string InputFile { get; set; } = "hashtag_meta.json";

    [Option('k', "public-key", Default = null, HelpText = "Public key")]
    public string? PublicKey { get; set; }

    [Option('o', "output", Default = "hashtag_verify_report.md", HelpText = "Output file [hashtag_verify_report.md]")]
    public string OutputFile { get; set; } = "hashtag_verify_report.md";
}
