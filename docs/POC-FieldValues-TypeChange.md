# Proof of Concept: FieldValues Type Change
# Demonstrating Dictionary<string, string[]> Implementation

This document provides concrete code examples demonstrating how the type change from `IDictionary<string, IEnumerable<string>>` to `Dictionary<string, string[]>` would be implemented.

## 1. Model Changes

### Before (Current)
```csharp
// Game.cs
public class Game(
    Guid id,
    string gameType,
    string playerName,
    DateTime startTime,
    int numberCodes,
    int maxMoves) : IGame
{
    public Guid Id { get; private set; } = id;
    public string GameType { get; private set; } = gameType;
    public string PlayerName { get; private set; } = playerName;
    public bool PlayerIsAuthenticated { get; set; } = false;
    public DateTime StartTime { get; private set; } = startTime;
    public DateTime? EndTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public int LastMoveNumber { get; set; } = 0;
    public int NumberCodes { get; private set; } = numberCodes;
    public int MaxMoves { get; private set; } = maxMoves;
    public bool IsVictory { get; set; } = false;

    public required IDictionary<string, IEnumerable<string>> FieldValues { get; init; }

    public required string[] Codes { get; init; }

    public ICollection<Move> Moves { get; init; } = [];

    public override string ToString() => $"{Id}:{GameType} - {StartTime}";
}
```

### After (Option 1: Direct Change)
```csharp
// Game.cs
public class Game(
    Guid id,
    string gameType,
    string playerName,
    DateTime startTime,
    int numberCodes,
    int maxMoves) : IGame
{
    public Guid Id { get; private set; } = id;
    public string GameType { get; private set; } = gameType;
    public string PlayerName { get; private set; } = playerName;
    public bool PlayerIsAuthenticated { get; set; } = false;
    public DateTime StartTime { get; private set; } = startTime;
    public DateTime? EndTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public int LastMoveNumber { get; set; } = 0;
    public int NumberCodes { get; private set; } = numberCodes;
    public int MaxMoves { get; private set; } = maxMoves;
    public bool IsVictory { get; set; } = false;

    public required Dictionary<string, string[]> FieldValues { get; init; }

    public required string[] Codes { get; init; }

    public ICollection<Move> Moves { get; init; } = [];

    public override string ToString() => $"{Id}:{GameType} - {StartTime}";
    
    // Explicit interface implementation for IGame compatibility
    IDictionary<string, IEnumerable<string>> IGame.FieldValues => 
        FieldValues.ToDictionary(kvp => kvp.Key, kvp => (IEnumerable<string>)kvp.Value);
}
```

### After (Option 2: Maintain Full Compatibility)
```csharp
// Game.cs
public class Game(
    Guid id,
    string gameType,
    string playerName,
    DateTime startTime,
    int numberCodes,
    int maxMoves) : IGame
{
    // ... other properties remain the same ...

    // Internal storage for EF Core
    private Dictionary<string, string[]> _fieldValues = new();

    // Public API - maintains interface compatibility
    public required Dictionary<string, string[]> FieldValues 
    { 
        get => _fieldValues;
        init => _fieldValues = value; 
    }

    // Explicit interface implementation
    IDictionary<string, IEnumerable<string>> IGame.FieldValues => 
        _fieldValues.ToDictionary(kvp => kvp.Key, kvp => (IEnumerable<string>)kvp.Value);

    // ... rest of properties ...
}
```

## 2. Database Configuration Changes

### SQL Server - String Storage (Minimal Change)

