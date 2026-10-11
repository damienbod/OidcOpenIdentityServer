using Open.IdentityServer;
using Open.IdentityServer.Models;
using System.Security.Cryptography.X509Certificates;

namespace Idp;

public static class Config
{
    public static IEnumerable<IdentityResource> IdentityResources =>
        [
            new IdentityResources.OpenId(),
            new IdentityResources.Profile(),
        ];

    public static IEnumerable<ApiScope> ApiScopes =>
        [
            //new ApiScope("scope2")
        ];

    public static IEnumerable<ApiResource> GetApiResources()
    {
        return
        [
            //new ApiResource("dpop-api", "DPoP API")
            //{
            //    Scopes = { "scope2" }
            //},
        ];
    }

    public static IEnumerable<Client> Clients(IWebHostEnvironment environment, IConfiguration configuration)
    {
        var uiUrl = configuration.GetValue<string>("UiUrl");
        var rsa256OidcPublicPem = configuration.GetValue<string>(Consts.Rsa256OidcPublicPem);
        var rsaCertificate = X509Certificate2.CreateFromPem(rsa256OidcPublicPem);
        var webOidcClientId = configuration.GetValue<string>(Consts.WebOidcClientId);

        // interactive client using code flow + pkce + par 
        return [
            new Client
            {
                ClientId = webOidcClientId!,
                ClientSecrets =
                {
                        //new Secret("test".Sha256()),
                        new Secret
                        {
                            // X509 cert base64-encoded
                            Type = IdentityServerConstants.SecretTypes.X509CertificateBase64,
                            Value = Convert.ToBase64String(rsaCertificate.GetRawCertData())
                        }
                },
                //RequireDPoP = true,
                RequirePushedAuthorization = true,

                AllowedGrantTypes = GrantTypes.Code,
                AlwaysIncludeUserClaimsInIdToken = true,

                RedirectUris = { $"{uiUrl}/signin-oidc" },
                FrontChannelLogoutUri = $"{uiUrl}/signout-oidc",
                PostLogoutRedirectUris = { $"{uiUrl}/signout-callback-oidc" },

                AllowOfflineAccess = true,
                AllowedScopes = { "openid", "profile", "offline_access" }
            }
        ];
    }
}
