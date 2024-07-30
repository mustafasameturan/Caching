using InMemoryCaching.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace InMemoryCaching.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class InMemoryCacheController : ControllerBase
{
    private readonly IMemoryCache _memoryCache;

    public InMemoryCacheController(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    [HttpPost]
    public IActionResult Cache()
    {
        _memoryCache.Set<string>("date", DateTime.Now.ToString());
        return Ok();
    }

    [HttpGet]
    public IActionResult ReadCache()
    {
        var date = _memoryCache.Get<string>("date");
        var callbackData = _memoryCache.TryGetValue("callback", out string? callback);
        
        var responseData = new
        {
            DateData = date,
            CallbackData = callback
        };
        
        return Ok(responseData);
    }

    [HttpPost]
    public IActionResult CacheIfValueDoesNotExists()
    {
        // İlk yol
        if(string.IsNullOrEmpty(_memoryCache.Get<string>("date")))
            _memoryCache.Set<string>("date", DateTime.Now.ToString());
        
        // İkinci yol
        // Out kullanımı olduğu için eğer value dolu ise dateCache değişkeni kodun geri kalanında kullanılabilir.
        if(!_memoryCache.TryGetValue("date", out string dateCache))
            _memoryCache.Set<string>("date", DateTime.Now.ToString());
        
        // Üçünü yol
        //"date" key'ine sahip value'yu almaya çalışır. Eğer yok ise yeni atar.
        _memoryCache.GetOrCreate<string>("date", entry =>
        {
            return DateTime.Now.ToString();
        });

        return Ok();
    }

    [HttpDelete]
    public IActionResult ClearCacheByKey(string key)
    {
        if (string.IsNullOrEmpty(_memoryCache.Get<string>("date")))
            return NotFound($"{key} not found!");
        
        _memoryCache.Remove(key);
        return Ok();
    }

    [HttpPost]
    public IActionResult CacheDataWithAbsoluteExpiration()
    {
        MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();

        options.AbsoluteExpiration = DateTime.Now.AddSeconds(10);
        // Bir veri silindiği zaman neden silindiği yakalanıyor.
        options.RegisterPostEvictionCallback((key, value, reason, state) =>
        {
            _memoryCache.Set("callback", $"{key} -> {value} => sebep: {reason}");
        });

        _memoryCache.Set<string>("date", DateTime.Now.ToString(), options);

        return Ok();
    }
    
    [HttpPost]
    public IActionResult CacheDataWithSlidingExpiration()
    {
        MemoryCacheEntryOptions options = new MemoryCacheEntryOptions();

        options.AbsoluteExpiration = DateTime.Now.AddMinutes(1);
        options.SlidingExpiration = TimeSpan.FromSeconds(10);
        
        // Öncelik
        options.Priority = CacheItemPriority.High;
        // CacheItemPriority.High -> Memory dolar ise bu veriyi en son sil.
        // CacheItemPriority.Normal -> High ile Low arasında bir değerdir. Önce low değerler sonra normal değerler silinir.
        // CacheItemPriority.Low -> Memory dolar ise bu veri silinebilir.
        // CacheItemPriority.NeverRemove -> Memory dolsa bile bu veriyi silme.
        
        // Bir veri silindiği zaman neden silindiği yakalanıyor.
        options.RegisterPostEvictionCallback((key, value, reason, state) =>
        {
            _memoryCache.Set("callback", $"{key} -> {value} => sebep: {reason}");
        });
        
        _memoryCache.Set<string>("date", DateTime.Now.ToString(), options);

        return Ok();
    }

    [HttpPost]
    public IActionResult CacheComplexType([FromBody] ComplexTypeCachingModel complexTypeCachingModel)
    {
        _memoryCache.Set<ComplexTypeCachingModel>("model:1", complexTypeCachingModel);
        return Ok();
    }

    [HttpGet]
    public IActionResult ReadCacheComplexType()
    {
        var cacheDataResponse = _memoryCache.TryGetValue("model:1", out ComplexTypeCachingModel? cacheData);

        if (!cacheDataResponse) return BadRequest("Data not found!");

        return Ok(cacheData);
    }
}