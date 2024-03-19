﻿namespace Codebreaker.GameAPIs.Client.Models;

/// <summary>
/// Information about the game that was created
/// </summary>
/// <param name="Id">The unique identifier of the game. This is needed to reference the game.</param>
/// <param name="GameType">The game type with one of the <see cref="GameType"/>enum values</param>
/// <param name="PlayerName">The name of the player</param>
/// <param name="NumberCodes">The number of the codes the player needs to fill</param>
/// <param name="MaxMoves">The maximum number of moves the game ends when its not solved</param>
/// <param name="FieldValues">A list of possible field values the user has to choose from</param>
internal record class CreateGameResponse(
    Guid Id,
    GameType GameType,
    string PlayerName,
    int NumberCodes,
    int MaxMoves,
    IDictionary<string, string[]> FieldValues);