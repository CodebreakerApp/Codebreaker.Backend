using Codebreaker.Identity.Models;

namespace Codebreaker.Identity.Services;

/// <summary>
/// Defines operations for managing anonymous users
/// </summary>
public interface IAnonymousUserService
{
    /// <summary>
    /// Creates an anonymous user with the specified user name
    /// </summary>
    /// <param name="userName">The user name</param>
    /// <returns>An anonymous user object with credentials</returns>
    Task<AnonymousUser> CreateAnonUser(string userName);
    
    /// <summary>
    /// Deletes anonymous users that haven't logged in for at least three months
    /// </summary>
    /// <returns>The number of deleted users</returns>
    Task<int> DeleteAnonUsers();

    /// <summary>
    /// Promotes an anonymous user to a registered user
    /// </summary>
    /// <param name="anonymousUserId">The ID of the anonymous user</param>
    /// <param name="email">The email address for the registered user</param>
    /// <param name="displayName">The display name for the registered user</param>
    /// <param name="password">The password for the registered user</param>
    /// <returns>The updated user object</returns>
    Task<AnonymousUser> PromoteAnonUser(string anonymousUserId, string email, string displayName, string password);
}