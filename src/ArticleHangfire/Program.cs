using ArticleHangfire;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire(config => config
    .UseInMemoryStorage()
);
builder.Services.AddHangfireServer();

builder.Services.Configure<HttpJobOptions>(
    builder.Configuration.GetSection("HttpJobs")
);

builder.Services.AddHttpClient();
builder.Services.AddTransient<HttpJobRunner>();

builder.Services.AddHostedService<HttpJobHostedService>();

var app = builder.Build();

app.MapGet("/api/is-alive", () => "I'm alive!");

app.MapHangfireDashboard("");

app.Run();