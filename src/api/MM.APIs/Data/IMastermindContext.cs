namespace MM.APIs.Data;

public interface IMastermindContext
{
    Task AddGameAsync(MasterMindGame game);
    Task AddMoveAsync(MasterMindGameMove move);
}
