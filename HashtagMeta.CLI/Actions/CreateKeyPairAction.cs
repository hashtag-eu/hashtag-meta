using HashtagMeta.Core.DnProto;
using System.Text;

namespace HashtagMeta.CLI.Actions;

public class CreateKeyPairAction:ActionBase<CreateKeyPairActionOptions> {

    public override int Execute(CreateKeyPairActionOptions options) {
        var generatedKey = KeyPair.Generate(KeyTypes.P256);

        var sb = new StringBuilder();
        sb.AppendLine("");

        sb.AppendLine($"Key Type: {generatedKey.KeyTypeName}");
        sb.AppendLine("");
        sb.AppendLine("PRIVATE KEY: save this securely (eg, add to password manager)");
        sb.AppendLine($"        (Multibase syntax) {generatedKey.PrivateKeyMultibase}");
        sb.AppendLine($"        (Hex syntax) {generatedKey.PrivateKeyHex}");
        sb.AppendLine("");
        sb.AppendLine("PUBLIC KEY: share or publish this (eg, in DID document)");
        sb.AppendLine($"        (DID Key Syntax) {generatedKey.DidKey}");
        sb.AppendLine($"        (Multibase syntax) {generatedKey.PublicKeyMultibase}");
        sb.AppendLine($"        (Hex syntax) {generatedKey.PublicKeyHex}");
        sb.AppendLine("");

        Console.WriteLine(sb.ToString());

        if (options.OutputFile != null) {
            File.WriteAllText(options.OutputFile, sb.ToString());
        }

        return 0;
    }
}
