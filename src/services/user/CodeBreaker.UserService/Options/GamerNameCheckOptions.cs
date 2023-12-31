﻿namespace CodeBreaker.UserService.Options;

internal class GamerNameCheckOptions
{
    public string TenantId { get; set; } = string.Empty;

    public string ClientId { get; set; } = string.Empty;

    public string ClientSecret { get; set; } = string.Empty;

    public string GamerNameAttributeKey { get; set; } = string.Empty;
}
