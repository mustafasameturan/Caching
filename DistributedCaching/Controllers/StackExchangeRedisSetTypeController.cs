using DistributedCaching.Services;
using Microsoft.AspNetCore.Mvc;
using StackExchange.Redis;

namespace DistributedCaching.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class StackExchangeRedisSetTypeController : ControllerBase
{
    private readonly IDatabase db;
    private readonly string listKey = "cars";

    public StackExchangeRedisSetTypeController(RedisService redisService)
    {
        db = redisService.GetDb(2);
    }

    [HttpPost]
    public async Task<IActionResult> CacheSetData(string car)
    {
        #region Absolute Expiration Example
        
        // Key exist konrolü yapılarak expire süresinin absolute expiration mantığında çalışması sağlanır
        // if(!db.KeyExists(listKey))
        //     await db.KeyExpireAsync(listKey, DateTime.Now.AddMinutes(2));
        
        #endregion    
            
        #region Sliding Expiration Example

        // Key Exist kontrolü yapılmadan expire süresi verilir ise sliding expiration mantığında çalışır.
        await db.KeyExpireAsync(listKey, DateTime.Now.AddMinutes(5));    

        #endregion    
        
        await db.SetAddAsync(listKey, car);
        return Ok("added");
    }

    [HttpGet]
    public IActionResult GetCachedCars()
    {
        HashSet<string> carsList = new HashSet<string>();

        if (!db.KeyExists(listKey)) return NotFound("data not found!");

        db.SetMembers(listKey).ToList().ForEach(x =>
        {
            carsList.Add(x.ToString());
        });

        return Ok(carsList);
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteCar(string car)
    {
        await db.SetRemoveAsync(listKey, car);
        return Ok("deleted");
    }
}