```csharp
// MappingExtensions.cs - Updated signatures
public static class MappingExtensions
{
    // Updated: Return type changed from IDictionary to Dictionary
    // Updated: Parameter changed to string[]
    public static string ToFieldsString(this Dictionary<string, string[]> fields)
    {
        return string.Join(
            '#', fields.SelectMany(
                key => key.Value
                    .Select(value => $"{key.Key}:{value}")));
    }

    // Updated: Return type changed from IDictionary to Dictionary
    // Updated: Return List converted to array
    public static Dictionary<string, string[]> FromFieldsString(this string fieldsString)
    {
        Dictionary<string, List<string>> fields = [];

        foreach (string pair in fieldsString.Split('#'))
        {
            int index = pair.IndexOf(':');

            if (index < 0)
            {
                throw new ArgumentException($"Field {pair} does not contain ':' delimiter.");
            }

            string key = pair[..index];
            string value = pair[(index + 1)..];

            if (!fields.TryGetValue(key, out List<string>? list))
            {
                list = [];
                fields[key] = list;
            }

            list.Add(value);
        }

        // Updated: Convert List to array
        return fields.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ToArray());
    }
}

// GameConfiguration.cs
internal class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(g => g.Id);

        builder.HasMany(g => g.Moves)
            .WithOne()
            .HasForeignKey("GameId");
            
        builder.Property(g => g.GameType).HasMaxLength(20);
        builder.Property(g => g.PlayerName).HasMaxLength(60);

        builder.Property(g => g.Codes).HasMaxLength(120);

        // Updated: ValueComparer generic type parameter
        builder.Property(g => g.FieldValues)
            .HasColumnName("Fields")
            .HasColumnType("nvarchar")
            .HasMaxLength(200)
            .HasConversion(
                convertToProviderExpression: fields => fields.ToFieldsString(),
                convertFromProviderExpression: fields => fields.FromFieldsString(),
                valueComparer: new ValueComparer<Dictionary<string, string[]>>(
                    equalsExpression: (a, b) => CompareDictionaries(a, b),
                    hashCodeExpression: a => GetDictionaryHashCode(a),
                    snapshotExpression: a => a.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray())));
    }
    
    private static bool CompareDictionaries(
        Dictionary<string, string[]>? a, 
        Dictionary<string, string[]>? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        if (a.Count != b.Count) return false;

        foreach (var kvp in a)
        {
            if (!b.TryGetValue(kvp.Key, out var bValues)) return false;
            if (!kvp.Value.SequenceEqual(bValues)) return false;
        }
        return true;
    }
    
    private static int GetDictionaryHashCode(Dictionary<string, string[]> dict)
    {
        var hash = new HashCode();
        foreach (var kvp in dict.OrderBy(x => x.Key))
        {
            hash.Add(kvp.Key);
            foreach (var value in kvp.Value)
                hash.Add(value);
        }
        return hash.ToHashCode();
    }
}
```

### SQL Server - JSON Storage (Future Option)

```csharp
// GameConfiguration.cs - JSON storage approach
internal class GameConfiguration : IEntityTypeConfiguration<Game>
{
    public void Configure(EntityTypeBuilder<Game> builder)
    {
        builder.HasKey(g => g.Id);
        // ... other configurations ...

        // Option A: EF Core 9 with JSON converter
        builder.Property(g => g.FieldValues)
            .HasConversion(
                v => JsonSerializer.Serialize(v),
                v => JsonSerializer.Deserialize<Dictionary<string, string[]>>(v) 
                     ?? new Dictionary<string, string[]>())
            .HasColumnType("nvarchar(max)");

        // Option B: EF Core 10 (hypothetical - may not need converter)
        // builder.Property(g => g.FieldValues)
        //     .HasColumnType("nvarchar(max)"); // or "json" for SQL Server 2025
    }
}
```

### Cosmos DB Configuration

