using HashtagMeta.CLI.Services;

namespace HashtagMeta.CLI.Actions;

public class CreateAction : ActionBase<CreateActionOptions> {
    public override int Execute(CreateActionOptions options) {
        var inputFiles = options.InputFiles ?? [];

        if (inputFiles.Any()) {
            var fi = new HashtagCalculator(inputFiles);

            var jsonString = fi.GetHashtagMetaJson();
            Console.WriteLine("hashtag_meta.json:");
            Console.WriteLine();
            Console.WriteLine(jsonString);
            File.WriteAllText("hashtag_meta.json", jsonString);
            return 0;
        }
        Console.WriteLine("Incorrect number of arguments, the init operation needs either a folder or a list of files");
        return 1;
    }
}
