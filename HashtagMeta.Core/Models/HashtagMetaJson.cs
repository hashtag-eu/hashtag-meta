using HashtagMeta.Core.DnProto;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HashtagMeta.Core.Models;

public record HashtagMetaJson {
    public static readonly JsonSerializerOptions VerboseSerializeOptions = new() {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        IndentSize = 2,
        WriteIndented = true,
        NewLine = "\x0A",
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };
    /// <summary>
    /// Use serializer to calculate DagCbor hash to ignore null values
    /// </summary>
    public static readonly JsonSerializerOptions CompactSerializeOptions = new() {
        ReadCommentHandling = JsonCommentHandling.Skip,
        WriteIndented = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [JsonPropertyName("data")]
    public HashtagData Data { get; set; } = new();
    [JsonPropertyName("sig")]
    public byte[] Signature { get; set; } = [];

    public string ToJson(bool pretty = false) {
        return JsonSerializer.Serialize(
            this,
            pretty ? VerboseSerializeOptions : CompactSerializeOptions
        );
    }

    public byte[] ToUtf8Bytes() => JsonSerializer.SerializeToUtf8Bytes(this, CompactSerializeOptions);

    public static HashtagMetaJson? FromJsonFile(FileInfo fi) {
        using var si = fi.OpenRead();
        var htData = JsonSerializer.Deserialize<HashtagMetaJson>(si);
        return htData;
    }
    public static HashtagMetaJson? FromJsonFile(string fileName) {
        return FromJsonFile(new FileInfo(fileName));
    }
    public static HashtagMetaJson? FromJsonString(string s) {
        var htData = JsonSerializer.Deserialize<HashtagMetaJson>(s);
        return htData;
    }
}

public record HashtagData {
    [JsonPropertyName("issuer")]
    public string Issuer { get; set; } = string.Empty;
    [JsonPropertyName("tags")]
    public Dictionary<string, string> Tags { get; set; } = [];
    [JsonPropertyName("source")]
    public string? Source { get; set; } = null;
    [JsonPropertyName("sourceCid")]
    public string? SourceCid { get; set; } = null;
    [JsonPropertyName("files")]
    public Dictionary<string, HashtagFile> Files { get; set; } = [];


    /// <summary>
    /// Create DagCbor object from this instance
    /// Serializes the instance using default json serialization and
    /// Converts to DagCbor using FromJsonString method which recursively
    /// creates a DagCbor object graph.
    /// Returns the SHA256 hash of the DagCbor bytes.
    /// </summary>
    /// <returns>SHA256 hash of the DagCbor bytes of this instance as JSON</returns>
    public byte[] CalculateDagCborHash() {
        var dataJson = JsonSerializer.Serialize(this, HashtagMetaJson.CompactSerializeOptions);
        var dagCbor = DagCborObject.FromJsonString(dataJson);
        var byteData = dagCbor.ToBytes();
        var hash = SHA256.HashData(byteData);

        return hash;
    }

    /// <summary>
    /// Clone the data part of this instance
    /// </summary>
    /// <returns></returns>
    public HashtagData CloneData() {
        return new() {
            Issuer = Issuer,
            Tags = Tags.ToDictionary(t => t.Key, t => t.Value),
            Source = Source,
            Files = []
        };
    }
}

public record HashtagFile {
    [JsonPropertyName("cid")]
    public string FileCID { get; set; } = string.Empty;
}
