﻿namespace CodeBreaker.Bot;

public class CodeBreakerGameRunner
{
    private readonly HttpClient _httpClient;
    private string? _gameId;
    private int _moveNumber = 0;
    private readonly List<Move> _moves = new();
    private List<int>? _possibleValues;
    private readonly ILogger _logger;

    public CodeBreakerGameRunner(HttpClient httpClient, ILogger<CodeBreakerGameRunner> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    private List<int> InitializePossibleValues()
    {
        static List<int> Create8Colors(int shift)
        {
            List<int> pin = new();
            for (int i = 0; i < 6; i++)
            {
                int x = 1 << i + shift;
                pin.Add(x);
            }
            return pin;
        }

        static List<int> AddColorsToList(List<int> list1, List<int> list2)
        {
            List<int> result = new(capacity: 1300);
            for (int i = 0; i < list1.Count; i++)
            {
                for (int j = 0; j < list2.Count; j++)
                {
                    int x = list1[i] ^ list2[j];
                    result.Add(x);
                }
            }
            return result;
        }

        var digits1 = Create8Colors(0);
        var digits2 = Create8Colors(6);
        var list2 = AddColorsToList(digits1, digits2);
        var digits3 = Create8Colors(12);
        var list3 = AddColorsToList(list2, digits3);
        var digits4 = Create8Colors(18);
        var list4 = AddColorsToList(list3, digits4);
        list4.Sort();
        return list4;
    }

    public async Task StartGameAsync()
    {
        _possibleValues = InitializePossibleValues();
        _moves.Clear();

        CreateGameRequest request = new("Bot");

        var responseMessage = await _httpClient.PostAsJsonAsync("start", request);
        responseMessage.EnsureSuccessStatusCode();
        var response = await responseMessage.Content.ReadFromJsonAsync<CreateGameResponse>();
        _moveNumber = 1;
        _gameId = response.Id;
    }

    /// <summary>
    /// Sends moves to the API server
    /// Moves are delayed by thinkSeconds before setting the next move
    /// Finishes when the game is over
    /// </summary>
    /// <param name="thinkSeconds">The seconds to simulate thinking before setting the next move</param>
    /// <returns>a task</returns>
    /// <exception cref="InvalidOperationException">throws if initialization was not done, or with invalid game state</exception>
    public async Task RunAsync(int thinkSeconds)
    {
        if (_gameId is null) throw new InvalidOperationException($"call {nameof(StartGameAsync)} before");
        if (_possibleValues is null) throw new InvalidOperationException($"call {nameof(StartGameAsync)} before");

        MoveResponse response;

        do
        {
            (var colorNames, var selection) = GetNextMoves();
            MoveRequest moveRequest = new(_gameId, _moveNumber++, colorNames);
            _logger.SendMove(moveRequest.ToString(), _gameId);

            var responseMessage = await _httpClient.PostAsJsonAsync("move", moveRequest);
            responseMessage.EnsureSuccessStatusCode();
            response = await responseMessage.Content.ReadFromJsonAsync<MoveResponse>();

            if (response.Won)
            {
                _logger.Matched(_moveNumber, _gameId);
                break;
            }

            int blackHits = response.KeyPegs?.Count(s => s == "black") ?? 0;
            if (blackHits >= 4)
            {
                throw new InvalidOperationException($"4 or more blacks but won was not set: {blackHits}");
            }

            int whiteHits = response.KeyPegs?.Count(s => s == "white") ?? 0;
            if (whiteHits > 4)
            {
                throw new InvalidOperationException($"more than 4 whites is not possible: {whiteHits}");
            }

            if (blackHits > 0)
            {
                _possibleValues = _possibleValues.HandleBlackMatches(blackHits, selection);
                _logger.ReducedPossibleValues(_possibleValues.Count, "black", _gameId);
            }
            if (whiteHits > 0)
            {
                _possibleValues = _possibleValues.HandleWhiteMatches(whiteHits + blackHits, selection);
                _logger.ReducedPossibleValues(_possibleValues.Count, "white", _gameId);
            }

            await Task.Delay(TimeSpan.FromSeconds(thinkSeconds));  // thinking delay
        } while (_moveNumber > 12 || !response.Won);

        _logger.FinishedRun(_moveNumber, _gameId);
    }

    /// <summary>
    /// Get the values for the next move
    /// </summary>
    /// <returns>A string and int representation for the next moves</returns>
    /// <exception cref="InvalidOperationException">Throws if there are no calculated possible values left to chose from</exception>
    private (string[] Colors, int Selection) GetNextMoves()
    {
        if (_possibleValues?.Count is null or 0) throw new InvalidOperationException("invalid number of possible values - 0");

        int random = Random.Shared.Next(_possibleValues.Count);
        var value = _possibleValues[random];

        return (value.IntToColors(), value);
    }
}

public enum CodeColors
{
    black = 1,
    white = 2,
    red = 4,
    green = 8,
    blue = 16,
    yellow = 32
}

public enum KeyColors
{
    black,
    white
}

public record struct CodePeg(string Color)
{
    public override string ToString() => Color;
}

public record struct KeyPeg(string Color)
{
    public override string ToString() => Color;
}

public record struct Move(string GameId, int MoveNumber, IList<CodePeg> Codes, IList<KeyPeg> Keys);
