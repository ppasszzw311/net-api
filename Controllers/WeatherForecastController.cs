using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NET_API.Data;
using NET_API.Models;

namespace NET_API.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public WeatherForecastController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: api/WeatherForecast
    [HttpGet]
    public async Task<ActionResult<IEnumerable<WeatherForecast>>> GetWeatherForecasts()
    {
        return await _context.WeatherForecasts.ToListAsync();
    }

    // GET: api/WeatherForecast/5
    [HttpGet("{id}")]
    public async Task<ActionResult<WeatherForecast>> GetWeatherForecast(int id)
    {
        var weatherForecast = await _context.WeatherForecasts.FindAsync(id);

        if (weatherForecast == null)
        {
            return NotFound();
        }

        return weatherForecast;
    }

    // POST: api/WeatherForecast
    [HttpPost]
    public async Task<ActionResult<WeatherForecast>> PostWeatherForecast(WeatherForecast weatherForecast)
    {
        _context.WeatherForecasts.Add(weatherForecast);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetWeatherForecast), new { id = weatherForecast.Id }, weatherForecast);
    }

    // PUT: api/WeatherForecast/5
    [HttpPut("{id}")]
    public async Task<IActionResult> PutWeatherForecast(int id, WeatherForecast weatherForecast)
    {
        if (id != weatherForecast.Id)
        {
            return BadRequest();
        }

        _context.Entry(weatherForecast).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!WeatherForecastExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    // DELETE: api/WeatherForecast/5
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteWeatherForecast(int id)
    {
        var weatherForecast = await _context.WeatherForecasts.FindAsync(id);
        if (weatherForecast == null)
        {
            return NotFound();
        }

        _context.WeatherForecasts.Remove(weatherForecast);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool WeatherForecastExists(int id)
    {
        return _context.WeatherForecasts.Any(e => e.Id == id);
    }
}