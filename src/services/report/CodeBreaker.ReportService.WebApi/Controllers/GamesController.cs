using CodeBreaker.ReportService.Services;
using CodeBreaker.ReportService.WebApi.Mapping;
using CodeBreaker.Shared.ReportService.Api;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Results;

namespace CodeBreaker.ReportService.Controllers;

[Route("api/[controller]")]
[ApiController]
public class GamesController : ControllerBase
{
    private readonly IGameService _gameService;

    public GamesController(IGameService gameService)
    {
        _gameService = gameService;
    }

    // GET: api/<GamesController>
    [HttpGet]
    [EnableQuery]
    public IQueryable<GameDto> Get() =>
        _gameService.Games.ProjectToDto();

    // GET api/<GamesController>/f030d8d4-d9f5-4a4d-ad5f-d7b8fd2f2849
    [HttpGet("{id}")]
    [EnableQuery]
    public SingleResult<GameDto> Get(Guid id) =>
        SingleResult.Create(_gameService.Games.Where(x => x.Id == id).ProjectToDto());
}
