using Azure.Identity;
using Codebreaker.Identity.Configuration;
using Codebreaker.Identity.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using System.Security.Cryptography;

namespace Codebreaker.Identity.Services;

/// <summary>
/// Implementation of the anonymous user service using Microsoft Graph API
/// </summary>
public class GraphAnonymousUserService : IAnonymousUserService
{
    private readonly GraphServiceClient _graphClient;
    private readonly AnonymousUserOptions _options;
    private readonly ILogger<GraphAnonymousUserService> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="GraphAnonymousUserService"/> class
    /// </summary>
    /// <param name="options">The anonymous user options</param>
    /// <param name="logger">The logger</param>
    public GraphAnonymousUserService(
        IOptions<AnonymousUserOptions> options,
        ILogger<GraphAnonymousUserService> logger)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        // Initialize the Graph client with client credentials
        var credential = new ClientSecretCredential(
            _options.TenantId,
            _options.ClientId,
            _options.ClientSecret);

        _graphClient = new GraphServiceClient(credential);
    }

    /// <inheritdoc />
    public async Task<AnonymousUser> CreateAnonUser(string userName)
    {
        _logger.LogInformation("Creating anonymous user with name {UserName}", userName);

        // Generate a secure random password
        string password = GenerateSecurePassword(_options.PasswordLength);
        
        // Create a unique email with the provided domain
        string email = $"{_options.UserNamePrefix}-{Guid.NewGuid()}@{_options.Domain}";
        
        // Prepare the display name
        string displayName = string.IsNullOrWhiteSpace(userName) ? "Anonymous User" : userName;
        
        try
        {
            // Create the user in Azure AD via Microsoft Graph
            User newUser = new()
            {
                AccountEnabled = true,
                DisplayName = displayName,
                MailNickname = _options.UserNamePrefix + Guid.NewGuid().ToString("N")[..8],
                UserPrincipalName = email,
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = false,
                    Password = password
                }
            };

            var createdUser = await _graphClient.Users.PostAsync(newUser);

            if (createdUser == null)
            {
                throw new InvalidOperationException("Failed to create anonymous user");
            }

            // Update the user with extension properties to mark as anonymous
            await _graphClient.Users[createdUser.Id].PatchAsync(new User
            {
                AdditionalData = new Dictionary<string, object>
                {
                    { "extension_AnonymousUser", true },
                    { "extension_CreatedAt", DateTimeOffset.UtcNow.ToString("o") }
                }
            });

            // Return the anonymous user details
            return new AnonymousUser
            {
                Id = createdUser.Id ?? string.Empty,
                UserName = createdUser.UserPrincipalName ?? email,
                DisplayName = createdUser.DisplayName ?? displayName,
                Email = createdUser.UserPrincipalName ?? email,
                Password = password,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create anonymous user {UserName}: {Message}", userName, ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<int> DeleteAnonUsers()
    {
        _logger.LogInformation("Deleting anonymous users that haven't logged in for at least three months");
        
        try
        {
            // Calculate the cutoff date (three months ago)
            DateTimeOffset cutoffDate = DateTimeOffset.UtcNow.AddMonths(-3);
            string cutoffDateString = cutoffDate.ToString("o");
            
            // Query for anonymous users created before cutoff date
            var usersToDelete = await _graphClient.Users
                .GetAsync(requestConfig =>
                {
                    requestConfig.QueryParameters.Filter = $"extension_AnonymousUser eq true";
                    requestConfig.QueryParameters.Select = ["id", "displayName", "extension_CreatedAt", "signInActivity"];
                });

            if (usersToDelete?.Value == null)
            {
                _logger.LogInformation("No anonymous users found");
                return 0;
            }

            // Filter users based on lastSignInDateTime or createdDateTime
            var staleUsers = usersToDelete.Value.Where(user => 
            {
                // Try to get the creation date from extension
                if (user.AdditionalData?.TryGetValue("extension_CreatedAt", out var createdAtObj) == true &&
                    createdAtObj is string createdAtStr && 
                    DateTimeOffset.TryParse(createdAtStr, out var createdAt))
                {
                    // If the user has sign-in activity, check if last sign-in was before cutoff date
                    if (user.SignInActivity?.LastSignInDateTime != null)
                    {
                        return user.SignInActivity.LastSignInDateTime < cutoffDate;
                    }
                    
                    // If no sign-in activity, check if creation date is before cutoff date
                    return createdAt < cutoffDate;
                }
                
                // Default to not delete if we can't determine creation date
                return false;
            }).ToList();

            // Delete the stale users
            int deletedCount = 0;
            foreach (var user in staleUsers)
            {
                if (user.Id == null)
                {
                    continue;
                }
                
                await _graphClient.Users[user.Id].DeleteAsync();
                deletedCount++;
                
                _logger.LogInformation("Deleted anonymous user: {UserId}, {DisplayName}", 
                    user.Id, user.DisplayName);
            }

            _logger.LogInformation("Deleted {Count} anonymous users", deletedCount);
            return deletedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete anonymous users: {Message}", ex.Message);
            throw;
        }
    }

    /// <inheritdoc />
    public async Task<AnonymousUser> PromoteAnonUser(string anonymousUserId, string email, string displayName, string password)
    {
        _logger.LogInformation("Promoting anonymous user {UserId} to registered user", anonymousUserId);

        try
        {
            // First, verify the user exists and is actually an anonymous user
            var user = await _graphClient.Users[anonymousUserId].GetAsync(requestConfig =>
            {
                requestConfig.QueryParameters.Select = ["id", "displayName", "userPrincipalName", "mail", "extension_AnonymousUser"];
            });

            if (user == null)
            {
                throw new InvalidOperationException($"User with ID {anonymousUserId} not found");
            }

            // Verify this is an anonymous user
            if (user.AdditionalData == null || 
                !user.AdditionalData.TryGetValue("extension_AnonymousUser", out var isAnonObj) ||
                isAnonObj is not bool isAnon || 
                !isAnon)
            {
                throw new InvalidOperationException($"User with ID {anonymousUserId} is not an anonymous user");
            }

            // Update the user properties but keep the same ID
            await _graphClient.Users[anonymousUserId].PatchAsync(new User
            {
                DisplayName = displayName,
                UserPrincipalName = email,
                Mail = email,
                PasswordProfile = new PasswordProfile
                {
                    ForceChangePasswordNextSignIn = false,
                    Password = password
                },
                // Remove the anonymous user flag and add promoted flag
                AdditionalData = new Dictionary<string, object>
                {
                    { "extension_AnonymousUser", false },
                    { "extension_PromotedAt", DateTimeOffset.UtcNow.ToString("o") }
                }
            });

            // Get the updated user to return
            var updatedUser = await _graphClient.Users[anonymousUserId].GetAsync();

            if (updatedUser == null)
            {
                throw new InvalidOperationException($"Failed to retrieve updated user with ID {anonymousUserId}");
            }

            // Return the updated user details
            return new AnonymousUser
            {
                Id = updatedUser.Id ?? anonymousUserId,
                UserName = updatedUser.UserPrincipalName ?? email,
                DisplayName = updatedUser.DisplayName ?? displayName,
                Email = updatedUser.Mail ?? email,
                Password = password,
                CreatedAt = DateTimeOffset.Parse(user.AdditionalData?["extension_CreatedAt"]?.ToString() ?? DateTimeOffset.UtcNow.ToString()),
                LastLoginAt = updatedUser.SignInActivity?.LastSignInDateTime ?? DateTimeOffset.UtcNow
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to promote anonymous user {UserId}: {Message}", anonymousUserId, ex.Message);
            throw;
        }
    }

    private static string GenerateSecurePassword(int length)
    {
        const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        const string digitChars = "0123456789";
        const string specialChars = "!@#$%^&*()-_=+[]{}|;:,.<>?";
        
        // Ensure password meets complexity requirements with at least one of each type
        char[] password = new char[length];
        
        using var rng = RandomNumberGenerator.Create();
        byte[] randomBytes = new byte[length];
        rng.GetBytes(randomBytes);
        
        // First, ensure at least one of each character type
        password[0] = uppercaseChars[randomBytes[0] % uppercaseChars.Length];
        password[1] = lowercaseChars[randomBytes[1] % lowercaseChars.Length];
        password[2] = digitChars[randomBytes[2] % digitChars.Length];
        password[3] = specialChars[randomBytes[3] % specialChars.Length];
        
        // Fill the rest randomly
        string allChars = uppercaseChars + lowercaseChars + digitChars + specialChars;
        for (int i = 4; i < length; i++)
        {
            password[i] = allChars[randomBytes[i] % allChars.Length];
        }
        
        // Shuffle the password characters
        for (int i = 0; i < length; i++)
        {
            int swapIndex = randomBytes[i] % length;
            (password[i], password[swapIndex]) = (password[swapIndex], password[i]);
        }
        
        return new string(password);
    }
}