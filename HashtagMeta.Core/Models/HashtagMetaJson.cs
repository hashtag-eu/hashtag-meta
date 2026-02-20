using HashtagMeta.Core.DnProto;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace HashtagMeta.Core.Models;


public record HashtagMetaJson {
    public static readonly JsonSerializerOptions HashtagSerializeOptions = new() {
        AllowTrailingCommas = true,
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        IndentSize = 2,
        WriteIndented = true,
        NewLine = "\x0A",
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    [JsonPropertyName("data")]
    public HashtagData Data { get; set; } = new();
    [JsonPropertyName("sig")]
    public byte[] Signature { get; set; } = [];

    public string ToJson() => JsonSerializer.Serialize(this, HashtagSerializeOptions);

    public static HashtagMetaJson? FromJson(FileInfo fi) {
        using var si = fi.OpenRead();
        var htData = JsonSerializer.Deserialize<HashtagMetaJson>(si);
        return htData;
    }
    public static HashtagMetaJson? FromJson(string s) {
        var htData = JsonSerializer.Deserialize<HashtagMetaJson>(s);
        return htData;
    }
}

public record HashtagData {
    [JsonPropertyName("issuer")]
    public string Issuer { get; set; } = string.Empty;
    [JsonPropertyName("tags")]
    public Dictionary<string, string>? Tags { get; set; }
    [JsonPropertyName("source")]
    public string? Source { get; set; }
    [JsonPropertyName("sourceCid")]
    public string? SourceCID { get; set; }
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
        var dataJson = JsonSerializer.Serialize(this);
        var dagCbor = DagCborObject.FromJsonString(dataJson);
        var byteData = dagCbor.ToBytes();
        var hash = SHA256.HashData(byteData);

        return hash;
    }
}

public record HashtagFile {
    [JsonPropertyName("cid")]
    public string FileCID { get; set; } = string.Empty;
}
