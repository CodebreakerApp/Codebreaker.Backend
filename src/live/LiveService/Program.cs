// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using LiveService.Hubs;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

builder.Services.AddSignalR().AddAzureSignalR();

WebApplication app = builder.Build();

app.MapHub<LiveHub>("/live");

app.Run();
