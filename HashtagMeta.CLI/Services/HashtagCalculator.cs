using HashtagMeta.CLI.DnProto;
using HashtagMeta.CLI.Helpers;
using HashtagMeta.CLI.Models;

namespace HashtagMeta.CLI.Services;

public class HashtagCalculator {
    private List<FileInfo> _files = [];

    public HashtagCalculator(IEnumerable<string> fileNames) {
        _files = [.. fileNames.Select(f => new FileInfo(f))];
    }

    public HashtagCalculator(string folderName) {
        if (Directory.Exists(folderName)) {
            _files = [.. Directory.GetFiles(folderName).Select(f => new FileInfo(f))];
        }
    }

    public string CreateHashtagMetaJson() {
        var htdata = new HashtagMetaJson {
            Data = new() {
                Issuer = "did:web:user1.test.farmmaps.eu"
            }
        };

        if (_files.Count > 0) {
            foreach (FileInfo file in _files) {
                var fileCid = HashtagFunctions.CreateCID(file);
                htdata.Data.Files.Add(file.Name, new() { FileCID = fileCid });
            }
        }

        return htdata.ToJson();
    }

    public static byte[] SignHashtagData(HashtagData hashtagData, string privateKey, string publicKey) {
        try {
            var signingFunction = Signer.CreateCommitSigningFunction(privateKey, publicKey);

            if (hashtagData != null) {
                var hash = hashtagData.CalculateDagCborHash();
                var signedBytes = signingFunction(hash);

                Console.WriteLine($"Hashtag Data hash: {Convert.ToBase64String(hash)}");
                Console.WriteLine($"Base64: {Convert.ToBase64String(signedBytes)}");
                Console.WriteLine($"Base32: {Base32Encoding.BytesToBase32(signedBytes)}");

                return signedBytes;
            }
            return [];
        } catch (Exception ex) {
            Console.WriteLine($"Error signing HashtagData object:");
            Console.WriteLine(ex.ToString());
            return [];
        }
    }
}
