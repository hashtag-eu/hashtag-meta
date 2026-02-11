using HashtagMeta.CLI.DnProto;
using Multiformats.Base;
using System.Security.Cryptography;
using System.Text;

namespace HashtagMeta.CLI.Helpers;

public class HashtagFunctions {

    public static string CreateCID(string input) {
        var bytes = Encoding.UTF8.GetBytes(input);

        return calculateCid(bytes);
    }

    public static string CreateCID(FileInfo inputFile) {

        if (!inputFile.Exists) {
            return string.Empty;
        }

        string cid = "";

        var hashAlg = SHA256.Create();
        byte[] hashValue;
        using (Stream s = inputFile.OpenRead()) {
            hashValue = hashAlg.ComputeHash(s);
        }

        using (var ms = new MemoryStream()) {
            ms.Write([1, 0x55, 0x12]); // version, codec and hashtype 1 , raw SHA256
            LEB128.WriteLEB128Signed(ms, hashValue.Length);
            ms.Write(hashValue);
            cid = Multibase.Encode(MultibaseEncoding.Base32Lower, ms.ToArray());
        }

        return cid;
    }

    private static string calculateCid(byte[] bytes) {
        string cid = "";
        var hashAlg = SHA256.Create();

        using (var cs = new CryptoStream(Stream.Null, hashAlg, CryptoStreamMode.Write)) {
            cs.Write(bytes);
            cs.FlushFinalBlock();
        }

        using (var ms = new MemoryStream()) {
            ms.Write([1, 0x55, 0x12]); // version, codec and hashtype 1 , raw SHA256
            LEB128.WriteLEB128Signed(ms, hashAlg.Hash?.Length ?? 0L);
            ms.Write(hashAlg.Hash);
            cid = Multibase.Encode(MultibaseEncoding.Base32Lower, ms.ToArray());
        }

        return cid;
    }

    public static string CreateKeyPair(string keyType) {
        var sb = new StringBuilder();

        var generatedKey = KeyPair.Generate(keyType);

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

        return sb.ToString();
    }
}
