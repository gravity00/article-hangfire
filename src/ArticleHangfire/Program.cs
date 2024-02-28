using Hangfire;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddHangfire(config => config
    .UseInMemoryStorage()
);
builder.Services.AddHangfireServer();

var app = builder.Build();

app.MapHangfireDashboard("");

app.Run();