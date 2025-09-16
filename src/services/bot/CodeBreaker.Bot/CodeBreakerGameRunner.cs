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
        // Use same approach as Game6x4 but with 8 colors and 5 positions
        // We'll use 6 bits per position which can handle up to 64 values (more than enough for 8 colors)
        // But we can only fit 5 positions * 6 bits = 30 bits in a 32-bit int
        // So this will work fine
        
        static List<int> Create8Colors(int shift)
        {
            List<int> pin = [];
            for (int i = 0; i < 8; i++)  // 8 colors instead of 6
            {
                int x = 1 << (i + shift);
                pin.Add(x);
            }
            return pin;
        }

        static List<int> AddColorsToList(List<int> list1, List<int> list2)
        {
            List<int> result = new(capacity: list1.Count * list2.Count);
            for (int i = 0; i < list1.Count; i++)
            {
                for (int j = 0; j < list2.Count; j++)
                {
                    int x = list1[i] ^ list2[j]; // Keep XOR like the original
                    result.Add(x);
                }
            }
            return result;
        }

        // Create combinations for 5 positions with 6 bits each
        var digits1 = Create8Colors(0);   // bits 0-5
        var digits2 = Create8Colors(6);   // bits 6-11
        var list2 = AddColorsToList(digits1, digits2);
        var digits3 = Create8Colors(12);  // bits 12-17
        var list3 = AddColorsToList(list2, digits3);
        var digits4 = Create8Colors(18);  // bits 18-23
        var list4 = AddColorsToList(list3, digits4);
        var digits5 = Create8Colors(24);  // bits 24-29 (fits in 32-bit int)
        var list5 = AddColorsToList(list4, digits5);
        list5.Sort();
        return list5;
    }

    private List<int> InitializePossibleValues5x5x4()
    {
        // For Game5x5x4, we need to represent 25 different shape+color combinations
        // across 4 positions. Since we have 25 combinations and only 32 bits available,
        // we'll have to make some compromises. Let's use 6 bits per position (like Game6x4)
        // but limit to the first few combinations that fit.
        
        static List<int> Create25Combinations(int shift)
        {
            List<int> pin = [];
            // Limit to combinations that fit in 6 bits when shifted
            int maxCombinations = Math.Min(25, 64 >> (shift % 6));
            for (int i = 0; i < maxCombinations; i++)
            {
                int x = 1 << (i + shift);
                pin.Add(x);
            }
            return pin;
        }

        static List<int> AddCombinationsToList(List<int> list1, List<int> list2)
        {
            List<int> result = new(capacity: list1.Count * list2.Count);
            for (int i = 0; i < list1.Count; i++)
            {
                for (int j = 0; j < list2.Count; j++)
                {
                    int x = list1[i] ^ list2[j]; // Keep XOR like the original
                    result.Add(x);
                }
            }
            return result;
        }

        // Create combinations for 4 positions with 6 bits each (same as Game6x4)
        // This limits us to fewer than 25 combinations per position, but that's acceptable
        var digits1 = Create25Combinations(0);   // bits 0-5
        var digits2 = Create25Combinations(6);   // bits 6-11  
        var list2 = AddCombinationsToList(digits1, digits2);
        var digits3 = Create25Combinations(12);  // bits 12-17
        var list3 = AddCombinationsToList(list2, digits3);
        var digits4 = Create25Combinations(18);  // bits 18-23
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
            int blueHits = results.Count(c => c == "Blue");

            if (blackHits >= fieldsCount)
                throw new InvalidOperationException($"{fieldsCount} or more blacks but won was not set: {blackHits}");

            if (whiteHits > fieldsCount)
                throw new InvalidOperationException($"more than {fieldsCount} whites is not possible: {whiteHits}");

            if (blueHits > fieldsCount)
                throw new InvalidOperationException($"more than {fieldsCount} blues is not possible: {blueHits}");

            if (blackHits == 0 && whiteHits == 0 && blueHits == 0)
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
            if (blueHits > 0)
            {
                _possibleValues = _possibleValues.HandleBlueMatches(gameType, blueHits, selection);
                logger.ReducedPossibleValues(_possibleValues.Count, "Blue", gameId);
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
        _colorNames?[(value >> 0) & 0b111111] ?? string.Empty,   // bits 0-5
        _colorNames?[(value >> 6) & 0b111111] ?? string.Empty,   // bits 6-11
        _colorNames?[(value >> 12) & 0b111111] ?? string.Empty,  // bits 12-17
        _colorNames?[(value >> 18) & 0b111111] ?? string.Empty,  // bits 18-23
        _colorNames?[(value >> 24) & 0b111111] ?? string.Empty   // bits 24-29
    ];

    private string[] IntToColors5x5x4(int value) =>
    [
        _colorNames?[(value >> 0) & 0b111111] ?? string.Empty,   // bits 0-5
        _colorNames?[(value >> 6) & 0b111111] ?? string.Empty,   // bits 6-11
        _colorNames?[(value >> 12) & 0b111111] ?? string.Empty,  // bits 12-17
        _colorNames?[(value >> 18) & 0b111111] ?? string.Empty   // bits 18-23
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
