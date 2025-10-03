namespace Codebreaker.Identity.Models;

/// <summary>
/// Represents an anonymous user with access credentials
/// </summary>
public class AnonymousUser
{
    /// <summary>
    /// Gets or sets the user's unique identifier
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the user name
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the display name
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the password
    /// </summary>
    public string Password { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the creation date
    /// </summary>
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    
    /// <summary>
    /// Gets or sets the last login date
    /// </summary>
    public DateTimeOffset? LastLoginAt { get; set; }
}