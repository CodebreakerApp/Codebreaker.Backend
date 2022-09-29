﻿using CodeBreaker.APIs.Factories.GameTypeFactories;
using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.APIs.Services
{
    public interface IGameService
    {
        ValueTask<Game?> GetAsync(Guid id);
        
        IAsyncEnumerable<Game> GetByDate(DateOnly date);

        IAsyncEnumerable<Game> GetByDate(DateTime date);

        Task<Game> CreateAsync(string username, GameTypeFactory<string> gameTypeFactory);

        Task CancelAsync(Guid id);

        Task DeleteAsync(Guid id);
    }
}
