using HashtagMeta.CLI.Helpers;
using HashtagMeta.CLI.Models;
using System.Text.Json;
using System.Xml;

namespace HashtagMeta.CLI.Services;

public class HashtagFileInitializer {
    private List<FileInfo> _files = [];

    private readonly JsonSerializerOptions _jsonOptions = new() {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        IndentSize = 2,
        WriteIndented = true,
        NewLine = "\x0A"
    };

    public HashtagFileInitializer(IEnumerable<string> fileNames) {
        _files = [.. fileNames.Select(f => new FileInfo(f))];
    }

    public HashtagFileInitializer(string folderName) {
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
            Signature = [0x44, 0x55, 0x66]
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
}
