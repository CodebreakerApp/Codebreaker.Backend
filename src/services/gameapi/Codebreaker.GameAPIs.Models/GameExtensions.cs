namespace Codebreaker.GameAPIs.Models;

public static class GameExtensions
{
    public static bool Ended(this Game game) => game.Endtime is not null;

    public static TResult AddMove<TField, TResult>(this Game<TField, TResult> game, IEnumerable<TField> fields)
      where TResult : IParsable<TResult>
    {
        int lastMove = 0;
        if (game._moves.Count > 0)
        {
            lastMove = game._moves.Last().MoveNumber;
        }

        // calculate result - TODO: based on the code list, this is just a dummy implementation returning sample values
        TResult result = game switch
        {
            { GameType: GameType.Game6x4 } => TResult.Parse("2:0", default),
            { GameType: GameType.Game8x5 } => TResult.Parse("1:2", default),
            { GameType: GameType.Game5x5x4 } => TResult.Parse("1:1:0", default),
            { GameType: GameType.Game6x4Mini } => TResult.Parse("0:1:1:2", default),
            _ => default,
        } ?? throw new InvalidOperationException();

        // create move
        Move<TField, TResult> move = new(game.GameId, Guid.NewGuid(), ++lastMove)
        {
            GuessPegs = fields.ToArray(),
            KeyPegs = result
        };
        // add move to list
        game._moves.Add(move);
        return result;
    }
}
