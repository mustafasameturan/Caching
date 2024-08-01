using DistributedCaching.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace DistributedCaching.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class StackExchangeRedisSortedSetTypeController : ControllerBase
{
    private readonly IDatabase db;
    private readonly string listKey = "colors";

    public StackExchangeRedisSortedSetTypeController(RedisService redisService)
    {
        db = redisService.GetDb(3);
    }

    [HttpPost]
    public async Task<IActionResult> CacheSortedSetData(string color, int score)
    {
        await db.SortedSetAddAsync(listKey, color, score);
        return Ok("added");
    }

    [HttpGet]
    public IActionResult GetCachedColors()
    {
        HashSet<string> colors = new HashSet<string>();

        if (!db.KeyExists(listKey)) return NotFound("data not found!");

        db.SortedSetScan(listKey).ToList().ForEach(x =>
        {
            colors.Add(x.ToString());
        });

        return Ok(colors);
    }

    [HttpGet]
    public IActionResult GetCachedColorsWithOrder(int start = 0, int stop = -1)
    {
        HashSet<string> colors = new HashSet<string>();

        if (!db.KeyExists(listKey)) return NotFound("data not found!");

        #region SortedSetScan Example

        // db.SortedSetScan(listKey, start, stop)
        //     .ToList()
        //     .ForEach(x =>
        //         {
        //             colors.Add(x.ToString());
        //         }
        //     );

        #endregion

        #region SortedSetRangeByRank Example

        db.SortedSetRangeByRank(listKey, start, stop, order: Order.Descending)
            .ToList()
            .ForEach(x =>
                {
                    colors.Add(x.ToString());
                }
            );

        #endregion
        
        return Ok(colors);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteColor(string color)
    {
        await db.SortedSetRemoveAsync(listKey, color);
        return Ok("deleted");
    }
}