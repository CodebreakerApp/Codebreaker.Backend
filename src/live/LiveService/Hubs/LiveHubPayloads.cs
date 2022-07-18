// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CodeBreaker.Shared;

namespace LiveService.Hubs;

internal record GameCreatedPayload(CodeBreakerGame Game);

internal record GameCancelledPayload(CodeBreakerGame Game);

internal record GameDeletedPayload(CodeBreakerGame Game);

internal record GameFinishedPayload(CodeBreakerGame Game);

internal record MoveCreatedPayload(CodeBreakerGame Game);
