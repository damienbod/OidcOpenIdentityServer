using Idp.Domain.Migrations;

var builder = Host.CreateApplicationBuilder(args);

builder.AddServiceDefaults();
builder.AddApplicationDbContext("db-idp-org-mgmt");

builder.Services.AddHostedService<Worker>();

builder.Services.AddOpenTelemetry()
    .WithTracing(tracing => tracing.AddSource(Worker.ActivitySourceName));

var host = builder.Build();
await host.RunAsync();
