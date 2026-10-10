using System.Text.Json.Serialization;

namespace Idp.Services;

///{
///  "from": {
///    "email": "no-reply@verd.cash",
///    "name": "VERD.cash"
///  },
///  "to": [
///    {
///      "email": "laurent.christen@isolutions.ch",
///      "name": "Laurent Christen"
///    }
///  ],
///  "subject": "TEST",
///  "text": "test",
///  "html": "htmltext"
///}
public class MailomatMessage
{
    [JsonPropertyName("from")]
    public EmailMessage From { get; set; } = new();
    [JsonPropertyName("to")]
    public List<EmailMessage> To { get; set; } = [];
    [JsonPropertyName("subject")]
    public string Subject { get; set; } = null!;
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    [JsonPropertyName("html")]
    public string? Html { get; set; }
}
