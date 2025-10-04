namespace Codebreaker.Identity.Configuration;

/// <summary>
/// Configuration options for anonymous users
/// </summary>
public class AnonymousUserOptions
{
    /// <summary>
    /// Gets or sets the tenant ID for the Azure AD tenant
    /// </summary>
    public string TenantId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the client ID for the Azure AD application used for graph operations
    /// </summary>
    public string ClientId { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the client secret for the Azure AD application
    /// </summary>
    public string ClientSecret { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the domain for anonymous users
    /// </summary>
    public string Domain { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the password policy for anonymous users
    /// </summary>
    public string PasswordPolicy { get; set; } = string.Empty;
    
    /// <summary>
    /// Gets or sets the password length for anonymous users
    /// </summary>
    public int PasswordLength { get; set; } = 12;
    
    /// <summary>
    /// Gets or sets the user name prefix for anonymous users
    /// </summary>
    public string UserNamePrefix { get; set; } = "anon";
}