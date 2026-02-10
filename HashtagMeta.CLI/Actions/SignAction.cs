using HashtagMeta.CLI.Services;

namespace HashtagMeta.CLI.Actions; 
public class SignAction: ActionBase<SignActionOptions> {
    public override int Execute(SignActionOptions options) {

        var jsonString = HashtagCalculator.CalculateHashtagDataCid(options.InputFile);

        if (!string.IsNullOrEmpty(jsonString)) {
            File.WriteAllText(options.OutputFile, jsonString);
        }

        return 0;
    }
}
