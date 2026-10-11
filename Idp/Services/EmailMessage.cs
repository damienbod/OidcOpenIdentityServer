using System.Text.Json.Serialization;

namespace Idp.Services;

public class EmailMessage
{
    [JsonPropertyName("email")]
    public string Email { get; set; } = "damien_bod@hotmail.com";
    [JsonPropertyName("name")]
    public string Name { get; set; } = null!;
}