```csharp
// FieldValueValueConverter.cs - Updated for new type
internal class FieldValueValueConverter : ValueConverter<Dictionary<string, string[]>, string>
{
    static string GetJson(Dictionary<string, string[]> values) => 
        JsonSerializer.Serialize(values);

    static Dictionary<string, string[]> GetDictionary(string json) => 
        JsonSerializer.Deserialize<Dictionary<string, string[]>>(json) 
            ?? new Dictionary<string, string[]>();

    public FieldValueValueConverter() : base(
        convertToProviderExpression: v => GetJson(v),
        convertFromProviderExpression: v => GetDictionary(v))
    { }
}

// FieldValueComparer.cs - Updated for new type
internal class FieldValueComparer : ValueComparer<Dictionary<string, string[]>>
{
    public FieldValueComparer() : base(
        (a, b) => CompareFieldValues(a, b),
        v => GetHashCode(v),
        v => v.ToDictionary(kv => kv.Key, kv => kv.Value.ToArray()))
    { }

    private static bool CompareFieldValues(
        Dictionary<string, string[]>? a, 
        Dictionary<string, string[]>? b)
    {
        if (a is null && b is null) return true;
        if (a is null || b is null) return false;
        if (a.Count != b.Count) return false;

        foreach (var kvp in a)
        {
            if (!b.TryGetValue(kvp.Key, out var bValues)) return false;
            if (!kvp.Value.SequenceEqual(bValues)) return false;
        }
        return true;
    }

    private static int GetHashCode(Dictionary<string, string[]> v)
    {
        var hash = new HashCode();
        foreach (var kvp in v.OrderBy(x => x.Key))
        {
            hash.Add(kvp.Key);
            foreach (var value in kvp.Value)
                hash.Add(value);
        }
        return hash.ToHashCode();
    }
}

// GamesCosmosContext.cs - No changes needed to converter usage
public class GamesCosmosContext(DbContextOptions<GamesCosmosContext> options) : DbContext(options), IGamesRepository
{
    private static readonly FieldValueValueConverter s_fieldValueConverter = new();
    private static readonly FieldValueComparer s_fieldValueComparer = new();

    // ... rest of context ...

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultContainer(ContainerName);
        var gameModel = modelBuilder.Entity<Game>();

        // ... other configurations ...

        gameModel.Property(g => g.FieldValues)
            .HasConversion(s_fieldValueConverter, s_fieldValueComparer);
    }
}
```

## 3. Factory Method Updates

```csharp
// GamesFactory.cs
public static class GamesFactory
{
    private static readonly string[] s_colors6 = 
        [Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Purple, Colors.Orange];
    private static readonly string[] s_colors8 = 
        [Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Purple, Colors.Orange, Colors.Pink, Colors.Brown];
    private static readonly string[] s_colors5 = 
        [Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Purple];
    private static readonly string[] s_shapes5 = 
        [Shapes.Circle, Shapes.Square, Shapes.Triangle, Shapes.Star, Shapes.Rectangle];

    public static Game CreateGame(string gameType, string playerName)
    {
        Game Create6x4SimpleGame() =>
            new(Guid.NewGuid(), gameType, playerName,  DateTime.UtcNow, 4, 12)
            {
                // Updated: Dictionary<string, string[]> instead of IDictionary<string, IEnumerable<string>>
                FieldValues = new Dictionary<string, string[]>()
                {
                    { FieldCategories.Colors, s_colors6 }
                },
                Codes = Random.Shared.GetItems(s_colors6, 4)
            };

        Game Create6x4Game() =>
            new(Guid.NewGuid(), gameType, playerName, DateTime.UtcNow, 4, 12)
            {
                // Updated: Dictionary<string, string[]>
                FieldValues = new Dictionary<string, string[]>()
                {
                    { FieldCategories.Colors, s_colors6 }
                },
                Codes = Random.Shared.GetItems(s_colors6, 4)
            };

        Game Create8x5Game() =>
            new(Guid.NewGuid(), gameType, playerName, DateTime.UtcNow, 5, 12)
            {
                // Updated: Dictionary<string, string[]>
                FieldValues = new Dictionary<string, string[]>()
                {
                    { FieldCategories.Colors, s_colors8 }
                },
                Codes = Random.Shared.GetItems(s_colors8, 5)
            };

        Game Create5x5x4Game() =>
            new(Guid.NewGuid(), gameType, playerName, DateTime.UtcNow, 4, 14)
            {
                // Updated: Dictionary<string, string[]>
                FieldValues = new Dictionary<string, string[]>()
                {
                    { FieldCategories.Colors, s_colors5 },
                    { FieldCategories.Shapes, s_shapes5 }
                },
                Codes = Random.Shared.GetItems(s_shapes5, 4)
                    .Zip(Random.Shared.GetItems(s_colors5, 4), (shape, color) => (Shape: shape, Color: color))
                    .Select(item => string.Join(';', item.Shape, item.Color))
                    .ToArray()
            };
        
        return gameType switch
        {
            GameTypes.Game6x4Mini => Create6x4SimpleGame(),
            GameTypes.Game6x4 => Create6x4Game(),
            GameTypes.Game8x5 => Create8x5Game(),
            GameTypes.Game5x5x4 => Create5x5x4Game(),
            _ => throw new CodebreakerException("Invalid game type") { Code = CodebreakerExceptionCodes.InvalidGameType }
        };
    }
}
```

