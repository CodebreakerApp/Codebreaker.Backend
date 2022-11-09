// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CodeBreaker.Shared.Models.Live;

namespace CodeBreaker.LiveService;
public interface IEventSourceService
{
    IAsyncEnumerable<LiveHubArgs> SubscribeAsync(CancellationToken token = default);
}
