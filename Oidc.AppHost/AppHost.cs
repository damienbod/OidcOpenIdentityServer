var builder = DistributedApplication.CreateBuilder(args);

const string IDENTITY_PROVIDER = "idp";

IResourceBuilder<ProjectResource>? idp = null;
IResourceBuilder<ProjectResource>? ui = null;
IResourceBuilder<ProjectResource>? api = null;
IResourceBuilder<ProjectResource>? migrationServiceIdp = null;

// To add
//var stsOidcWebClientPublicPemBase64 = builder.AddParameter("StsOidcWebClientPublicPemBase64");
//var webOidcClientPrivatePemBase64 = builder.AddParameter("WebOidcClientPrivatePemBase64", secret: true);
//var webOidcClientPublicPemBase64 = builder.AddParameter("WebOidcClientPublicPemBase64");
//var webDpopClientPrivatePemBase64 = builder.AddParameter("WebDpopClientPrivatePemBase64", secret: true);
//var webDpopClientPublicPemBase64 = builder.AddParameter("WebDpopClientPublicPemBase64");

//var stsSigningPrivatePemBase64 = builder.AddParameter("StsSigningPrivatePemBase64", secret: true);
//var stsSigningPublicPemBase64 = builder.AddParameter("StsSigningPublicPemBase64");

var rsa256OidcPrivatePem = builder.AddParameter("Rsa256OidcPrivatePem", secret: true);
var rsa256OidcPublicPem = builder.AddParameter("Rsa256OidcPublicPem");
var webOidcClientId = builder.AddParameter("WebOidcClientId");
var webOidcClientSecret = builder.AddParameter("WebOidcClientSecret", secret: true);

var stsSigningPrivatePem = builder.AddParameter("StsSigningPrivatePem", secret: true);
var stsSigningPublicPem = builder.AddParameter("StsSigningPublicPem");

var env = builder.AddAzureContainerAppEnvironment("env");

var sqlServer = builder.AddAzureSqlServer("sqlserver")
    .ConfigureInfrastructure(infra => // Add when deploying
    {
        var resources = infra.GetProvisionableResources();

        foreach (var dbRes in resources.OfType<Azure.Provisioning.Sql.SqlDatabase>())
        {
            dbRes.Sku = new Azure.Provisioning.Sql.SqlSku()
            {
                Tier = "Basic",
                Name = "Basic",
                Capacity = 5
            };
            dbRes.UseFreeLimit = false;
        }
    });

var databaseIdp = sqlServer.AddDatabase("db-idp", "Idp");

var mailjetApiSecret = builder.AddParameter("MailjetApiSecret", secret: true);
var mailjetApiKey = builder.AddParameter("MailjetApiKey", secret: true);

var sendGridApiKey = builder.AddParameter("SendGridApiKey", secret: true);
var notificationFromEmail = builder.AddParameter("NotificationFromEmail");

var mailomatApiKey = builder.AddParameter("MailomatApiKey", secret: true);

migrationServiceIdp = builder.AddProject<Projects.Idp_Domain_Migrations>("migrations-idp")
    .WithReference(databaseIdp)
    .WaitFor(sqlServer);

// OIDC web endpoints
var webOidcAuthority = builder.AddParameter("WebOidcAuthority");
var uiUrl = builder.AddParameter("UiUrl");

idp = builder.AddProject<Projects.Idp>(IDENTITY_PROVIDER)
    .WithExternalHttpEndpoints()
    .WithReference(databaseIdp)
    .WaitForCompletion(migrationServiceIdp)
    .WithEnvironment("WebOidcAuthority", webOidcAuthority)
    .WithEnvironment("UiUrl", uiUrl)
    .WithEnvironment("WebOidcClientId", webOidcClientId)
    .WithEnvironment("Rsa256OidcPublicPem", rsa256OidcPublicPem)
    .WithEnvironment("StsSigningPrivatePem", stsSigningPrivatePem)
    .WithEnvironment("StsSigningPublicPem", stsSigningPublicPem)
    .WithEnvironment("MailomatApiKey", mailomatApiKey)
    .WithHttpHealthCheck("/healthcheck");

if (builder.ExecutionContext.IsRunMode)
{
    var password = builder.AddParameter("sql-password", secret: true);
    sqlServer.RunAsContainer(container =>
    {
        container.WithDataVolume();
        container.WithHostPort(50001);
        container.WithPassword(password);
    });
}

api = builder.AddProject<Projects.WebApi>("api-service")
    .WithExternalHttpEndpoints()
    .WithEnvironment("WebOidcAuthority", webOidcAuthority)
    .WithHttpHealthCheck("/health")
    .WaitFor(idp);

ui = builder.AddProject<Projects.Ui>("ui")
    .WithExternalHttpEndpoints()
    .WithEnvironment("WebOidcClientId", webOidcClientId)
    .WithEnvironment("WebOidcAuthority", webOidcAuthority)
    .WithEnvironment("WebOidcClientSecret", webOidcClientSecret)
    .WithEnvironment("Rsa256OidcPrivatePem", rsa256OidcPrivatePem)
    .WithEnvironment("Rsa256OidcPublicPem", rsa256OidcPublicPem)
    .WithHttpHealthCheck("/health")
    .WithReference(api)
    .WaitFor(idp);



builder.Build().Run();
