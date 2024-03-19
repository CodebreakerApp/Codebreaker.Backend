namespace Codebreaker.GameAPIs.Client.Serialization;

[JsonSourceGenerationOptions(
    AllowTrailingCommas = true,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    UseStringEnumConverter = true
)]
[JsonSerializable(typeof(CreateGameRequest))]
[JsonSerializable(typeof(CreateGameResponse))]
[JsonSerializable(typeof(GameInfo))]  // MoveInfo is contained by GameInfo
[JsonSerializable(typeof(GamesQuery))]
[JsonSerializable(typeof(UpdateGameRequest))]
[JsonSerializable(typeof(UpdateGameResponse))]
internal partial class GamesClientJsonSerializerContext : JsonSerializerContext
{
}
