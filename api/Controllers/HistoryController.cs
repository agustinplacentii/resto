using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Restaurant.Api.Data;
using Restaurant.Api.Dtos;

namespace Restaurant.Api.Controllers;

[ApiController]
[Route("api/history")]
public class HistoryController(RestaurantDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<ActivityLogDto>>> GetHistory()
    {
        var logs = await db.ActivityLogs
            .OrderByDescending(log => log.CreatedAt)
            .Take(200)
            .Select(log => new ActivityLogDto(log.Id, log.Type, log.Description, log.CreatedAt))
            .ToListAsync();

        return Ok(logs);
    }
}