## 4. Test Updates

```csharp
// GamesServiceTests.cs
public class GamesServiceTests
{
    [Fact]
    public async Task CreateGameAsync_ShouldCreateGame()
    {
        // Arrange
        var mockRepository = new Mock<IGamesRepository>();
        var service = new GamesService(mockRepository.Object);
        
        // Updated: Use Dictionary<string, string[]>
        var expectedGame = new Game(Guid.NewGuid(), "Game6x4", "TestPlayer", 
            DateTime.UtcNow, 4, 12)
        {
            FieldValues = new Dictionary<string, string[]>()
            {
                { "colors", ["Red", "Green", "Blue", "Yellow", "Purple", "Orange"] }
            },
            Codes = ["Red", "Blue", "Green", "Yellow"]
        };

        mockRepository.Setup(r => r.AddGameAsync(It.IsAny<Game>(), default))
            .Returns(Task.CompletedTask);

        // Act
        var result = await service.CreateGameAsync("Game6x4", "TestPlayer");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Game6x4", result.GameType);
        Assert.Equal("TestPlayer", result.PlayerName);
        Assert.Equal(6, result.FieldValues["colors"].Length); // Updated: Use .Length instead of .Count()
    }
}

// GamesFactoryTests.cs
public class GamesFactoryTests
{
    [Fact]
    public void CreateGame_Game6x4_ShouldHaveCorrectFieldValues()
    {
        // Act
        var game = GamesFactory.CreateGame(GameTypes.Game6x4, "TestPlayer");

        // Assert
        Assert.NotNull(game.FieldValues);
        Assert.Single(game.FieldValues); // One category: colors
        Assert.True(game.FieldValues.ContainsKey("colors"));
        
        // Updated: Access as array
        var colors = game.FieldValues["colors"];
        Assert.Equal(6, colors.Length); // Updated: Use .Length
        Assert.Contains("Red", colors);
        Assert.Contains("Green", colors);
        Assert.Contains("Blue", colors);
    }

    [Fact]
    public void CreateGame_Game5x5x4_ShouldHaveColorAndShapeFields()
    {
        // Act
        var game = GamesFactory.CreateGame(GameTypes.Game5x5x4, "TestPlayer");

        // Assert
        Assert.NotNull(game.FieldValues);
        Assert.Equal(2, game.FieldValues.Count); // Two categories
        Assert.True(game.FieldValues.ContainsKey("colors"));
        Assert.True(game.FieldValues.ContainsKey("shapes"));
        
        // Updated: Access as arrays
        var colors = game.FieldValues["colors"];
        var shapes = game.FieldValues["shapes"];
        
        Assert.Equal(5, colors.Length); // Updated: Use .Length
        Assert.Equal(5, shapes.Length); // Updated: Use .Length
    }

    [Fact]
    public void FieldValues_CanStillBeUsedAsIEnumerable()
    {
        // Act
        var game = GamesFactory.CreateGame(GameTypes.Game6x4, "TestPlayer");

        // Assert - arrays implement IEnumerable<string>
        IEnumerable<string> colors = game.FieldValues["colors"];
        Assert.NotEmpty(colors);
        
        // Can still use LINQ
        Assert.True(colors.Count() == 6);
        Assert.True(colors.Any(c => c == "Red"));
    }
    
    [Fact]
    public void FieldValues_WorksThroughIGameInterface()
    {
        // Arrange
        Game game = GamesFactory.CreateGame(GameTypes.Game6x4, "TestPlayer");
        IGame iGame = game;
        
        // Act - access through interface
        var colors = iGame.FieldValues["colors"];
        
        // Assert - interface returns IEnumerable<string>
        Assert.NotEmpty(colors);
        Assert.True(colors.Count() == 6);
    }
}

// MappingExtensionsTests.cs
public class MappingExtensionsTests
{
    [Fact]
    public void ToFieldString_ShouldReturnCorrectString()
    {
        // Arrange - Updated type
        Dictionary<string, string[]> dict = new()
        {
            { "colors", ["Red", "Green", "Blue"] },
            { "shapes", ["Rectangle", "Circle"] }
        };

        string expected = "colors:Red#colors:Green#colors:Blue#shapes:Rectangle#shapes:Circle";

        // Act
        string result = MappingExtensions.ToFieldsString(dict);

        // Assert
        Assert.Equal(expected, result);
    }

    [Fact]
    public void FromFieldsString_ShouldReturnCorrectDictionary()
    {
        // Arrange
        string input = "colors:Red#colors:Green#colors:Blue#shapes:Rectangle#shapes:Circle";

        // Act - Updated return type
        Dictionary<string, string[]> result = MappingExtensions.FromFieldsString(input);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.True(result.ContainsKey("colors"));
        Assert.True(result.ContainsKey("shapes"));
        
        // Updated: Access as arrays
        Assert.Equal(3, result["colors"].Length);
        Assert.Equal(2, result["shapes"].Length);
        
        Assert.Equal(["Red", "Green", "Blue"], result["colors"]);
        Assert.Equal(["Rectangle", "Circle"], result["shapes"]);
    }

    [Fact]
    public void RoundTrip_ShouldMaintainData()
    {
        // Arrange
        Dictionary<string, string[]> original = new()
        {
            { "colors", ["Red", "Green", "Blue"] },
            { "shapes", ["Rectangle", "Circle"] }
        };

        // Act - round trip conversion
        string serialized = original.ToFieldsString();
        Dictionary<string, string[]> deserialized = serialized.FromFieldsString();

        // Assert
        Assert.Equal(original.Count, deserialized.Count);
        foreach (var kvp in original)
        {
            Assert.True(deserialized.ContainsKey(kvp.Key));
            Assert.Equal(kvp.Value, deserialized[kvp.Key]);
        }
    }
}
```

