using Codebreaker.Identity.Services;
using Microsoft.Extensions.Logging;
using Moq;

namespace Codebreaker.Identity.Tests;

public class MockAnonymousUserServiceTests
{
    private readonly Mock<ILogger<MockAnonymousUserService>> _mockLogger = new();

    [Fact]
    public async Task CreateAnonUser_ShouldCreateAndReturnUser()
    {
        // Arrange
        var service = new MockAnonymousUserService(_mockLogger.Object);
        string userName = "TestUser";

        // Act
        var result = await service.CreateAnonUser(userName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userName, result.UserName);
        Assert.Equal(userName, result.DisplayName);
        Assert.NotEmpty(result.Id);
        Assert.NotEmpty(result.Password);
        Assert.NotEmpty(result.Email);
        
        // Verify user was added to the internal collection
        var users = service.GetUsers();
        Assert.Single(users);
        Assert.Equal(userName, users[0].UserName);
    }

    [Fact]
    public async Task CreateAnonUser_WithEmptyName_ShouldUseDefaultName()
    {
        // Arrange
        var service = new MockAnonymousUserService(_mockLogger.Object);
        string userName = string.Empty;

        // Act
        var result = await service.CreateAnonUser(userName);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(string.Empty, result.UserName);
        Assert.Equal("Anonymous User", result.DisplayName);
    }

    [Fact]
    public async Task DeleteAnonUsers_ShouldRemoveStaleUsers()
    {
        // Arrange
        var service = new MockAnonymousUserService(_mockLogger.Object);
        
        // Add a recent user
        var recentUser = await service.CreateAnonUser("RecentUser");
        
        // Add a stale user (creation time > 3 months ago)
        var staleUsers = new List<Models.AnonymousUser>
        {
            new()
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "StaleUser1",
                DisplayName = "Stale User 1",
                Email = "stale1@example.com",
                Password = "password1",
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-4)
            },
            new()
            {
                Id = Guid.NewGuid().ToString(),
                UserName = "StaleUser2",
                DisplayName = "Stale User 2",
                Email = "stale2@example.com",
                Password = "password2",
                CreatedAt = DateTimeOffset.UtcNow.AddMonths(-3).AddDays(-1),
                LastLoginAt = DateTimeOffset.UtcNow.AddMonths(-4)
            }
        };

        // Add the stale users to the service's internal collection using reflection
        var usersField = typeof(MockAnonymousUserService).GetField("_users", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var usersList = (List<Models.AnonymousUser>)usersField!.GetValue(service)!;
        usersList.AddRange(staleUsers);

        // Act
        int deletedCount = await service.DeleteAnonUsers();

        // Assert
        Assert.Equal(2, deletedCount);
        var remainingUsers = service.GetUsers();
        Assert.Single(remainingUsers);
        Assert.Equal(recentUser.Id, remainingUsers[0].Id);
    }

    [Fact]
    public async Task PromoteAnonUser_ShouldUpdateUserProperties()
    {
        // Arrange
        var service = new MockAnonymousUserService(_mockLogger.Object);
        var user = await service.CreateAnonUser("AnonUser");
        
        string newEmail = "registered@example.com";
        string newDisplayName = "Registered User";
        string newPassword = "SecurePassword123!";

        // Act
        var result = await service.PromoteAnonUser(user.Id, newEmail, newDisplayName, newPassword);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(user.Id, result.Id);  // ID should remain the same
        Assert.Equal(newEmail, result.Email);
        Assert.Equal(newDisplayName, result.DisplayName);
        Assert.Equal(newPassword, result.Password);
        Assert.NotNull(result.LastLoginAt);  // Should set last login time
        
        // Verify the user was updated in the collection
        var users = service.GetUsers();
        Assert.Single(users);
        Assert.Equal(newEmail, users[0].Email);
        Assert.Equal(newDisplayName, users[0].DisplayName);
    }

    [Fact]
    public async Task PromoteAnonUser_WithInvalidId_ShouldThrowException()
    {
        // Arrange
        var service = new MockAnonymousUserService(_mockLogger.Object);
        string invalidId = "non-existent-id";

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(async () => 
            await service.PromoteAnonUser(invalidId, "email@example.com", "Display Name", "password"));
    }

    [Fact]
    public void GetUsers_ShouldReturnReadOnlyListOfUsers()
    {
        // Arrange
        var service = new MockAnonymousUserService(_mockLogger.Object);

        // Act
        var users = service.GetUsers();

        // Assert
        Assert.NotNull(users);
        Assert.IsAssignableFrom<IReadOnlyList<Models.AnonymousUser>>(users);
    }

    [Fact]
    public async Task ClearUsers_ShouldRemoveAllUsers()
    {
        // Arrange
        var service = new MockAnonymousUserService(_mockLogger.Object);
        _ = await service.CreateAnonUser("User1");
        _ = await service.CreateAnonUser("User2");
        Assert.Equal(2, service.GetUsers().Count);

        // Act
        service.ClearUsers();

        // Assert
        Assert.Empty(service.GetUsers());
    }
}