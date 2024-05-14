namespace CodeBreaker.UserService.Options;

internal class GamerNameSuggestionOptions
{
    public int MinimumNumber { get; set; } = 1;

    public int MaximumNumber { get; set; } = 1000;

    public NameParts GamerNameParts { get; set; } = new ();
}

internal struct NameParts
{
    public NameParts()
    {
    }

    public string[] First { get; set; } = [];

    public string[] Second { get; set; } = [];
}