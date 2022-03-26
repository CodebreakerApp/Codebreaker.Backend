using MMBot;

using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("MMBot.Tests")]

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddHttpClient<MMGameRunner>(options =>
{
    options.BaseAddress = new Uri("https://localhost:7053/api/MasterMind/");
});
builder.Services.AddScoped<MMTimer>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/start", (MMTimer timer, int? delayseconds, int? loops) =>
{
    string id = timer.Start(delayseconds ?? 60, loops ?? 3);

    return Results.Accepted($"/status/{id}", id);
});

app.MapGet("/status/{id}", (string id) =>
{
    string status = MMTimer.Status(id);
    return Results.Ok(status);
});

app.MapGet("/stop/{id}", (string id) =>
{
    string result = MMTimer.Stop(id);
    return Results.Ok(result);
});


app.Run();
