namespace CodeBreaker.Services;

public interface INavigationServiceCore
{
    bool NavigateTo(string pageKey, object? parameter = null, bool clearNavigation = false);

    bool GoBack();
}
