using Microsoft.Extensions.Options;
using System.Net.Http.Headers;

namespace Ui;

public class PhotoService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly IConfiguration _configuration;

    public PhotoService(IConfiguration configuration,
        IHttpClientFactory clientFactory)
    {
        _configuration = configuration;
        _clientFactory = clientFactory;
    }

    /// <summary>
    /// HttpContext is used to get the access token and it is passed as a parameter
    /// </summary>
    public async Task<string> GetPhotoAsync(string accessToken)
    {
        try
        {
            var client = _clientFactory.CreateClient();
            // Aspire configuration, see AppHost
            client.BaseAddress = new("https+http://api-service");
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await client.GetAsync("api/Profiles/photo");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();

                if (data != null)
                    return data;

                return string.Empty;
            }

            throw new ApplicationException($"Status code: {response.StatusCode}, Error: {response.ReasonPhrase}");
        }
        catch (Exception e)
        {
            throw new ApplicationException($"Exception {e}");
        }
    }

    /// <summary>
    /// Accesses API using named client and access token 
    /// </summary>

    public async Task<string> GetPhotoAsync()
    {
        try
        {
            var client = _clientFactory.CreateClient("profileClient");

            var response = await client.GetAsync("api/Profiles/photo");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadAsStringAsync();

                if (data != null)
                    return data;

                return string.Empty;
            }

            throw new ApplicationException($"Status code: {response.StatusCode}, Error: {response.ReasonPhrase}");
        }
        catch (Exception e)
        {
            throw new ApplicationException($"Exception {e}");
        }
    }
}