using CodeBreaker.Shared.Models.Data;
using System.ComponentModel.DataAnnotations;

namespace CodeBreaker.Shared.Models.Api;

public readonly record struct MoveDto<TField>(
    int MoveNumber,
    IReadOnlyList<TField> GuessPegs,
    KeyPegs? KeyPegs
);

public readonly record struct CreateMoveResponse(
    KeyPegs KeyPegs,
    bool Ended,
    bool Won
);

public readonly record struct CreateMoveRequest(IReadOnlyList<string> GuessPegs);