## 5. Bot Client Updates

```csharp
// GrpcGamesClient.cs (both Bot and BotQ)
public class GrpcGamesClient : IGamesClient
{
    // Updated return type
    public async Task<(Guid Id, int NumberCodes, int MaxMoves, IDictionary<string, string[]> FieldValues)> 
        StartGameAsync(GameType gameType, string playerName, CancellationToken cancellationToken = default)
    {
        var gameType1 = gameType switch
        {
            GameType.Game6x4 => CNinnovation.Codebreaker.Grpc.GameType.Game6X4,
            GameType.Game6x4Mini => CNinnovation.Codebreaker.Grpc.GameType.Game6X4Mini,
            GameType.Game8x5 => CNinnovation.Codebreaker.Grpc.GameType.Game8X5,
            GameType.Game5x5x4 => CNinnovation.Codebreaker.Grpc.GameType.Game5X5X4,
            _ => throw new InvalidGameException(gameType.ToString()) { GameType = gameType.ToString() }
        };

        StartGameRequest request = new()
        {
            GameType = gameType1,
            PlayerName = playerName
        };

        StartGameResponse response = await _client.StartGameAsync(request, cancellationToken: cancellationToken);
        
        // Updated: Use new converter
        Dictionary<string, string[]> fieldValues = ConvertFieldValues(response.FieldValues);

        return (
            Guid.Parse(response.GameId),
            response.NumberCodes,
            response.MaxMoves,
            fieldValues
        );
    }

    // Updated: Return type changed to Dictionary<string, string[]>
    private static Dictionary<string, string[]> ConvertFieldValues(MapField<string, FieldNames> fieldValues)
    {
        Dictionary<string, string[]> convertedFieldValues = [];

        foreach (var pair in fieldValues)
        {
            // Updated: Convert to array directly
            convertedFieldValues.Add(pair.Key, [.. pair.Value.Values]);
        }

        return convertedFieldValues;
    }

    public async Task<UpdateGameResponse> SetMoveAsync(Guid gameId, string gameType, 
        int moveNumber, string[] guessPegs, CancellationToken cancellationToken = default)
    {
        SetMoveRequest request = new()
        {
            GameId = gameId.ToString(),
            GameType = gameType,
            MoveNumber = moveNumber
        };
        request.GuessPegs.AddRange(guessPegs);

        SetMoveResponse response = await _client.SetMoveAsync(request, cancellationToken: cancellationToken);

        return new UpdateGameResponse(
            gameId,
            response.MoveNumber,
            response.Ended,
            response.IsVictory,
            // Updated: Use new converter
            response.Results.ToDictionary(
                x => x.Key, 
                x => x.Value.Results.ToArray()))
        {
            FieldValues = fieldValues,  // This is already Dictionary<string, string[]>
        };
    }
}
```

