using CodeBreaker.LiveService.Extensions;
using CodeBreaker.LiveService.Options;
using LiveService;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
builder.Services.AddSignalR().AddAzureSignalRIfConfigured(builder.Configuration);
WebApplication app = builder.Build();

app.MapHub<LiveHub>("/live");
app.Run();
