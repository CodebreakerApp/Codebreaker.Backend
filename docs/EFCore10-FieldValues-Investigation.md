# EF Core 10 and SQL Server 2025 JSON Support Investigation
# FieldValues Property Type Change Impact Assessment

**Issue:** [#340 - EF Core 10 and SQL Server 2025 support with JSON updates](https://github.com/CodebreakerApp/Codebreaker.Backend/issues/340)  
**Date:** October 15, 2025  
**EF Core Version:** Currently using EF Core 9.0.9  

## Executive Summary

This document investigates the impact of changing the `FieldValues` property from `IDictionary<string, IEnumerable<string>>` to `Dictionary<string, string[]>` in the Game class, and explores mapping strategies leveraging EF Core 10's improved JSON support.

### Key Findings:
1. **Breaking Change Impact:** Moderate - affects 8+ files across multiple layers
2. **Current EF Core Version:** 9.0.9 (not 10.x yet)
3. **JSON Support:** EF Core 9 requires custom converters; native JSON support improves but doesn't eliminate all conversion needs
4. **Recommended Approach:** Wait for EF Core 10 release and test native JSON column support before making breaking changes

---

## 1. Current Implementation Analysis

### 1.1 Property Definition

**Location:** `src/services/common/Codebreaker.GameAPIs.Models/Game.cs`

```csharp
public class Game(
    Guid id,
    string gameType,
    string playerName,
    DateTime startTime,
    int numberCodes,
    int maxMoves) : IGame
{
    // ... other properties ...
    
    public required IDictionary<string, IEnumerable<string>> FieldValues { get; init; }
    
    // ... other properties ...
}
```

**Interface Contract:** `src/services/common/Codebreaker.GameAPIs.Analyzers/Contracts/IGame.cs`

```csharp
public interface IGame
{
    /// <summary>
    /// The string representation of available field values the user can chose from to position the pegs.
    /// Multiple categories for the field values can be defined, such as *colors* and *shapes*.
    /// </summary>
    IDictionary<string, IEnumerable<string>> FieldValues { get; }
}
```

### 1.2 Database Configurations

#### SQL Server Configuration
**Location:** `src/services/common/Codebreaker.Data.SqlServer/Configurations/GameConfiguration.cs`

```csharp
builder.Property(g => g.FieldValues)
    .HasColumnName("Fields")
    .HasColumnType("nvarchar")
    .HasMaxLength(200)
    .HasConversion(
        convertToProviderExpression: fields => fields.ToFieldsString(),
        convertFromProviderExpression: fields => fields.FromFieldsString(),
        valueComparer: new ValueComparer<IDictionary<string, IEnumerable<string>>>(
            equalsExpression: (a, b) => a!.SequenceEqual(b!),
            hashCodeExpression: a => a.Aggregate(0, (result, next) => HashCode.Combine(result, next.GetHashCode())),
            snapshotExpression: a => a.ToDictionary(kv => kv.Key, kv => kv.Value)));
```

**Current Storage Format:** String-based with custom delimiter format
- Example: `"colors:Red#colors:Green#colors:Blue#shapes:Rectangle#shapes:Circle"`
- Stores in `nvarchar(200)` column

#### PostgreSQL Configuration
**Location:** `src/services/common/Codebreaker.Data.Postgres/Configurations/GameConfiguration.cs`

```csharp
builder.Property(g => g.FieldValues)
    .HasColumnName("Fields")
    .HasConversion(
        convertToProviderExpression: fields => fields.ToFieldsString(),
        convertFromProviderExpression: fields => fields.FromFieldsString(),
        valueComparer: new ValueComparer<IDictionary<string, IEnumerable<string>>>(
            equalsExpression: (a, b) => a!.SequenceEqual(b!),
            hashCodeExpression: a => a.Aggregate(0, (result, next) => HashCode.Combine(result, next.GetHashCode())),
            snapshotExpression: a => a.ToDictionary(kv => kv.Key, kv => kv.Value)));
```

**Current Storage Format:** Same string-based format as SQL Server

#### Cosmos DB Configuration
**Location:** `src/services/common/Codebreaker.Data.Cosmos/GamesCosmosContext.cs`

```csharp
private static readonly FieldValueValueConverter s_fieldValueConverter = new();
private static readonly FieldValueComparer s_fieldValueComparer = new();

gameModel.Property(g => g.FieldValues)
    .HasConversion(s_fieldValueConverter, s_fieldValueComparer);
```

**Converter Implementation:** `src/services/common/Codebreaker.Data.Cosmos/Utilities/FieldValueValueConverter.cs`

```csharp
internal class FieldValueValueConverter : ValueConverter<IDictionary<string, IEnumerable<string>>, string>
{
    static string GetJson(IDictionary<string, IEnumerable<string>> values) => 
        JsonSerializer.Serialize(values);

    static IDictionary<string, IEnumerable<string>> GetDictionary(string json) => 
        JsonSerializer.Deserialize<IDictionary<string, IEnumerable<string>>>(json) 
            ?? new Dictionary<string, IEnumerable<string>>();

    public FieldValueValueConverter() : base(
        convertToProviderExpression: v => GetJson(v),
        convertFromProviderExpression: v => GetDictionary(v))
    { }
}
```

**Current Storage Format:** JSON serialization (proper JSON document in Cosmos DB)

### 1.3 Conversion Extension Methods

**Location:** `src/services/common/Codebreaker.Data.SqlServer/MappingExtensions.cs` (same in Postgres)

```csharp
public static string ToFieldsString(this IDictionary<string, IEnumerable<string>> fields)
{
    return string.Join(
        '#', fields.SelectMany(
            key => key.Value
                .Select(value => $"{key.Key}:{value}")));
}

public static IDictionary<string, IEnumerable<string>> FromFieldsString(this string fieldsString)
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

    return fields.ToDictionary(
        pair => pair.Key,
        pair => (IEnumerable<string>)pair.Value);
}
```

---

## 2. Impact Assessment of Type Change

### 2.1 Affected Files and Locations

#### Core Model Layer (2 files)
1. **`src/services/common/Codebreaker.GameAPIs.Models/Game.cs`**
   - Property declaration: `IDictionary<string, IEnumerable<string>>` → `Dictionary<string, string[]>`

2. **`src/services/common/Codebreaker.GameAPIs.Analyzers/Contracts/IGame.cs`**
   - Interface property: `IDictionary<string, IEnumerable<string>>` → Change to `Dictionary<string, string[]>` OR keep as-is if polymorphism desired

#### Database Configuration Layer (6 files)
3. **`src/services/common/Codebreaker.Data.SqlServer/Configurations/GameConfiguration.cs`**
   - Update `HasConversion` to work with new type
   - Update `ValueComparer` generic type parameter

4. **`src/services/common/Codebreaker.Data.Postgres/Configurations/GameConfiguration.cs`**
   - Same changes as SQL Server

5. **`src/services/common/Codebreaker.Data.Cosmos/GamesCosmosContext.cs`**
   - Update converter reference

6. **`src/services/common/Codebreaker.Data.Cosmos/Utilities/FieldValueValueConverter.cs`**
   - Change from: `ValueConverter<IDictionary<string, IEnumerable<string>>, string>`
   - Change to: `ValueConverter<Dictionary<string, string[]>, string>`
   - Update serialization/deserialization

7. **`src/services/common/Codebreaker.Data.Cosmos/Utilities/FieldValueComparer.cs`**
   - Update comparer type

8. **`src/services/common/Codebreaker.Data.SqlServer/MappingExtensions.cs`**
   - Update `ToFieldsString` signature: `Dictionary<string, string[]>` parameter
   - Update `FromFieldsString` return type: `Dictionary<string, string[]>`

9. **`src/services/common/Codebreaker.Data.Postgres/MappingExtensions.cs`**
   - Same changes as SQL Server extensions

#### Factory/Service Layer (1 file)
10. **`src/services/gameapis/Codebreaker.GameAPIs/Services/GamesFactory.cs`**
    - Update 4 game creation methods:
    ```csharp
    // From:
    FieldValues = new Dictionary<string, IEnumerable<string>>()
    {
        { FieldCategories.Colors, s_colors6 }
    }
    
    // To:
    FieldValues = new Dictionary<string, string[]>()
    {
        { FieldCategories.Colors, s_colors6 }
    }
    ```

#### Client Library (1 file)
11. **`src/clients/Codebreaker.GameAPIs.Client/Models/GameInfo.cs`**
    - Property type change: `IDictionary<string, IEnumerable<string>>` → `Dictionary<string, string[]>`

#### Bot Clients (2 files)
12. **`src/services/bot/Codebreaker.BotQ/GrpcGamesClient.cs`**
    - Update return type and conversion logic for `StartGameAsync`
    - Update `ConvertFieldValues` method signature

13. **`src/services/bot/CodeBreaker.Bot/GrpcGamesClient.cs`**
    - Same changes as BotQ

#### Test Files (3+ files)
14. **`src/services/gameapis/Codebreaker.GameAPIs.Tests/GamesFactoryTests.cs`**
    - Update test assertions that use `.Count()` (string arrays already support this)
    - Most tests should work unchanged since arrays implement IEnumerable

15. **`src/services/gameapis/Codebreaker.GameAPIs.Tests/GamesServiceTests.cs`**
    - Update mock game creation

16. **`src/services/gameapis/Codebreaker.GameAPIs.Tests/GamesMetricsTests.cs`**
    - Update mock game creation

17. **`src/services/common/Codebreaker.Data.SqlServer.Tests/MappingExtensionsTests.cs`**
    - Update all test cases for new type signatures

### 2.2 Usage Pattern Analysis

**Read-only Access Patterns (Compatible):**
```csharp
// These patterns work with both IEnumerable<string> and string[]
game.FieldValues["colors"].Count()  // Works - arrays support .Count()
game.FieldValues["colors"]          // Works - can assign to var or IEnumerable<string>
foreach (var color in game.FieldValues["colors"])  // Works - arrays are enumerable
```

**Analyzer Usage (Requires Interface Update):**
```csharp
// In ColorGameGuessAnalyzer.cs and similar
if (Guesses.Any(guessPeg => !_game.FieldValues[FieldCategories.Colors].Contains(guessPeg.ToString())))
    throw new ArgumentException($"The guess peg {guessPeg} is not available...");

// This works fine with string[] as it's IEnumerable<string> compatible
```

**Critical Consideration:** The `IGame` interface in the analyzers package defines `FieldValues` as `IDictionary<string, IEnumerable<string>>`. If we want backward compatibility with external consumers, we should keep the interface unchanged and use explicit implementation.

---

## 3. EF Core JSON Support Investigation

### 3.1 Current State (EF Core 9.0.9)

The repository currently uses **EF Core 9.0.9**, not EF Core 10:

```xml
<!-- From Directory.Packages.props -->
<PackageVersion Include="Microsoft.EntityFrameworkCore.Cosmos" Version="9.0.9" Condition="'$(TargetFramework)' == 'net9.0'" />
<PackageVersion Include="Microsoft.EntityFrameworkCore.SqlServer" Version="9.0.9" Condition="'$(TargetFramework)' == 'net9.0'" />
```

### 3.2 EF Core 9 JSON Column Support

EF Core 9 introduced improved JSON column support:

**What's Supported (EF Core 9):**
- Native JSON columns for SQL Server 2016+
- `Dictionary<string, T>` where T is a primitive or simple type
- Automatic serialization/deserialization
- Some query capabilities (limited)

**What's NOT Supported:**
- `IDictionary<string, IEnumerable<string>>` - interfaces with collection values
- Complex nested collection scenarios without custom converters
- Full LINQ queryability inside JSON

### 3.3 Proposed EF Core 10 Approach (SQL Server)

With EF Core 10 and SQL Server 2025, the following approach would work:

```csharp
// Game class
public class Game
{
    // Option 1: Change property type directly (Breaking Change)
    public required Dictionary<string, string[]> FieldValues { get; init; }
}

// EF Core 10 configuration (SQL Server)
builder.Entity<Game>()
    .Property(g => g.FieldValues)
    .HasColumnType("nvarchar(max)"); // Or specific JSON type in SQL Server 2025

// Note: This requires EF Core 10 to natively support Dictionary<string, string[]>
```

**Queryability Benefits:**
```csharp
// With native JSON support, you could potentially query:
var games = await context.Games
    .Where(g => g.FieldValues["colors"].Contains("Red"))
    .ToListAsync();
```

### 3.4 Alternative: Keep Interface, Map to Concrete Type

**Approach:** Keep `IDictionary<string, IEnumerable<string>>` in the model but configure EF Core to use `Dictionary<string, string[]>` for storage.

**EF Core 9/10 Configuration Attempt:**
```csharp
builder.Property(g => g.FieldValues)
    .HasConversion(
        // Convert IDictionary<string, IEnumerable<string>> to Dictionary<string, string[]>
        v => v.ToDictionary(
            kvp => kvp.Key, 
            kvp => kvp.Value.ToArray()),
        // Convert Dictionary<string, string[]> back to IDictionary<string, IEnumerable<string>>
        v => v.ToDictionary(
            kvp => kvp.Key, 
            kvp => (IEnumerable<string>)kvp.Value));
```

**Limitations:**
1. Still requires a converter even with EF Core 10
2. No native JSON queryability since EF Core sees it as converted value
3. More complex than direct type change
4. Serialization would still need explicit JSON converter

---

## 4. Mapping Strategy Options

### Strategy 1: Direct Property Type Change (Recommended for Major Version)

**Approach:** Change `FieldValues` from `IDictionary<string, IEnumerable<string>>` to `Dictionary<string, string[]>`

**Pros:**
- Aligns with EF Core's native JSON support capabilities
- Cleaner configuration with less custom code
- Better performance (arrays vs enumerable)
- Future-proof for EF Core 10+

**Cons:**
- **Breaking change** for all consumers
- Requires updating IGame interface
- May affect external packages/clients
- Requires coordinated deployment

**Implementation Steps:**
1. Update `Game.cs` property type
2. Update `IGame.cs` interface
3. Update all database configurations
4. Update mapping extensions
5. Update factory methods
6. Update tests
7. Bump major version (4.0.0)
8. Document migration path for consumers

**Migration Path for Consumers:**
```csharp
// Before:
IDictionary<string, IEnumerable<string>> fieldValues = game.FieldValues;

// After:
Dictionary<string, string[]> fieldValues = game.FieldValues;
// Or keep using as IEnumerable:
foreach (var color in game.FieldValues["colors"]) { } // Still works
```

### Strategy 2: Keep Interface, Use Explicit Implementation

**Approach:** Keep public interface as `IDictionary<string, IEnumerable<string>>`, use explicit property internally

**Pros:**
- Maintains backward compatibility
- No breaking changes for consumers
- Gradual migration possible

**Cons:**
- More complex implementation
- Requires dual properties or complex getters
- Doesn't simplify EF Core configuration
- Performance overhead of conversions

**Implementation Example:**
```csharp
public class Game : IGame
{
    // Internal storage
    private Dictionary<string, string[]> _fieldValues = new();
    
    // Public interface implementation
    IDictionary<string, IEnumerable<string>> IGame.FieldValues => 
        _fieldValues.ToDictionary(kvp => kvp.Key, kvp => (IEnumerable<string>)kvp.Value);
    
    // EF Core mapped property
    public Dictionary<string, string[]> FieldValues
    {
        get => _fieldValues;
        init => _fieldValues = value;
    }
}
```

**Note:** This adds complexity and doesn't solve the EF Core configuration challenges.

### Strategy 3: Wait for EF Core 10 Native Support

**Approach:** Wait for EF Core 10 release and test native JSON support before making changes

**Pros:**
- Can leverage native features without custom code
- Better understanding of actual capabilities
- Avoid premature optimization

**Cons:**
- Continues using custom converters for now
- Delays potential benefits
- EF Core 10 may still require converters for this specific scenario

**Current Action:** Keep monitoring EF Core 10 development

---

## 5. Code Samples and Proof of Concept

### 5.1 Current Working Configuration (EF Core 9)

#### SQL Server with String Storage
```csharp
// Configuration
builder.Property(g => g.FieldValues)
    .HasColumnName("Fields")
    .HasColumnType("nvarchar(200)")
    .HasConversion(
        fields => fields.ToFieldsString(),
        fields => fields.FromFieldsString(),
        new ValueComparer<IDictionary<string, IEnumerable<string>>>(
            (a, b) => a!.SequenceEqual(b!),
            a => a.Aggregate(0, (r, n) => HashCode.Combine(r, n.GetHashCode())),
            a => a.ToDictionary(kv => kv.Key, kv => kv.Value)));

// Storage format in database:
// "colors:Red#colors:Green#colors:Blue#shapes:Circle#shapes:Square"
```

#### Cosmos DB with JSON Storage
```csharp
// Configuration
gameModel.Property(g => g.FieldValues)
    .HasConversion(
        v => JsonSerializer.Serialize(v),
        v => JsonSerializer.Deserialize<IDictionary<string, IEnumerable<string>>>(v) 
             ?? new Dictionary<string, IEnumerable<string>>());

// Storage format in Cosmos DB:
// {
//   "colors": ["Red", "Green", "Blue"],
//   "shapes": ["Circle", "Square"]
// }
```

### 5.2 Proposed Configuration with Type Change

#### Game Model Update
```csharp
public class Game : IGame
{
    // ... other properties ...
    
    public required Dictionary<string, string[]> FieldValues { get; init; }
    
    // Explicit interface implementation for backward compatibility
    IDictionary<string, IEnumerable<string>> IGame.FieldValues => 
        FieldValues.ToDictionary(kvp => kvp.Key, kvp => (IEnumerable<string>)kvp.Value);
}
```

#### SQL Server Configuration (Option A: Continue with String Storage)
```csharp
public static class MappingExtensions
{
    public static string ToFieldsString(this Dictionary<string, string[]> fields)
    {
        return string.Join(
            '#', fields.SelectMany(
                key => key.Value
                    .Select(value => $"{key.Key}:{value}")));
    }

    public static Dictionary<string, string[]> FromFieldsString(this string fieldsString)
    {
        Dictionary<string, List<string>> fields = [];

        foreach (string pair in fieldsString.Split('#'))
        {
            int index = pair.IndexOf(':');
            if (index < 0)
                throw new ArgumentException($"Field {pair} does not contain ':' delimiter.");

            string key = pair[..index];
            string value = pair[(index + 1)..];

            if (!fields.TryGetValue(key, out List<string>? list))
            {
                list = [];
                fields[key] = list;
            }
            list.Add(value);
        }

        return fields.ToDictionary(
            pair => pair.Key,
            pair => pair.Value.ToArray());
    }
}

// Configuration
builder.Property(g => g.FieldValues)
    .HasColumnName("Fields")
    .HasColumnType("nvarchar(200)")
    .HasConversion(
        fields => fields.ToFieldsString(),
        fields => fields.FromFieldsString(),
        new ValueComparer<Dictionary<string, string[]>>(
            (a, b) => a.SequenceEqual(b, new KeyValuePairComparer()),
            a => a.Aggregate(0, (r, n) => HashCode.Combine(r, n.Key, n.Value)),
            a => new Dictionary<string, string[]>(a)));
```

#### SQL Server Configuration (Option B: JSON Storage with EF Core 9)
```csharp
builder.Property(g => g.FieldValues)
    .HasConversion(
        v => JsonSerializer.Serialize(v),
        v => JsonSerializer.Deserialize<Dictionary<string, string[]>>(v) 
             ?? new Dictionary<string, string[]>())
    .HasColumnType("nvarchar(max)"); // For SQL Server 2016-2022
    // or .HasColumnType("json"); // For SQL Server 2025
```

#### Cosmos DB Configuration
```csharp
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

// Comparer update
internal class FieldValueComparer : ValueComparer<Dictionary<string, string[]>>
{
    public FieldValueComparer() : base(
        (a, b) => CompareFieldValues(a, b),
        v => GetHashCode(v),
        v => new Dictionary<string, string[]>(v))
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
```

### 5.3 Factory Method Updates

```csharp
public static class GamesFactory
{
    private static readonly string[] s_colors6 = 
        [Colors.Red, Colors.Green, Colors.Blue, Colors.Yellow, Colors.Purple, Colors.Orange];
    
    private static readonly string[] s_shapes5 = 
        [Shapes.Circle, Shapes.Square, Shapes.Triangle, Shapes.Star, Shapes.Rectangle];

    public static Game CreateGame(string gameType, string playerName)
    {
        Game Create6x4Game() =>
            new(Guid.NewGuid(), gameType, playerName, DateTime.UtcNow, 4, 12)
            {
                FieldValues = new Dictionary<string, string[]>
                {
                    { FieldCategories.Colors, s_colors6 }
                },
                Codes = Random.Shared.GetItems(s_colors6, 4)
            };

        Game Create5x5x4Game() =>
            new(Guid.NewGuid(), gameType, playerName, DateTime.UtcNow, 4, 14)
            {
                FieldValues = new Dictionary<string, string[]>
                {
                    { FieldCategories.Colors, s_colors5 },
                    { FieldCategories.Shapes, s_shapes5 }
                },
                Codes = Random.Shared.GetItems(s_shapes5, 4)
                    .Zip(Random.Shared.GetItems(s_colors5, 4))
                    .Select(item => string.Join(';', item.First, item.Second))
                    .ToArray()
            };
        
        return gameType switch
        {
            GameTypes.Game6x4 => Create6x4Game(),
            GameTypes.Game5x5x4 => Create5x5x4Game(),
            // ... other types
            _ => throw new CodebreakerException("Invalid game type")
        };
    }
}
```

---

## 6. Database Migration Considerations

### 6.1 SQL Server Migration

#### Option A: Keep String Storage Format (Recommended)
**No schema migration needed** - existing data format remains compatible.

**Data Migration:** None required - the string format `"colors:Red#colors:Green"` works identically.

**Rollback:** Easy - revert code changes only.

#### Option B: Migrate to JSON Column
**Schema Migration Required:**

```sql
-- Step 1: Add new JSON column
ALTER TABLE Games ADD FieldsJson NVARCHAR(MAX);

-- Step 2: Migrate data
UPDATE Games 
SET FieldsJson = (
    -- Convert string format to JSON
    -- This requires a complex SQL function or application-level migration
);

-- Step 3: Drop old column (after verification)
ALTER TABLE Games DROP COLUMN Fields;

-- Step 4: Rename column
EXEC sp_rename 'Games.FieldsJson', 'Fields', 'COLUMN';
```

**Data Migration Strategy:**
1. Deploy application with dual-column support
2. Background job to migrate data
3. Verify all records migrated
4. Deploy application with new column only
5. Drop old column

**Rollback:** Complex - requires reverse migration.

### 6.2 PostgreSQL Migration

Similar considerations to SQL Server. PostgreSQL has native JSONB support which could be leveraged:

```sql
-- Migrate to JSONB
ALTER TABLE games ALTER COLUMN fields TYPE JSONB USING fields::jsonb;
```

### 6.3 Cosmos DB Migration

**Cosmos DB is Already Using JSON** - minimal changes needed:

1. Update converter type signatures
2. Deploy new application version
3. Existing documents work with new structure
4. No data migration required (JSON structure remains same)

**Example Document (Before and After):**
```json
{
  "id": "...",
  "fieldValues": {
    "colors": ["Red", "Green", "Blue"],
    "shapes": ["Circle", "Square"]
  }
}
```
Structure remains identical; only .NET types change.

---

## 7. Testing Strategy

### 7.1 Unit Tests to Update

1. **MappingExtensionsTests.cs**
   - Test `ToFieldsString` with `Dictionary<string, string[]>`
   - Test `FromFieldsString` returning `Dictionary<string, string[]>`
   - Test round-trip conversion

2. **GamesFactoryTests.cs**
   - Verify created games have correct `FieldValues` type
   - Test array access patterns

3. **Database Integration Tests**
   - Test SQL Server save/retrieve with new type
   - Test PostgreSQL save/retrieve with new type
   - Test Cosmos DB save/retrieve with new type

### 7.2 Integration Tests

1. **API Tests**
   - Verify JSON serialization in HTTP responses
   - Test client deserialization

2. **Bot Tests**
   - Verify gRPC field conversion
   - Test bot game runner with new types

### 7.3 Backward Compatibility Tests

```csharp
[Fact]
public void FieldValues_CanBeAccessedAsIEnumerable()
{
    var game = GamesFactory.CreateGame(GameTypes.Game6x4, "TestPlayer");
    
    // Should work with IEnumerable access pattern
    IEnumerable<string> colors = game.FieldValues["colors"];
    Assert.NotEmpty(colors);
}

[Fact]
public void FieldValues_CanBeAccessedViaInterface()
{
    Game game = GamesFactory.CreateGame(GameTypes.Game6x4, "TestPlayer");
    IGame iGame = game;
    
    // Should work through interface
    var colors = iGame.FieldValues["colors"];
    Assert.NotEmpty(colors);
}
```

---

## 8. Recommendations

### 8.1 Short-term (Current State - EF Core 9)

**Recommendation: No Changes**

**Rationale:**
1. Current implementation works well with EF Core 9
2. EF Core 10 is not yet released (currently using 9.0.9)
3. Breaking changes should wait for major version bump
4. SQL Server 2025 features not yet GA

**Actions:**
- Continue using current `IDictionary<string, IEnumerable<string>>` type
- Maintain existing custom converters
- Document this investigation for future reference

### 8.2 Medium-term (EF Core 10 Preview/Release)

**Recommendation: Evaluate and Test**

**Actions:**
1. **Test EF Core 10 Preview:**
   - Create branch with EF Core 10 preview packages
   - Test native JSON support for `Dictionary<string, string[]>`
   - Measure query performance improvements
   - Evaluate migration complexity

2. **Prepare Migration Plan:**
   - Document step-by-step migration process
   - Create database migration scripts
   - Plan version bump to 4.0.0
   - Notify consumers of upcoming breaking change

3. **Create Migration Guide:**
   - Document API changes
   - Provide code samples
   - Explain backward compatibility options

### 8.3 Long-term (Version 4.0.0)

**Recommendation: Implement Type Change with Major Version**

**Implementation Plan:**

**Phase 1: Property Type Change**
1. Update `Game.cs` property to `Dictionary<string, string[]>`
2. Update `IGame.cs` interface (or provide explicit implementation)
3. Update all factory methods
4. Update database configurations

**Phase 2: Database Layer**
- **SQL Server:** Keep string storage OR migrate to JSON (decision point)
- **PostgreSQL:** Keep string storage OR migrate to JSONB
- **Cosmos DB:** Update converters (structure unchanged)

**Phase 3: Testing & Validation**
1. Run full test suite
2. Integration test with all databases
3. Performance benchmarks
4. API contract validation

**Phase 4: Documentation & Release**
1. Update API documentation
2. Create migration guide
3. Release as version 4.0.0
4. Support backward compatibility in client libraries if needed

### 8.4 Alternative Approach: Phased Migration

**Option:** Provide both types in transition version 3.9.0

```csharp
public class Game
{
    // Deprecated - to be removed in 4.0.0
    [Obsolete("Use FieldValuesArray instead. Will be removed in version 4.0.0")]
    public IDictionary<string, IEnumerable<string>> FieldValues => 
        FieldValuesArray.ToDictionary(kvp => kvp.Key, kvp => (IEnumerable<string>)kvp.Value);
    
    // New property
    public Dictionary<string, string[]> FieldValuesArray { get; init; }
}
```

**Pros:**
- Gradual migration for consumers
- Clear deprecation warning

**Cons:**
- Temporary code complexity
- Dual property maintenance

---

## 9. Known Limitations and Gaps

### 9.1 EF Core 9 Limitations

1. **No Native Support for Interface Collections:**
   - `IDictionary<string, IEnumerable<string>>` requires custom converters
   - Cannot use native JSON column features

2. **Limited JSON Queryability:**
   - Even with custom converters, LINQ queries inside JSON are limited
   - String-based storage has no queryability

3. **Value Comparison Complexity:**
   - Custom `ValueComparer` needed for change tracking
   - Can impact performance for large dictionaries

### 9.2 SQL Server 2025 Considerations

1. **Not Yet Released:**
   - SQL Server 2025 features not available in production
   - JSON column improvements need validation

2. **Migration Complexity:**
   - Changing storage format requires careful migration
   - Potential downtime during migration

### 9.3 Cross-Database Compatibility

1. **Different Storage Strategies:**
   - SQL Server: String or JSON
   - PostgreSQL: String or JSONB
   - Cosmos DB: Native JSON

2. **Query Capabilities Vary:**
   - Cosmos DB has best JSON query support
   - SQL Server 2025 promises improvements
   - PostgreSQL JSONB is mature but requires migration

---

## 10. Conclusion

### Key Takeaways

1. **Breaking Change Scope:** Moderate impact affecting 8+ core files and additional test files

2. **Current State:** Repository uses EF Core 9.0.9 with custom converters that work well

3. **EF Core 10 Considerations:** 
   - Not yet available (currently on 9.0.9)
   - Will improve JSON support but may still require converters for this scenario
   - Should be tested before committing to changes

4. **Recommended Path:** 
   - **Now:** Document findings (this document)
   - **Next:** Monitor EF Core 10 development and test preview
   - **Future:** Implement type change in major version 4.0.0 after EF Core 10 is stable

5. **Alternative Approach:**
   - Keep interface unchanged (`IDictionary<string, IEnumerable<string>>`)
   - Use explicit implementation for backward compatibility
   - Internal storage as `Dictionary<string, string[]>`

### Decision Points

| Decision | Option A | Option B | Recommendation |
|----------|----------|----------|----------------|
| **When to Change** | Wait for EF Core 10 | Change now with EF Core 9 | Wait for EF Core 10 |
| **Property Type** | `Dictionary<string, string[]>` | Keep interface type | Direct type change |
| **Storage Format** | Migrate to JSON | Keep string format | Keep string format initially |
| **Version** | Major bump (4.0.0) | Minor bump (3.9.0) | Major bump (4.0.0) |
| **Migration** | Big bang | Phased with deprecation | Big bang for 4.0.0 |

### Next Steps

1. ✅ **Complete this investigation** (Done)
2. **Monitor EF Core 10 releases** and test preview packages
3. **Create tracking issue** for version 4.0.0 planning
4. **Wait for EF Core 10 stable release** before implementing changes
5. **Re-evaluate** this document when EF Core 10 is available

---

## Appendix A: Related Resources

### Official Documentation
- [EF Core JSON Columns](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-7.0/whatsnew#json-columns)
- [EF Core 9 What's New](https://learn.microsoft.com/en-us/ef/core/what-is-new/ef-core-9.0/whatsnew)
- [Value Conversions](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions)

### Repository Files
- Main Issue: [#340](https://github.com/CodebreakerApp/Codebreaker.Backend/issues/340)
- Game Model: `src/services/common/Codebreaker.GameAPIs.Models/Game.cs`
- IGame Interface: `src/services/common/Codebreaker.GameAPIs.Analyzers/Contracts/IGame.cs`

### Version Information
- Current .NET: 9.0
- Current EF Core: 9.0.9
- Target EF Core: 10.x (not yet released)

---

## Appendix B: Glossary

- **FieldValues**: Dictionary property storing available game field options (colors, shapes, etc.)
- **Value Converter**: EF Core mechanism to transform property values during database storage
- **Value Comparer**: EF Core mechanism for comparing property values for change tracking
- **Breaking Change**: API modification that requires consumer code updates
- **JSON Column**: Database column type storing JSON data with potential query capabilities

---

**Document Version:** 1.0  
**Last Updated:** October 15, 2025  
**Author:** GitHub Copilot Investigation  
**Status:** Final
