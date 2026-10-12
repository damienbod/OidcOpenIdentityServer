using Duende.AccessTokenManagement;
using Duende.AccessTokenManagement.OpenIdConnect;
using Duende.IdentityModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using NetEscapades.AspNetCore.SecurityHeaders.Infrastructure;
using Oidc.ServiceDefaults;
using System.Security.Cryptography.X509Certificates;
using Ui;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

var webDpopClientPrivatePem = ConfigConverter.GetPemFromBase64Config("WebDpopClientPrivatePemBase64", builder.Configuration);
var webDpopClientPublicPem = ConfigConverter.GetPemFromBase64Config("WebDpopClientPublicPemBase64", builder.Configuration);

var ecdsaCertificate = X509Certificate2.CreateFromPem(webDpopClientPublicPem, webDpopClientPrivatePem);
var ecdsaCertificateKey = new ECDsaSecurityKey(ecdsaCertificate.GetECDsaPrivateKey());

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
    options.DefaultSignOutScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie(options =>
{
    options.Cookie.Name = "__Host-Http-ui-web";
    options.Cookie.SameSite = SameSiteMode.Lax;
    // can be strict if same-site
    //options.Cookie.SameSite = SameSiteMode.Strict;
})
.AddOpenIdConnect(options =>
{
    options.Events = OidcEventHandlers.OidcEvents(builder.Configuration);

    options.ClientId = builder.Configuration["WebOidcClientId"];
    options.Authority = builder.Configuration["WebOidcAuthority"];
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.ResponseType = OpenIdConnectResponseType.Code;

    // client_assertion used, set in oidc events
    //options.ClientSecret = "test";

    options.SaveTokens = true;

    options.GetClaimsFromUserInfoEndpoint = true;
    options.MapInboundClaims = false;

    options.ClaimActions.MapUniqueJsonKey("loa", "loa");
    options.ClaimActions.MapUniqueJsonKey("loi", "loi");
    options.ClaimActions.MapUniqueJsonKey(JwtClaimTypes.Email, JwtClaimTypes.Email);

    // OAuth PAR not supported
    //options.PushedAuthorizationBehavior = PushedAuthorizationBehavior.Require;

    options.Scope.Add("scope-api");
    options.TokenValidationParameters = new TokenValidationParameters
    {
        NameClaimType = "name"
    };
});


builder.Services.AddOpenIdConnectAccessTokenManagement();

// If you want to use named clients
builder.Services.AddUserAccessTokenHttpClient("profileClient",
    configureClient: client =>
    {
        // Aspire configuration, see AppHost
        client.BaseAddress = new("https+http://api-service");
    });

var requireAuthPolicy = new AuthorizationPolicyBuilder()
    .RequireAuthenticatedUser()
    .Build();

builder.Services.AddAuthorizationBuilder()
    .SetFallbackPolicy(requireAuthPolicy);

builder.Services.AddScoped<PhotoService>();
builder.Services.AddHttpClient();

builder.Services.AddRazorPages();

builder.Services.AddSecurityHeaderPolicies()
    .SetPolicySelector((PolicySelectorContext ctx) =>
    {
        return SecurityHeadersDefinitions.GetHeaderPolicyCollection(
            builder.Environment.IsDevelopment(),
            builder.Configuration["WebOidcAuthority"]);
    });

var app = builder.Build();

app.MapDefaultEndpoints();

//IdentityModelEventSource.ShowPII = true;
JsonWebTokenHandler.DefaultInboundClaimTypeMap.Clear();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Error");
}

app.UseSecurityHeaders();

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapRazorPages();

app.Run();
