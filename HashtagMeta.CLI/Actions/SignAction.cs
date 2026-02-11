using HashtagMeta.CLI.Helpers;
using HashtagMeta.CLI.Models;
using HashtagMeta.CLI.Services;

namespace HashtagMeta.CLI.Actions;

public class SignAction : ActionBase<SignActionOptions> {
    public override int Execute(SignActionOptions options) {
        var inputFile = new FileInfo(options.MetadataFile);
        if (inputFile.Exists) {
            var htData = HashtagMetaJson.FromJson(inputFile);

            if (htData != null) {
                //create CID for source if a source is provided:
                if (!string.IsNullOrWhiteSpace(htData.Data.Source)) {
                    htData.Data.SourceCID = HashtagFunctions.CreateCID(htData.Data.Source);
                }

                var signedBytes = HashtagCalculator.SignHashtagData(htData.Data, options.PrivateKey, options.PublicKey);

                htData.Signature = signedBytes;

                //save back to file
                if (signedBytes.Length > 0) {
                    var serialized = htData.ToJson();
                    Console.WriteLine("Signed metadata file:");
                    Console.WriteLine();
                    Console.WriteLine(serialized);

                    File.WriteAllText(options.MetadataFile, serialized);
                }
            }
            return 0;
        } else {
            Console.WriteLine($"Input file '{options.MetadataFile}' is not a valid input file.");
        }
        return 1;
    }
}
