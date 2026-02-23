using HashtagMeta.Core.Helpers;
using HashtagMeta.Core.Models;
using System.IO.Compression;
using System.Text.Json;

namespace HashtagMeta.Core.Services;

public class HashtagCalculator {
    private List<FileInfo> _files = [];
    private HashtagMetaJson _hashtagMetaJson = new();

    public HashtagCalculator(params string[] fileNames) {
        _files = [.. fileNames.Select(f => new FileInfo(f))];
    }

    public HashtagCalculator(DirectoryInfo dir) {
        if (dir.Exists) {
            _files = [.. dir.GetFiles()];
        }
    }

    public HashtagData CreateHashtagData(string? dataJson = null) {

        HashtagData data = dataJson != null
            ? JsonSerializer.Deserialize<HashtagData>(dataJson) ?? new()
            : new();

        if (_files.Count > 0) {
            foreach (FileInfo file in _files) {
                var fileCid = HashtagFunctions.CreateCID(file);
                data.Files.Add(file.Name, new() { FileCID = fileCid });
            }
        }

        return data;
    }

    public HashtagMetaJson CreateSignedHashtagJson(
        string? dataJson,
        string privateKey,
        string publicKey
    ) {
        var hashTagData = CreateHashtagData(dataJson);

        //create CID for source if a source is provided:
        if (!string.IsNullOrWhiteSpace(hashTagData.Source)) {
            hashTagData.SourceCID = HashtagFunctions.CreateCID(hashTagData.Source);
        }

        var signedBytes = hashTagData.GetSignature(privateKey, publicKey);

        var hashTagJson = new HashtagMetaJson {
            Data = hashTagData,
            Signature = signedBytes
        };

        return hashTagJson;
    }

    public FileInfo CreateSignedHashtagZipfile(
        string outputFileName,
        string? dataJson,
        string privateKey,
        string publicKey,
        string hashtagMetaFileName = "hashtag_meta.json"
    ) {
        var hashTagJson = CreateSignedHashtagJson(dataJson, privateKey, publicKey);

        using var fs = new FileStream(outputFileName, FileMode.CreateNew);
        using var zipFile = new ZipArchive(fs, ZipArchiveMode.Create);

        foreach(var f in _files) {
            zipFile.CreateEntryFromFile(f.FullName, f.Name);
        }
        //create entry for the hashtag meta Json
        var htmetazip = zipFile.CreateEntry(hashtagMetaFileName);

        using var metafs = htmetazip.Open();
        metafs.Write(JsonSerializer.SerializeToUtf8Bytes(hashTagJson));

        return new(outputFileName);
    }
}
