﻿# CNinnovation.Codebreaker.GamesClient

This library contains the `GamesClient` class to communicate with the Codebreaker service, and model types that are used for the communication.

See https://github.com/codebreakerapp for more information on the complete solution.

See [Codebreakerlight](https://github.com/codebreakerapp/codebreakerlight) for a simple version of the Codebreaker solution with a Wiki to create your own Codebreaker service.

## The GamesClient class and IGamesClient interface

The `IGamesClient` class is the main contract to be used for communication to play the game. It contains the following methods:

| Method     | Description        |
|------------|--------------------|
| `StartGameAsync` | Start a new game |
| `SetMoveAsync` | Set guesses for a game move |
| `GetGameAsync` | Get a game by id with all details and moves |
| `GetGamesAsync` | Get a list of games with all details and moves (use the `GamesQuery` class to define the filter) |
| `RevealGameAsync` | Ends a game and returns the correct answer |

The `GamesClient` class implements the `IGamesClient` interface. In the constructor, inject the `HttpClient` class. You can use `Microsoft.Extensions.Http` to configure the `HttpClient` class.

## Model types

The following model types are used to return information about the game.

| Model type | Description |
|------------|-------------|
| `GameType` | Enum value to list different game types |
| `GamesQuery` | Use this class to query for game info lists using `GetGamesAsync` |
| `GameInfo` | Contains the game id, the game status, the game moves and the game result |
| `MoveInfo` | Contains the move number, the guess and the result of the guess. Contained within a `GameInfo` |

## How to use

The following code snippet shows how to use the `GamesClient` class to play the game.

Configure the DI container with the GamesClient class (use the NuGet package `Microsoft.Extensions.Http` to configure the `HttpClient` class):

```csharp
services.AddHttpClient<IGamesClient, GamesClient>(client =>
{
	client.BaseAddress = new Uri("https://localhost:5001");
});
```

Start a game:

```csharp
(Guid id, int numberCodes, int maxMoves, IDictionary<string, string[]> fieldValues) = await gamesClient.StartGameAsync("Game6x4", "player1");
```

The returned fieldValues contains an array of possible values for code fields, with the key being "colors". With a Game5x5x4 game, the returned fieldValues contains a key "shapes", and a key "colors".

Set a move:

```csharp
(string[] result, bool ended, bool isVictory) = await gameClient.SetMoveAsync(id, "Game6x4", [ "Red", "Green", "Blue", "Yellow" ]);
```

With a Game5x5x4 game, you can set a move like this:

```csharp
(string[] result, bool ended, bool isVictory) = await gameClient.SetMoveAsync(id, "Game5x5x4", [ "Circle;Red", "Rectangle;Green", "Triangle;Blue", "Circle;Yellow" ]);
```