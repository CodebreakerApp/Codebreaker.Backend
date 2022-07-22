using CodeBreaker.Shared;

namespace CodeBreaker.Services;
public interface IGameClient
{
    Task<(bool Completed, bool Won, string[] KeyPegs)> SetMoveAsync(Guid gameId, int moveNumber, params string[] colorNames);
    Task<CreateGameResponse> StartGameAsync(string name);
}
