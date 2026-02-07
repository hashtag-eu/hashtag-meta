using HashtagMeta.CLI.Helpers;
using HashtagMeta.CLI.Models;
using System.Text.Json;

namespace HashtagMeta.CLI.Services;

public class HashtagCalculator {
    private List<FileInfo> _files = [];

    private readonly JsonSerializerOptions _jsonOptions = new() {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        IndentSize = 2,
        WriteIndented = true,
        NewLine = "\x0A"
    };

    public HashtagCalculator(IEnumerable<string> fileNames) {
        _files = [.. fileNames.Select(f => new FileInfo(f))];
    }

    public HashtagCalculator(string folderName) {
        if (Directory.Exists(folderName)) {
            _files = [.. Directory.GetFiles(folderName).Select(f => new FileInfo(f))];
        }
    }

    public string GetHashtagMetaJson() {
        var htdata = new HashtagMetaJson {
            Data = new() {
                Issuer = "did:something:something",
                Tags = new() { { "key1", "value1" }, { "key2", "value2" } },
                Source = "https://example.com",
            },
            Signature = ""
        };

        htdata.Data.SourceCID = JsonFunctions.CreateCID(htdata.Data.Source);

        if (_files.Count > 0) {
            foreach (FileInfo file in _files) {
                var fileCid = JsonFunctions.CreateCID(file);
                htdata.Data.Files.Add(new() { FileName = file.Name, FileCID = fileCid });
            }
        }

        return JsonSerializer.Serialize(htdata, _jsonOptions);
    }

    public string CalculateHashtagDataCid(string json) {
        var htData = JsonSerializer.Deserialize<HashtagMetaJson>(json);

        if (htData != null) {
            var dataJson = JsonSerializer.Serialize(htData.Data, _jsonOptions);
            var dataCid = JsonFunctions.CalculateJsonSignature(dataJson);

            htData.Signature = dataCid;
        }

        return JsonSerializer.Serialize(htData, _jsonOptions);
    }
}