## 6. Client Library Updates

```csharp
// GameInfo.cs
public class GameInfo(
    Guid id,
    string gameType,
    string playerName,
    DateTime startTime,
    int numberCodes,
    int maxMoves)
{
    public Guid Id { get; private set; } = id;
    public string GameType { get; private set; } = gameType;
    public string PlayerName { get; private set; } = playerName;
    public bool PlayerIsAuthenticated { get; set; } = false;
    public DateTime StartTime { get; private set; } = startTime;
    public DateTime? EndTime { get; set; }
    public TimeSpan? Duration { get; set; }
    public int LastMoveNumber { get; set; } = 0;
    public int NumberCodes { get; private set; } = numberCodes;
    public int MaxMoves { get; private set; } = maxMoves;
    public bool IsVictory { get; set; } = false;

    /// <summary>
    /// A list of possible field values the user has to chose from
    /// </summary>
    // Updated: Changed from IDictionary<string, IEnumerable<string>> to Dictionary<string, string[]>
    public required Dictionary<string, string[]> FieldValues { get; init; }

    public required string[] Codes { get; init; }

    public ICollection<MoveInfo> Moves { get; init; } = [];

    public override string ToString() => $"{Id}:{GameType} - {StartTime}";
}
```

## 7. Usage Examples

### Creating a Game
```csharp
// Creating a game with the new type
var game = new Game(
    Guid.NewGuid(),
    "Game6x4",
    "Player1",
    DateTime.UtcNow,
    4,
    12)
{
    FieldValues = new Dictionary<string, string[]>
    {
        { "colors", ["Red", "Green", "Blue", "Yellow", "Purple", "Orange"] }
    },
    Codes = ["Red", "Green", "Blue", "Yellow"]
};

// Accessing field values
string[] colors = game.FieldValues["colors"];
int colorCount = colors.Length;

// Still works with IEnumerable patterns
IEnumerable<string> colorsEnum = game.FieldValues["colors"];
bool hasRed = colorsEnum.Contains("Red");

// LINQ still works
var upperColors = game.FieldValues["colors"]
    .Select(c => c.ToUpper())
    .ToArray();
```

