# CNinnovation.Codebreaker.Postgres

This library contains the data backend for Codebreaker for PostgreSQL using EF Core.

See https://github.com/codebreakerapp for more information on the complete solution.

See [Codebreakerlight](https://github.com/codebreakerapp/codebreakerlight) for a simple version of the Codebreaker solution with a Wiki to create your own Codebreaker service.

## Types available in this package


| Type | Description |
| --- | --- |
| `GamesPostgresContext` | This class implements `IGamesRepository` |

Configure this class to be injected for `IGamesRepository` in your DI container when Codebreaker games data should be stored in PosgreSQL.
