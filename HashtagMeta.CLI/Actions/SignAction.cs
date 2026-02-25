using HashtagMeta.Core.Helpers;
using HashtagMeta.Core.Models;

namespace HashtagMeta.CLI.Actions;

public class SignAction : ActionBase<SignActionOptions> {
    public override int Execute(SignActionOptions options) {
        var inputFile = new FileInfo(options.MetadataFile);
        if (inputFile.Exists) {
            var htJson = HashtagMetaJson.FromJsonFile(inputFile);

            if (htJson != null) {
                htJson.Sign(options.PrivateKey, options.PublicKey);

                //save back to file
                var serialized = htJson.ToJson();
                Console.WriteLine("Signed metadata file:");
                Console.WriteLine();
                Console.WriteLine(htJson.ToJson(true));

                File.WriteAllText(options.MetadataFile, serialized);
                return 0;
            }
        }
        Console.WriteLine($"Input file '{options.MetadataFile}' is not a valid input file.");
        return 1;
    }
}