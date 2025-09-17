using Codebreaker.GameAPIs.Client;
using Codebreaker.GameAPIs.Client.Models;

namespace CodeBreaker.BotWithString;

/// <summary>
/// Demonstrates how to use the string-based algorithms with the GameAPIs client
/// </summary>
public class StringBotGameRunner
{
    private readonly IGamesClient _gamesClient;

    public StringBotGameRunner(IGamesClient gamesClient)
    {
        _gamesClient = gamesClient;
    }

    /// <summary>
    /// Plays a simple game using string-based algorithms
    /// </summary>
    /// <param name="gameType">The type of game to play</param>
    /// <param name="playerName">The name of the player</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Game result information</returns>
    public async Task<GameResult> PlayGameAsync(GameType gameType, string playerName, CancellationToken cancellationToken = default)
    {
        // Start a new game
        var (gameId, numberCodes, maxMoves, fieldValues) = await _gamesClient.StartGameAsync(gameType, playerName, cancellationToken);

        // Get available colors/values for this game type
        string[] availableValues = fieldValues.ContainsKey("Colors")
            ? fieldValues["Colors"].ToArray()
            : fieldValues.Values.First().ToArray();

        // Generate all possible combinations for this game type
        List<string[]> possibleCombinations = StringCodeBreakerAlgorithms.GenerateAllPossibleCombinations(gameType, availableValues);

        int moveNumber = 1;
        bool gameWon = false;
        bool gameEnded = false;
        string[]? winningCombination = null;
        int actualMovesUsed = 0;

        while (!gameEnded && moveNumber <= maxMoves && possibleCombinations.Count > 0)
        {
            // For this simple implementation, just pick the first possible combination
            string[] guess = possibleCombinations[0];

            // Make the move
            var (results, ended, isVictory) = await _gamesClient.SetMoveAsync(
                gameId, playerName, gameType, moveNumber, guess, cancellationToken);

            actualMovesUsed = moveNumber; // Track actual moves used
            gameEnded = ended;
            gameWon = isVictory;

            if (isVictory)
            {
                winningCombination = guess;
                break;
            }

            // Parse results and filter possible combinations
            if (results.Length >= 2)
            {
                int blackHits = 0;
                int whiteHits = 0;
                int blueHits = 0;

                // Assuming results format: [black_hits, white_hits, ...]
                if (int.TryParse(results[0], out blackHits))
                {
                    possibleCombinations = possibleCombinations.HandleBlackMatches(gameType, blackHits, guess);
                }

                if (results.Length > 1 && int.TryParse(results[1], out whiteHits))
                {
                    possibleCombinations = possibleCombinations.HandleWhiteMatches(gameType, whiteHits, guess);
                }

                if (results.Length > 2 && int.TryParse(results[2], out blueHits))
                {
                    possibleCombinations = possibleCombinations.HandleBlueMatches(gameType, blueHits, guess);
                }

                // If no matches at all
                if (blackHits == 0 && whiteHits == 0 && (results.Length <= 2 || blueHits == 0))
                {
                    possibleCombinations = possibleCombinations.HandleNoMatches(gameType, guess);
                }
            }

            moveNumber++;
        }

        return new GameResult
        {
            GameId = gameId,
            GameType = gameType,
            PlayerName = playerName,
            MovesUsed = actualMovesUsed,
            MaxMoves = maxMoves,
            GameWon = gameWon,
            GameEnded = gameEnded,
            WinningCombination = winningCombination,
            RemainingPossibilities = possibleCombinations.Count
        };
    }
}

/// <summary>
/// Result of a game played by the string-based bot
/// </summary>
public record GameResult
{
    public required Guid GameId { get; init; }
    public required GameType GameType { get; init; }
    public required string PlayerName { get; init; }
    public required int MovesUsed { get; init; }
    public required int MaxMoves { get; init; }
    public required bool GameWon { get; init; }
    public required bool GameEnded { get; init; }
    public required string[]? WinningCombination { get; init; }
    public required int RemainingPossibilities { get; init; }
}