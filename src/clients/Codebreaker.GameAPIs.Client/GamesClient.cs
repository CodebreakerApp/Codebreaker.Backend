namespace Codebreaker.GameAPIs.Client;

/// <summary>
/// Client to interact with the Codebreaker Game API.
/// </summary>
public class GamesClient(HttpClient httpClient, ILogger<GamesClient> logger) : IGamesClient
{
    internal const string ActivitySourceName = "Codebreaker.GameAPIs.Client";
    internal const string Version = "1.0.0";
    internal static ActivitySource ActivitySource { get; } = new ActivitySource(ActivitySourceName, Version);

    private readonly static JsonSerializerOptions s_jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Starts a new game.
    /// </summary>
    /// <param name="gameType">The game type with one of the <see cref="GameType"/>enum values</param>
    /// <param name="playerName">The name of the player</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the request early</param>
    /// <returns>A tuple with the unique game id, the number of codes that need to be filled, the maximum available moves, and possible field values for guesses</returns>
    /// <exception cref="InvalidOperationException"></exception>
    /// <exception cref="HttpRequestException"></exception>"
    public async Task<(Guid Id, int NumberCodes, int MaxMoves, IDictionary<string, string[]> FieldValues)>
        StartGameAsync(GameType gameType, string playerName, CancellationToken cancellationToken = default)
    {
        using Activity? activity = ActivitySource.StartActivity("StartGameAsync", ActivityKind.Client);
        try
        {
            CreateGameRequest createGameRequest = new(gameType, playerName);
            var response = await httpClient.PostAsJsonAsync("/games", createGameRequest, s_jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            var gameResponse = await response.Content.ReadFromJsonAsync<CreateGameResponse>(s_jsonOptions, cancellationToken) ?? throw new InvalidOperationException();

            logger.GameCreated(gameResponse.Id);
            activity?.GameCreatedEvent(gameResponse.Id.ToString(), gameResponse.GameType.ToString());
            return (gameResponse.Id, gameResponse.NumberCodes, gameResponse.MaxMoves, gameResponse.FieldValues);
        }
        catch (HttpRequestException ex)
        {
            logger.StartGameError(ex.Message, ex);
            activity?.ErrorEvent(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Cancels a game.
    /// </summary>
    /// <param name="id">The unique identifier of the game</param>
    /// <param name="playerName">The name of the player</param>
    /// <param name="gameType">The game type with one of the <see cref="GameType"/>enum values</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the request early</param>
    /// <exception cref="HttpRequestException"></exception>
    public async Task CancelGameAsync(Guid id, string playerName, GameType gameType, CancellationToken cancellationToken = default)
    {
        using Activity? activity = ActivitySource.StartActivity("CancelGameAsync", ActivityKind.Client);
        try
        {
            var request = new UpdateGameRequest(id, gameType, playerName, 0, true);
            var response = await httpClient.PatchAsJsonAsync($"/games/{id}", request, s_jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            logger.GameCanceled(id);
            activity?.GameCanceledEvent(id.ToString());
        }
        catch (HttpRequestException ex)
        {
            logger.CancelGameError(ex.Message, ex);
            activity?.ErrorEvent(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Cancels and reveals the details of a game based on the specified parameters.
    /// </summary>
    /// <param name="id">The unique identifier of the game to reveal.</param>
    /// <param name="playerName">The name of the player requesting the game details. Cannot be null or empty.</param>
    /// <param name="gameType">The type of the game to reveal.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. Optional.</param>
    /// <returns>A <see cref="GameInfo"/> object containing the details of the revealed game.</returns>
    /// <exception cref="HttpRequestException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<GameInfo> RevealGameAsync(Guid id, string playerName, GameType gameType, CancellationToken cancellationToken = default)
    {
        using Activity? activity = ActivitySource.StartActivity("RevealGameAsync", ActivityKind.Client);
        try
        {
            // First cancel/end the game
            var request = new UpdateGameRequest(id, gameType, playerName, 0, true);
            var cancelResponse = await httpClient.PatchAsJsonAsync($"/games/{id}", request, s_jsonOptions, cancellationToken);
            cancelResponse.EnsureSuccessStatusCode();
            
            // Then get the full game details
            var gameResponse = await httpClient.GetFromJsonAsync<GameInfo>($"/games/{id}", s_jsonOptions, cancellationToken) 
                ?? throw new InvalidOperationException($"Could not retrieve game with ID {id}");
            
            int moveCount = gameResponse.Moves?.Count ?? 0;
            logger.GameRevealed(id, moveCount);
            activity?.GameRevealedEvent(id.ToString(), gameType.ToString());
            
            return gameResponse;
        }
        catch (HttpRequestException ex)
        {
            logger.RevealGameError(ex.Message, ex);
            activity?.ErrorEvent(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Set a game move by supplying guess pegs. This method returns the results of the move (the key pegs), and whether the game ended, and whether the game was won.
    /// </summary>
    /// <param name="id">The game id received from StartGameAsync</param>
    /// <param name="playerName">The player name (needs to be the same as received). This must match with the game started.</param>
    /// <param name="gameType">The game type with one of the <see cref="GameType"/>enum values. This must match with the game started.</param>
    /// <param name="moveNumber">The incremented move number. The game analyzer returns an error if this does not match the state of the game.</param>
    /// <param name="guessPegs">The guess pegs for this move. The number of guess pegs must conform to the number codes returned when creating the game.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the request early.</param>
    /// <returns>A tuple with the results of the move (the key pegs), whether the game ended, and whether the game was won.</returns>
    /// <exception cref="HttpRequestException"></exception>
    /// <exception cref="InvalidOperationException"></exception>
    public async Task<(string[] Results, bool Ended, bool IsVictory)> SetMoveAsync(Guid id, string playerName, GameType gameType, int moveNumber, string[] guessPegs, CancellationToken cancellationToken = default)
    {
        using Activity? activity = ActivitySource.StartActivity("SetMoveAsync", ActivityKind.Client);
        try
        {
            UpdateGameRequest updateGameRequest = new(id, gameType, playerName, moveNumber)
            {
                GuessPegs = guessPegs
            };
            var response = await httpClient.PatchAsJsonAsync($"/games/{id}", updateGameRequest, s_jsonOptions, cancellationToken);
            response.EnsureSuccessStatusCode();
            var moveResponse = await response.Content.ReadFromJsonAsync<UpdateGameResponse>(s_jsonOptions, cancellationToken)
                ?? throw new InvalidOperationException();

            logger.MoveSet(id, moveResponse.MoveNumber);
            activity?.AddEvent(new ActivityEvent("MoveSet", 
                tags: [ 
                    new KeyValuePair<string, object?>("gameId", id.ToString()),
                    new KeyValuePair<string, object?>("moveNumber", moveResponse.MoveNumber)
                ]));

            (_, _, _, bool ended, bool isVictory, string[] results) = moveResponse;
            return (results, ended, isVictory);
        }
        catch (HttpRequestException ex)
        {
            logger.SetMoveError(ex.Message, ex);
            activity?.ErrorEvent(ex.Message);
            throw;
        }
    }

    /// <summary>
    /// Retrieves a game by ID.
    /// </summary>
    /// <param name="id">The unique identifier of a game.</param>
    /// <param name="cancellationToken">Optional cancellation token to cancel the request early.</param>
    /// <returns>The <see cref="GameInfo"/> if it exists, otherwise null.</returns>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<GameInfo?> GetGameAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using Activity? activity = ActivitySource.StartActivity("GetGameAsync", ActivityKind.Client);
        GameInfo? game;
        try
        {
            game = await httpClient.GetFromJsonAsync<GameInfo>($"/games/{id}", s_jsonOptions, cancellationToken);
            logger.GameReceived(id, game?.EndTime != null, game?.LastMoveNumber ?? 0);
        }
        catch (HttpRequestException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
        {
            logger.GetGameNotFound(id, ex.Message);
            return default;
        }
        catch (HttpRequestException ex)
        {
            logger.GetGameError(ex.Message, ex);
            activity?.ErrorEvent(ex.Message);
            throw;
        }
        return game;
    }

    /// <summary>
    /// Retrieves a list of games matching a specified query.
    /// </summary>
    /// <param name="query">The games query object containing parameters to filter games.</param>
    /// <param name="cancellationToken">Cancellation token to cancel the request early.</param>
    /// <returns>An IEnumerable collection of Game objects that match the specified query.</returns>
    /// <exception cref="HttpRequestException"></exception>
    public async Task<IEnumerable<GameInfo>> GetGamesAsync(GamesQuery query, CancellationToken cancellationToken = default)
    {
        using Activity? activity = ActivitySource.StartActivity("GetGamesAsync", ActivityKind.Client);
        try
        {
            string urlQuery = query.AsUrlQuery();
            IEnumerable<GameInfo> games = (await httpClient.GetFromJsonAsync<IEnumerable<GameInfo>>($"/games{urlQuery}", s_jsonOptions, cancellationToken)) ?? [];

            int gameCount = games.Count();
            logger.GamesReceived(urlQuery, gameCount);

            activity?.AddEvent(new ActivityEvent("GamesReceived",
                tags: [
                    new KeyValuePair<string, object?>("count", gameCount),
                    new KeyValuePair<string, object?>("query", urlQuery)
                ]));

            return games;
        }
        catch (HttpRequestException ex)
        {
            logger.GetGamesError(ex.Message, ex);
            activity?.ErrorEvent(ex.Message);
            throw;
        }
    }
}
