namespace Codebreaker.GameAPIs.Extensions;

public static class GameExtensions
{
    public static bool Ended(this Game game) => game.EndTime != null;

    public static void ApplyMove(this Game game, Move move)
    {
        if (game is SimpleGame sg && move is SimpleMove sm)
        {
            sg.ApplySimpleMove(sm);
        }
        else if (game is ColorGame cg && move is ColorMove cm)
        {
            cg.ApplyColorMove(cm);
        }
        else if (game is ShapeGame scg && move is ShapeMove scm)
        {
            scg.ApplyShapeMove(scm);
        }
        else
        {
            throw new InvalidOperationException("invalid game");
        }
    }
}
