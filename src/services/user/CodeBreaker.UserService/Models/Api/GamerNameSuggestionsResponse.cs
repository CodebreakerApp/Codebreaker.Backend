namespace CodeBreaker.UserService.Models.Api;

public record class GamerNameSuggestionsResponse(IEnumerable<string> suggestions);