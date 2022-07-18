// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.AspNetCore.SignalR;
using static CodeBreaker.LiveService.Shared.LiveEventNames;

namespace LiveService.Hubs;

public class LiveHub : Hub, ILiveHub
{
    public override Task OnConnectedAsync() =>
        Clients.Caller.SendAsync("test");

    public Task GameCreated(GameCreatedArgs args) =>
        Clients.All.SendAsync(GameCreatedEventName, new GameCreatedPayload(args.Game));

    public Task GameCancelled(GameCancelledArgs args) =>
        Clients.All.SendAsync(GameCancelledEventName, new GameCancelledPayload(args.Game));

    public Task GameDeleted(GameDeletedArgs args) =>
        Clients.All.SendAsync(GameDeletedEventName, new GameDeletedPayload(args.Game));

    public Task GameFinished(GameFinishedArgs args) =>
        Clients.All.SendAsync(GameFinishedEventName, new GameFinishedPayload(args.Game));

    public Task MoveCreated(MoveCreatedArgs args) =>
        Clients.All.SendAsync(MoveCreatedEventName, new MoveCreatedPayload(args.Game));
}
