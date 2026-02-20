using CommandLine;

namespace HashtagMeta.CLI.Actions;

public enum InputFileType {
    Auto,
    Files,
    Zip,
    Folder
}

public class ActionOptionsBase {
    [Option('w', "wait", Default = false, HelpText = "Wait for keypress after command completes.")]
    public bool Wait { get; set; } = false;

    [Option('v', "verbose", Default = false, HelpText = "Set to run in verbose mode.")]
    public bool Verbose { get; set; } = false;
}
