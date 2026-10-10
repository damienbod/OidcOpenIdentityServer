namespace Idp.Services;

public class EmailSettings
{
    public string Url { get; set; } = "https://api.mailomat.swiss/";
    public string MailomatApiKey { get; set; } = null!;
    public string SenderEmail { get; set; } = null!;
    public string Sender { get; set; } = "damien_bod@hotmail.com";
}
