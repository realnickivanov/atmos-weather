using Atmos.Application.Weather.GetWeather;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Atmos.Api.Controllers;

[ApiController]
[Route("api/weather")]
public sealed class WeatherController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<WeatherResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status502BadGateway)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<WeatherResponse>> Get(CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetWeatherQuery(), cancellationToken);
        return Ok(response);
    }
}
