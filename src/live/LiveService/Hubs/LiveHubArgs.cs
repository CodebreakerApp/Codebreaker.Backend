// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using CodeBreaker.Shared;

namespace LiveService.Hubs;

public record GameCreatedArgs(CodeBreakerGame Game);

public record GameCancelledArgs(CodeBreakerGame Game);

public record GameDeletedArgs(CodeBreakerGame Game);

public record GameFinishedArgs(CodeBreakerGame Game);

public record MoveCreatedArgs(CodeBreakerGame Game);
