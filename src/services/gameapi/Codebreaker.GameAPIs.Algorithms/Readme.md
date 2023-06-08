# Codebreaker.GameAPIs.Algorithms

This library contains algorithms for the Codebreaker app. Reference this library when creating a Codebreaker service.
See https://github.com/codebreakerapp for more information on the complete solution.

See [Codebreakerlight](https://github.com/codebreakerapp/codebreakerlight) for a simple version of the Codebreaker solution with a Wiki to create your own Codebreaker service.

## Types available in this package

### Contracts, namespace Codebreaker.GameAPIs.Contracts

|----------|--------------------|
| Type     | Description        |
|----------|--------------------|
| IGame    | Implement this interface with your game model. This is required by the anylzer |
| IMove    | Implement this interface with your move model. This is required by the anylzer |
| IGameMoveAnalyzer | If you want to create your own game type, create an anaylzer implementing this interface. Instead, you can derive your analyzer type from the base class `GameMoveAnalyzer` |
|----------|--------------------|

### Analyzers, namespace Codebreaker.GameAPIs.Analyzers

|----------|--------------------|
| Type     | Description        |
|----------|--------------------|
| GameMoveAnalyzer  | This is the base class of all analyzers. Derive from this class when you create your own game type |
| ColorGameMoveAnalyzer | This anaylzer uses the types `ColorField` and `ColorResult` to analyze games moves with a list of colors. |
| SimpleGameMoveAnalyzer | This anaylzer implements the children-mode of the game and uses the types `ColorField` and `SimpleColorResult` to analyze games moves with a list of colors. |
| ShapeGameMoveAnalyzer | This anaylzer uses the types `ShapeAndColorField` and `ShapeAndColorResult` to analyze games moves with a list of shapes and colors. |
|----------|--------------------|

### Field and Result Types, namespace Codebreaker.GameAPIs.Models

|----------|--------------------|
| Type     | Description        |
|----------|--------------------|
[ GameTypes | Constants for available game types. |
| ColorField  | A field type for color fields |
| ShapeAndColorField | A field type for shape and color fields |
| ColorResult | A result type with Correct and WrongPosition  numbers |
| ShapeAndColorResult | A result type with Correct, WrongPosition and ColorOrShape (either the color or the shape is correct) numbers |
| SimpleColorResult | A result type with a list to show positional results using the `ResultValue` enum. |
|----------|--------------------|
