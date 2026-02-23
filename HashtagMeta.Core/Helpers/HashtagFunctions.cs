using HashtagMeta.Core.DnProto;
using Multiformats.Base;
using System.Security.Cryptography;
using System.Text;

namespace HashtagMeta.Core.Helpers;

public class HashtagFunctions {

    public static string CreateCID(string input) {
        var bytes = Encoding.UTF8.GetBytes(input);

        return CalculateCid(bytes);
    }

    public static string CreateCID(FileInfo inputFile) {

        if (!inputFile.Exists) {
            return string.Empty;
        }

        var hashAlg = SHA256.Create();
        byte[] hashValue;
        using (Stream s = inputFile.OpenRead()) {
            hashValue = hashAlg.ComputeHash(s);
        }
        return WriteCid(hashValue);
    }

    public static string CalculateCid(byte[] bytes) {
        var hashAlg = SHA256.Create();

        using (var cs = new CryptoStream(Stream.Null, hashAlg, CryptoStreamMode.Write)) {
            cs.Write(bytes);
            cs.FlushFinalBlock();
        }
        return WriteCid(hashAlg.Hash);
    }

    private static string WriteCid(byte[]? hash) {
        string cid = "";
        using (var ms = new MemoryStream()) {
            ms.Write([1, 0x55, 0x12]); // version, codec and hashtype 1 , raw SHA256
            LEB128.WriteLEB128Signed(ms, hash?.Length ?? 0L);
            ms.Write(hash);
            cid = Multibase.Encode(MultibaseEncoding.Base32Lower, ms.ToArray());
        }
        return cid;
    }

    public static KeyPair CreateKeyPair(string keyType) {

        var generatedKey = KeyPair.Generate(keyType);

        return generatedKey;
    }
}
