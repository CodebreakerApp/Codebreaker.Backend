using Codebreaker.GameAPIs.Contracts;

namespace Codebreaker.GameAPIs.Analyzers;

public abstract class GameGuessAnalyzer<TField, TResult> : IGameGuessAnalyzer<TResult>
    where TResult : struct
{
    protected readonly IGame _game;
    private readonly int _moveNumber;

    protected TField[] Guesses { get; private set; }

    protected GameGuessAnalyzer(IGame game, TField[] guesses, int moveNumber)
    {
        _game = game;
        Guesses = guesses;
        _moveNumber = moveNumber;
    }

    /// <summary>
    /// Gets the results using Guesses
    /// </summary>
    /// <returns></returns>
    protected abstract TResult GetCoreResult();

    /// <summary>
    /// Override this method to validate the values of the guess with the current state of the game.
    /// </summary>
    protected abstract void ValidateGuessValues();

    /// <summary>
    /// Validates the values of the guess with the current state of the game.
    /// </summary>
    /// <exception cref="ArgumentException">Thrown with an invalid number of guesses (HRESULT=4200), an unexpected move number (4300), or invalid values (44xx)</exception>
    private void ValidateGuess()
    {
        /// The number of holes in the game does not match the number of pegs in the move.
        if (_game.NumberCodes != Guesses.Length)
            throw new ArgumentException($"Invalid guess number {Guesses.Length} for {_game.NumberCodes} code numbers") { HResult = 4200 };

        ValidateGuessValues();

        _game.LastMoveNumber++;

        if (_game.LastMoveNumber != _moveNumber)
        {
            throw new ArgumentException($"Incorrect move number received {_moveNumber}") { HResult = 4300 };
        }
    }

    /// <summary>
    /// Sets the game end information if the guess is correct, or the maximum number of moves reached.
    /// Override this method in concrete guess analyzers to set the result of the game.
    /// </summary>
    /// <param name="result">The result of the guess analysis</param>
    protected abstract void SetGameEndInformation(TResult result);

    public TResult GetResult()
    {
        ValidateGuess();
        TResult result = GetCoreResult();
        SetGameEndInformation(result);
        return result;
    }
}
