using Microsoft.Extensions.Options;

namespace Idp.Services;

public class EmailSender : Microsoft.AspNetCore.Identity.UI.Services.IEmailSender
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmailSender> _logger;
    private readonly EmailSettings _settings;

    public EmailSender(IHttpClientFactory clientFactory, IOptions<EmailSettings> settings,
        ILogger<EmailSender> logger)
    {
        _httpClient = clientFactory.CreateClient(Consts.EmailMailomat);
        _logger = logger;
        _settings = settings.Value;
    }

    private const string Message = "message";

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var mailomatMessage = new MailomatMessage
        {
            From = new EmailMessage
            {
                Email = _settings.SenderEmail,
                Name = _settings.Sender
            },
            To =
            [
                new EmailMessage
                {
                    Email = email,
                    Name = email,
                }
            ],
            Subject = subject,
            Html = htmlMessage
        };

        var result = await _httpClient.PostAsJsonAsync(Message, mailomatMessage);

        if (result.IsSuccessStatusCode)
        {
            await result.Content.ReadAsStringAsync();
        }
        else
        {
            _logger.LogWarning("Error sending Email, {ReasonPhrase}", result.ReasonPhrase);
        }
    }
}
