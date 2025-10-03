using Codebreaker.Identity.Models;
using Codebreaker.Identity.Services;
using Microsoft.Extensions.Logging;

namespace Codebreaker.Identity.Tests;

/// <summary>
/// A mock implementation of <see cref="IAnonymousUserService"/> for testing
/// </summary>
public class MockAnonymousUserService : IAnonymousUserService
{
    private readonly ILogger<MockAnonymousUserService> _logger;
    private readonly List<AnonymousUser> _users = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="MockAnonymousUserService"/> class
    /// </summary>
    /// <param name="logger">The logger</param>
    public MockAnonymousUserService(ILogger<MockAnonymousUserService> logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public Task<AnonymousUser> CreateAnonUser(string userName)
    {
        _logger.LogInformation("Creating mock anonymous user: {UserName}", userName);

        var user = new AnonymousUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = userName,
            DisplayName = string.IsNullOrWhiteSpace(userName) ? "Anonymous User" : userName,
            Email = $"anon-{Guid.NewGuid()}@example.com",
            Password = $"Password_{Guid.NewGuid().ToString().Substring(0, 8)}",
            CreatedAt = DateTimeOffset.UtcNow
        };

        _users.Add(user);
        return Task.FromResult(user);
    }

    /// <inheritdoc />
    public Task<int> DeleteAnonUsers()
    {
        _logger.LogInformation("Deleting stale anonymous users");

        // Delete users older than 3 months
        DateTimeOffset cutoffDate = DateTimeOffset.UtcNow.AddMonths(-3);
        int count = _users.RemoveAll(u => 
            (u.LastLoginAt == null && u.CreatedAt < cutoffDate) || 
            (u.LastLoginAt != null && u.LastLoginAt < cutoffDate));

        _logger.LogInformation("Deleted {Count} stale anonymous users", count);
        return Task.FromResult(count);
    }

    /// <inheritdoc />
    public Task<AnonymousUser> PromoteAnonUser(string anonymousUserId, string email, string displayName, string password)
    {
        _logger.LogInformation("Promoting mock anonymous user: {UserId}", anonymousUserId);

        var user = _users.FirstOrDefault(u => u.Id == anonymousUserId);
        if (user == null)
        {
            throw new InvalidOperationException($"User with ID {anonymousUserId} not found");
        }

        // Update the user properties
        user.Email = email;
        user.DisplayName = displayName;
        user.Password = password;
        user.LastLoginAt = DateTimeOffset.UtcNow;

        return Task.FromResult(user);
    }

    /// <summary>
    /// Gets the current anonymous users
    /// </summary>
    /// <returns>The list of anonymous users</returns>
    public IReadOnlyList<AnonymousUser> GetUsers() => _users.AsReadOnly();
    
    /// <summary>
    /// Clears all users
    /// </summary>
    public void ClearUsers() => _users.Clear();
}