namespace Codebreaker.GameAPIs.Client.Serialization;


[JsonSourceGenerationOptions(PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(CreateGameRequest))]
[JsonSerializable(typeof(CreateGameResponse))]
[JsonSerializable(typeof(IEnumerable<GameInfo>))]  // MoveInfo is contained by GameInfo
[JsonSerializable(typeof(GamesQuery))]
[JsonSerializable(typeof(UpdateGameRequest))]
[JsonSerializable(typeof(UpdateGameResponse))]
internal partial class GamesClientJsonSerializerContext : JsonSerializerContext
{
}