### Using Through Interface
```csharp
IGame iGame = game;

// Access through interface (if explicit implementation is used)
var colors = iGame.FieldValues["colors"]; // Returns IEnumerable<string>
foreach (var color in colors)
{
    Console.WriteLine(color);
}
```

### Analyzer Usage (No Changes Needed)
```csharp
// In ColorGameGuessAnalyzer - no changes required
// string[] works as IEnumerable<string> seamlessly
if (Guesses.Any(guessPeg => !_game.FieldValues[FieldCategories.Colors].Contains(guessPeg.ToString())))
{
    string fields = string.Join(", ", _game.FieldValues[FieldCategories.Colors]);
    throw new ArgumentException($"The guess peg {guessPeg} is not available. Use {fields}");
}
```

## 8. Backward Compatibility Considerations

### For API Consumers
```csharp
// Old code that used IEnumerable<string>
void ProcessGame(Game game)
{
    // This still works - string[] implements IEnumerable<string>
    IEnumerable<string> colors = game.FieldValues["colors"];
    
    foreach (var color in colors)
    {
        Console.WriteLine(color);
    }
    
    // Count() still works
    int count = colors.Count();
    
    // LINQ still works
    var upperColors = colors.Select(c => c.ToUpper());
}

// New code can use array-specific features
void ProcessGameEfficiently(Game game)
{
    // Direct array access
    string[] colors = game.FieldValues["colors"];
    
    // More efficient than Count()
    int count = colors.Length;
    
    // Array indexing
    string firstColor = colors[0];
    
    // Array methods
    Array.Sort(colors);
}
```

### For Interface Implementations
```csharp
// If IGame interface is updated to use Dictionary<string, string[]>
// Existing implementations need to change

// Before:
class MockGame : IGame
{
    public IDictionary<string, IEnumerable<string>> FieldValues { get; set; }
    // ...
}

// After:
class MockGame : IGame
{
    public Dictionary<string, string[]> FieldValues { get; set; }
    // ...
}

// If interface keeps IDictionary<string, IEnumerable<string>>
// Game class uses explicit implementation - no breaking change for interface consumers
```

## 9. Summary of Changes

### Files to Modify (Minimal Set)
1. ✏️ `Game.cs` - Property type change
2. ✏️ `IGame.cs` - Interface update (or keep and use explicit implementation)
3. ✏️ `MappingExtensions.cs` (SQL Server) - Method signatures
4. ✏️ `MappingExtensions.cs` (Postgres) - Method signatures
5. ✏️ `GameConfiguration.cs` (SQL Server) - ValueComparer type
6. ✏️ `GameConfiguration.cs` (Postgres) - ValueComparer type
7. ✏️ `FieldValueValueConverter.cs` (Cosmos) - Generic type parameter
8. ✏️ `FieldValueComparer.cs` (Cosmos) - Generic type parameter
9. ✏️ `GamesFactory.cs` - Dictionary initialization
10. ✏️ `GameInfo.cs` (Client) - Property type
11. ✏️ `GrpcGamesClient.cs` (Bot) - Return types and conversions
12. ✏️ `GrpcGamesClient.cs` (BotQ) - Return types and conversions
13. ✏️ Test files - Assertions and type usage

### Database Impact
- **SQL Server:** String format unchanged, no migration needed
- **PostgreSQL:** String format unchanged, no migration needed
- **Cosmos DB:** JSON structure unchanged, no migration needed

### API Impact
- **Breaking Change:** Yes, if consumers directly use the property
- **Mitigation:** Explicit interface implementation maintains IGame compatibility
- **Version Bump:** Major version (4.0.0) recommended

---

**Conclusion:** The type change is straightforward and mostly involves updating type signatures. The actual storage format and database schemas can remain unchanged, making this a low-risk breaking change that provides better performance and type safety.
