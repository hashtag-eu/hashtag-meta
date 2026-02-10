using HashtagMeta.CLI.DnProto;
using HashtagMeta.CLI.Helpers;

namespace HashtagMeta.CLI.Actions;

public class CreateKeyPairAction:ActionBase<CreateKeyPairActionOptions> {

    public override int Execute(CreateKeyPairActionOptions options) {
        var keyInfo = JsonFunctions.CreateKeyPair(KeyTypes.Secp256k1);

        Console.WriteLine(keyInfo);

        File.WriteAllText("hashtag_keys.txt", keyInfo);

        return 0;
    }
}
