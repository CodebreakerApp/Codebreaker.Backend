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
}