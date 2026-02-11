using HashtagMeta.CLI.DnProto;
using HashtagMeta.CLI.Helpers;

namespace HashtagMeta.CLI.Actions;

public class CreateKeyPairAction:ActionBase<CreateKeyPairActionOptions> {

    public override int Execute(CreateKeyPairActionOptions options) {
        var keyInfo = HashtagFunctions.CreateKeyPair(KeyTypes.P256);

        Console.WriteLine(keyInfo);

        if (options.OutputFile != null) {
            File.WriteAllText(options.OutputFile, keyInfo);
        }

        return 0;
    }
}
