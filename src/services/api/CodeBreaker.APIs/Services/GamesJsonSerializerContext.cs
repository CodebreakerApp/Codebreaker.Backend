using System.Text.Json.Serialization;

using CodeBreaker.Shared.Models.Api;
using CodeBreaker.Shared.Models.Data;

namespace CodeBreaker.APIs.Services;

[JsonSourceGenerationOptions(
    WriteIndented = false,
    IgnoreReadOnlyProperties = false,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
[JsonSerializable(typeof(GetGameResponse))]
[JsonSerializable(typeof(GetGameTypesResponse))]
[JsonSerializable(typeof(GetGamesResponse))]
[JsonSerializable(typeof(CreateGameRequest))]
[JsonSerializable(typeof(CreateMoveRequest))]
[JsonSerializable(typeof(CreateMoveResponse))]
[JsonSerializable(typeof(Game))]
internal partial class GamesJsonSerializerContext : JsonSerializerContext
{
}
