
using CodeBreaker.Bot;

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MMBot.Tests")]

var builder = WebApplication.CreateBuilder(args);
WebApplication? app = null;

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<CodeBreakerGameRunner>(options =>
{
    // running with tye?
    Uri? apiUri = builder.Configuration.GetServiceUri("codebreaker-apis");
    
    if (apiUri is null)
    {
        string? codebreakeruri = builder.Configuration.GetSection("CodeBreakerBot")["APIUri"];
        if (codebreakeruri is null) throw new InvalidOperationException("APIURI not retrieved");

        apiUri = new Uri(codebreakeruri);
    }
    Uri uri = new(apiUri, "v1/");
    app?.Logger.LogInformation("Using URI {uri} to access the API service", uri);
    options.BaseAddress = uri;
});
builder.Services.AddScoped<CodeBreakerTimer>();

app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/start", (CodeBreakerTimer timer, int? delayseconds, int? loops) =>
{
    string id = timer.Start(delayseconds ?? 60, loops ?? 3);

    return Results.Accepted($"/status/{id}", id);
});

app.MapGet("/status/{id}", (string id) =>
{
    string status = CodeBreakerTimer.Status(id);
    return Results.Ok(status);
});

app.MapGet("/stop/{id}", (string id) =>
{
    string result = CodeBreakerTimer.Stop(id);
    return Results.Ok(result);
});

app.Run();
