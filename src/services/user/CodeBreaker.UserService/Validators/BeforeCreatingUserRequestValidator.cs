using Azure.Identity;
using CodeBreaker.UserService.Models.Api;
using CodeBreaker.UserService.Options;
using FluentValidation;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Options;
using Microsoft.Graph;

namespace CodeBreaker.UserService.Validators;

internal class BeforeCreatingUserRequestValidator : AbstractValidator<BeforeCreatingUserRequest>, IDisposable
{
    private readonly GraphServiceClient _graphServiceClient;

    private readonly string _gamerNameAttributeKey;

    public BeforeCreatingUserRequestValidator(IOptions<GamerNameCheckOptions> gamerNameCheckOptions, IStringLocalizer<BeforeCreatingUserRequestValidator> localizer)
    {
        // Setup
        _gamerNameAttributeKey = gamerNameCheckOptions.Value.GamerNameAttributeKey;
        string[] scopes = ["https://graph.microsoft.com/.default"];
        TokenCredentialOptions options = new() { AuthorityHost = AzureAuthorityHosts.AzurePublicCloud };
        ClientSecretCredential clientSecretCredential = new(gamerNameCheckOptions.Value.TenantId, gamerNameCheckOptions.Value.ClientId, gamerNameCheckOptions.Value.ClientSecret, options);
        _graphServiceClient = new(clientSecretCredential, scopes);

        // Validation rules
        RuleFor(x => x.Email)
            .EmailAddress()
            .NotEmpty()
            .MinimumLength(7)
            .MaximumLength(200);
        RuleFor(x => x.GamerName)
            .NotEmpty()
            .MinimumLength(3)
            .MaximumLength(100)
            .MustAsync(DoesGamerNameExistAsync)
            .WithMessage(localizer["GamerNameTakenMessage"]);
        RuleFor(x => x.GivenName)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
        RuleFor(x => x.Surname)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100);
    }

    public void Dispose()
    {
        _graphServiceClient.Dispose();
    }

    /// <summary>
    /// Checks whether the specified gamer name exists.
    /// </summary>
    /// <param name="gamerName">The gamer name to check.</param>
    /// <param name="cancellationToken"></param>
    /// <returns>True if the gamer name exists, false otherwise.</returns></returns>
    /// <remarks>
    /// In AAD this service requires the permission <b>User.Read.All</b> in order to check whether the gamer name exists.
    /// </remarks>
    private async Task<bool> DoesGamerNameExistAsync(string gamerName, CancellationToken cancellationToken)
    {
        var response = await _graphServiceClient.Users
        .GetAsync(builder =>
        {
            builder.QueryParameters.Top = 1;
                builder.QueryParameters.Select = ["id"];
                builder.QueryParameters.Filter = $"{_gamerNameAttributeKey} eq '{gamerName}'";
            }, cancellationToken);
        return response?.Value?.Count == 0;
    }
}
