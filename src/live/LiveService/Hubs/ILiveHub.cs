// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace LiveService.Hubs;

public interface ILiveHub
{
    Task GameCreated(GameCreatedArgs args);

    Task GameCancelled(GameCancelledArgs args);

    Task GameDeleted(GameDeletedArgs args);

    Task GameFinished(GameFinishedArgs args);

    Task MoveCreated(MoveCreatedArgs args);
}
