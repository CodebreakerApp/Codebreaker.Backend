using Codebreaker.GameAPIs.Client.Models;

namespace CodeBreaker.Bot;

public class CodeBreakerGameRunner(IGamesClient gamesClient, ILogger<CodeBreakerGameRunner> logger)
{
    private const string PlayerName = "Bot";
    private Guid? _gameId;
    private int _moveNumber = 0;
    private readonly List<Move> _moves = [];
    private List<int>? _possibleValues;
    private Dictionary<int, string>? _colorNames;
    private readonly IGamesClient _gamesClient = gamesClient;

    // initialize a list of all the possible options using numbers for every color
    private List<int> InitializePossibleValues(GameType gameType)
    {
        return gameType switch
        {
            GameType.Game6x4 => InitializePossibleValues6x4(),
            GameType.Game8x5 => InitializePossibleValues8x5(), 
            GameType.Game5x5x4 => InitializePossibleValues5x5x4(),
            _ => InitializePossibleValues6x4() // default fallback
        };
    }

    private List<int> InitializePossibleValues6x4()
    {
        static List<int> CreateColors(int colorCount, int shift)
        {
            List<int> pin = [];
            for (int i = 0; i < colorCount; i++)
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

        var digits1 = CreateColors(6, 0);
        var digits2 = CreateColors(6, 6);
        var list2 = AddColorsToList(digits1, digits2);
        var digits3 = CreateColors(6, 12);
        var list3 = AddColorsToList(list2, digits3);
        var digits4 = CreateColors(6, 18);
        var list4 = AddColorsToList(list3, digits4);
        list4.Sort();
        return list4;
    }

    private List<int> InitializePossibleValues8x5()
    {
        static List<int> CreateColors(int colorCount, int shift)
        {
            List<int> pin = [];
            for (int i = 0; i < colorCount; i++)
            {
                int x = 1 << i + shift;
                pin.Add(x);
            }
            return pin;
        }

        static List<int> AddColorsToList(List<int> list1, List<int> list2)
        {
            List<int> result = new(capacity: 32768);
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

        // For 8x5, we need 4 bits per color to support 8 colors, 5 positions
        var digits1 = CreateColors(8, 0);
        var digits2 = CreateColors(8, 4);
        var list2 = AddColorsToList(digits1, digits2);
        var digits3 = CreateColors(8, 8);
        var list3 = AddColorsToList(list2, digits3);
        var digits4 = CreateColors(8, 12);
        var list4 = AddColorsToList(list3, digits4);
        var digits5 = CreateColors(8, 16);
        var list5 = AddColorsToList(list4, digits5);
        list5.Sort();
        return list5;
    }

    private List<int> InitializePossibleValues5x5x4()
    {
        static List<int> CreateCombinations(int combinationCount, int shift)
        {
            List<int> pin = [];
            for (int i = 0; i < combinationCount; i++)
            {
                int x = 1 << i + shift;
                pin.Add(x);
            }
            return pin;
        }

        static List<int> AddCombinationsToList(List<int> list1, List<int> list2)
        {
            List<int> result = new(capacity: 625 * 625);
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

        // For 5x5x4, we have 25 shape+color combinations, 4 positions
        // Use 5 bits per position to support 25 combinations
        var digits1 = CreateCombinations(25, 0);
        var digits2 = CreateCombinations(25, 5);
        var list2 = AddCombinationsToList(digits1, digits2);
        var digits3 = CreateCombinations(25, 10);
        var list3 = AddCombinationsToList(list2, digits3);
        var digits4 = CreateCombinations(25, 15);
        var list4 = AddCombinationsToList(list3, digits4);
        list4.Sort();
        return list4;
    }

    public async Task StartGameAsync(GameType gameType = GameType.Game6x4, CancellationToken cancellationToken = default)
    {
        static int NextKey(ref int key)
        {
            int next = key;
            key <<= 1;
            return next;
        }

        _possibleValues = InitializePossibleValues(gameType);
        _moves.Clear();
        _moveNumber = 0;

        (_gameId, _, _, IDictionary<string, string[]> fieldValues) = await _gamesClient.StartGameAsync(gameType, "Bot", cancellationToken);
        int key = 1;
        
        // Handle different field types based on game type
        if (gameType == GameType.Game5x5x4)
        {
            // For shape+color game, create combinations as the values
            var colors = fieldValues["colors"];
            var shapes = fieldValues["shapes"];
            _colorNames = [];
            foreach (var shape in shapes)
            {
                foreach (var color in colors)
                {
                    string combination = $"{shape};{color}";
                    _colorNames[NextKey(ref key)] = combination;
                }
            }
        }
        else
        {
            // For color-only games
            _colorNames = fieldValues["colors"]
                .ToDictionary(keySelector: c => NextKey(ref key), elementSelector: color => color);
        }
    }

    /// <summary>
    /// Sends moves to the API server
    /// Moves are delayed by thinkSeconds before setting the next move
    /// Finishes when the game is over
    /// </summary>
    /// <param name="gameType">The type of game being played</param>
    /// <param name="thinkSeconds">The seconds to simulate thinking before setting the next move</param>
    /// <returns>a task</returns>
    /// <exception cref="InvalidOperationException">throws if initialization was not done, or with invalid game state</exception>
    public async Task RunAsync(GameType gameType, int thinkSeconds, CancellationToken cancellationToken = default)
    {
        if (_possibleValues is null) 
            throw new InvalidOperationException($"call {nameof(StartGameAsync)} before");
        Guid gameId = _gameId ?? 
            throw new InvalidOperationException($"call {nameof(StartGameAsync)} before");

        int fieldsCount = GetFieldsCount(gameType);
        bool ended = false;
        do
        {
            _moveNumber++;
            (string[] guessPegs, int selection) = GetNextMoves(gameType);
            logger.SendMove(string.Join(':', guessPegs), gameId);

            (string[] results, ended, bool isVictory) = await _gamesClient.SetMoveAsync(gameId, PlayerName, gameType, _moveNumber, guessPegs, cancellationToken);

            if (isVictory)
            {
                logger.Matched(_moveNumber, gameId);
                break;
            }

            int blackHits = results.Count(c => c == "Black");
            int whiteHits = results.Count(c => c == "White");

            if (blackHits >= fieldsCount)
                throw new InvalidOperationException($"{fieldsCount} or more blacks but won was not set: {blackHits}");

            if (whiteHits > fieldsCount)
                throw new InvalidOperationException($"more than {fieldsCount} whites is not possible: {whiteHits}");

            if (blackHits == 0 && whiteHits == 0)
            {
                _possibleValues = _possibleValues.HandleNoMatches(gameType, selection);
                logger.ReducedPossibleValues(_possibleValues.Count, "none", gameId);
            }
            if (blackHits > 0)
            {
                _possibleValues = _possibleValues.HandleBlackMatches(gameType, blackHits, selection);
                logger.ReducedPossibleValues(_possibleValues.Count, "Black", gameId);
            }
            if (whiteHits > 0)
            {
                _possibleValues = _possibleValues.HandleWhiteMatches(gameType, whiteHits + blackHits, selection);
                logger.ReducedPossibleValues(_possibleValues.Count, "White", gameId);
            }

            await Task.Delay(TimeSpan.FromSeconds(thinkSeconds), cancellationToken);  // thinking delay
        } while (!ended);

        logger.FinishedRun(_moveNumber, gameId);
    }

    /// <summary>
    /// Get the values for the next move
    /// </summary>
    /// <param name="gameType">The type of game being played</param>
    /// <returns>A string and int representation for the next moves</returns>
    /// <exception cref="InvalidOperationException">Throws if there are no calculated possible values left to chose from</exception>
    private (string[] Colors, int Selection) GetNextMoves(GameType gameType)
    {
        if (_possibleValues?.Count is null or 0) 
            throw new InvalidOperationException("invalid number of possible values - 0");

        int random = Random.Shared.Next(_possibleValues.Count);
        int value = _possibleValues[random];

        return (IntToColors(gameType, value), value);
    }

    private string[] IntToColors(GameType gameType, int value) =>
        gameType switch
        {
            GameType.Game6x4 => IntToColors6x4(value),
            GameType.Game8x5 => IntToColors8x5(value),
            GameType.Game5x5x4 => IntToColors5x5x4(value),
            _ => IntToColors6x4(value)
        };

    private string[] IntToColors6x4(int value) =>
    [
        _colorNames?[value & 0b111111] ?? string.Empty,
        _colorNames?[(value >> 6) & 0b111111] ?? string.Empty,
        _colorNames?[(value >> 12) & 0b111111] ?? string.Empty,
        _colorNames?[(value >> 18) & 0b111111] ?? string.Empty
    ];

    private string[] IntToColors8x5(int value) =>
    [
        _colorNames?[value & 0b1111] ?? string.Empty,
        _colorNames?[(value >> 4) & 0b1111] ?? string.Empty,
        _colorNames?[(value >> 8) & 0b1111] ?? string.Empty,
        _colorNames?[(value >> 12) & 0b1111] ?? string.Empty,
        _colorNames?[(value >> 16) & 0b1111] ?? string.Empty
    ];

    private string[] IntToColors5x5x4(int value) =>
    [
        _colorNames?[value & 0b11111] ?? string.Empty,
        _colorNames?[(value >> 5) & 0b11111] ?? string.Empty,
        _colorNames?[(value >> 10) & 0b11111] ?? string.Empty,
        _colorNames?[(value >> 15) & 0b11111] ?? string.Empty
    ];

    private static int GetFieldsCount(GameType gameType) =>
        gameType switch
        {
            GameType.Game6x4 => 4,
            GameType.Game8x5 => 5,
            GameType.Game5x5x4 => 4,
            _ => 4
        };
}

public enum CodeColors
{
    Black = 1,
    White = 2,
    Red = 4,
    Green = 8,
    Blue = 16,
    Yellow = 32
}

public enum KeyColors
{
    Black,
    White
}

public record struct CodePeg(string Color)
{
    public override readonly string ToString() => Color;
}

public record struct KeyPeg(string Color)
{
    public override readonly string ToString() => Color;
}

public record struct Move(string GameId, int MoveNumber, IList<CodePeg> Codes, IList<KeyPeg> Keys);
