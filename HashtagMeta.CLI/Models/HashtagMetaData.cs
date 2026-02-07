using System.Text.Json.Serialization;

namespace HashtagMeta.CLI.Models; 
public record HashtagMetaJson {
    [JsonPropertyName("data")]
    public HashtagData? Data { get; set; }
    [JsonPropertyName("sig")]
    public string? Signature { get; set; }
}

public record HashtagData {
    [JsonPropertyName("issuer")]
    public string? Issuer { get; set; }
    [JsonPropertyName("tags")]
    public Dictionary<string, string>? Tags { get; set; }
    [JsonPropertyName("source")]
    public string? Source { get; set; }
    [JsonPropertyName("sourceCid")]
    public string? SourceCID { get; set; }
    [JsonPropertyName("files")]
    public List<HashtagFile> Files { get; set; } = [];
}

public record HashtagFile {
    [JsonPropertyName("fileName")]
    public string? FileName { get; set; }
    [JsonPropertyName("fileCid")]
    public string? FileCID { get; set; }
}
