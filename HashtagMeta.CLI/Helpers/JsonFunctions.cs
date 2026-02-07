using System.Security.Cryptography;
using System.Text;
using Cid;
using Multiformats.Base;

namespace HashtagMeta.CLI.Helpers; 
public class JsonFunctions {
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
            LEB128.WriteLEB128Signed(ms, (long)hashAlg.Hash?.Length);
            ms.Write(hashAlg.Hash);
            cid = Multibase.Encode(MultibaseEncoding.Base32Lower, ms.ToArray());
        }

        return cid;
    }
}
