using Duende.AccessTokenManagement;
using Duende.AccessTokenManagement.OpenIdConnect;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using NetEscapades.AspNetCore.SecurityHeaders.Infrastructure;
using Ui;
var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults();

builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
})
.AddCookie()
.AddOpenIdConnect(options =>
{
    options.SignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.Scope.Add("profile");
    options.Scope.Add("myscope");
    options.Scope.Add("offline_access");

    options.Authority = builder.Configuration["WebOidcAuthority"];
    options.ClientId = builder.Configuration["WebOidcClientId"];
    options.ClientSecret = builder.Configuration["WebOidcClientSecret"];

    options.ResponseType = OpenIdConnectResponseType.Code;
    options.MapInboundClaims = false;
    options.TokenValidationParameters.NameClaimType = "name";
    options.TokenValidationParameters.RoleClaimType = "role";

    options.SaveTokens = true;
    options.GetClaimsFromUserInfoEndpoint = true;
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
