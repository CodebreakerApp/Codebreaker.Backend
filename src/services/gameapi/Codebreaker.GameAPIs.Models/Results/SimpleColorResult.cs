namespace Codebreaker.GameAPIs.Models;

public enum ResultValue : byte
{
    Incorrect = 0,
    CorrectColor = 1,
    CorrectPositionAndColor = 2
}

public readonly partial struct SimpleColorResult
{
    private readonly ResultValue[] _results;
    public SimpleColorResult(ResultValue[] results) =>
        _results = results;
}
