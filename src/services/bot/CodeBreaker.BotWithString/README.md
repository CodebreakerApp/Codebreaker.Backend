# CodeBreaker.BotWithString

A string-based implementation of the Codebreaker bot that works with the Games API using string arrays instead of binary data.

## Overview

This project provides a string-based alternative to the original `CodeBreaker.Bot` project. Instead of using binary data and bit manipulation, this implementation works directly with string arrays representing colors, making it easier to understand and integrate with string-based APIs.

## Key Features

- **String-based algorithms**: All core algorithms work with `string[]` arrays instead of `int` binary representations
- **GameAPIs Client compatibility**: Works seamlessly with the `IGamesClient` interface
- **Support for all game types**: Game6x4, Game8x5, and Game5x5x4
- **Comprehensive algorithm set**:
  - `HandleBlackMatches`: Filters by exact position and color matches
  - `HandleWhiteMatches`: Filters by correct color, wrong position matches
  - `HandleBlueMatches`: Handles partial matches for Game5x5x4
  - `HandleNoMatches`: Removes combinations containing selection colors
  - `SelectPeg`: Gets specific peg from string array
  - `GenerateAllPossibleCombinations`: Creates all possible game combinations

## Core Components

### StringCodeBreakerAlgorithms

The main class containing all string-based algorithms:

```csharp
// Example usage
var possibleCombinations = StringCodeBreakerAlgorithms.GenerateAllPossibleCombinations(
    GameType.Game6x4, 
    new[] { "Red", "Blue", "Green", "Yellow" });

// Filter based on black matches (exact position and color)
var filtered = possibleCombinations.HandleBlackMatches(
    GameType.Game6x4, 
    2, // number of black hits
    new[] { "Red", "Blue", "Green", "Yellow" }); // the guess
```

### StringBotGameRunner

A demonstration class showing how to use the string-based algorithms with the GameAPIs client:

```csharp
var runner = new StringBotGameRunner(gamesClient);
var result = await runner.PlayGameAsync(GameType.Game6x4, "PlayerName");
```

## Key Differences from Binary Version

| Aspect | Binary Version | String Version |
|--------|----------------|----------------|
| Data representation | `int` with bit manipulation | `string[]` arrays |
| Color handling | Bit masks and shifts | Direct string comparison |
| Algorithm complexity | Bit operations | Simple array operations |
| API compatibility | Requires conversion | Direct compatibility |
| Readability | Lower (bit manipulation) | Higher (string operations) |

## Testing

The project includes comprehensive unit tests covering:

- All algorithm methods for different game types
- Edge cases and error conditions
- Game runner functionality with mocked clients
- Parameter validation

Run tests with:
```bash
dotnet test CodeBreaker.BotWithString.Tests.csproj
```

## Usage in Benchmarks

This string-based implementation is designed to be integrated into bot benchmark projects to compare performance and behavior with the binary version. The simplified string operations may have different performance characteristics compared to bit manipulation, making it valuable for performance analysis.

## API Compatibility

The string-based algorithms are fully compatible with the `IGamesClient` interface, which uses string-based methods:

- `StartGameAsync` returns field values as `string[]`
- `SetMoveAsync` accepts guesses as `string[]`
- Results are returned as `string[]`

This makes integration straightforward without requiring data conversion layers.