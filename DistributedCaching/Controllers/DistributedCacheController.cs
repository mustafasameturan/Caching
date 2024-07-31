using System.Text;
using InMemoryCaching.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;

namespace DistributedCaching.Controllers;

[ApiController]
[Route("api/[controller]/[action]")]
public class DistributedCacheController : ControllerBase
{
    private readonly IDistributedCache _distributedCache;

    public DistributedCacheController(IDistributedCache distributedCache)
    {
        _distributedCache = distributedCache;
    }

    [HttpPost]
    public IActionResult SetData()
    {
        DistributedCacheEntryOptions cacheEntryOptions = new();
        cacheEntryOptions.AbsoluteExpiration = DateTime.Now.AddMinutes(1);
        
        _distributedCache.SetString("series", "Dexter", cacheEntryOptions);

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> SetDataAsynchronous()
    {
        await _distributedCache.SetStringAsync("series", "Game Of Thrones");
        return Ok("added");
    }

    [HttpGet]
    public IActionResult GetData()
    {
        string? series = _distributedCache.GetString("series");

        if (series is null) return BadRequest("Data not found!");

        return Ok(series);
    }

    [HttpDelete]
    public IActionResult RemoveData()
    {
        _distributedCache.Remove("series");
        return Ok("deleted");
    }

    [HttpPost]
    public async Task<IActionResult> CacheComplexTypesWithJsonSerialize([FromBody] ComplexTypeCachingModel model)
    {
        string json = JsonConvert.SerializeObject(model);
        await _distributedCache.SetStringAsync($"complexType:{model.Id}", json);

        return Ok("added");
    }

    [HttpGet]
    public async Task<IActionResult> GetComplexTypesWithJsonDeserialize(string key)
    {
        var json = await _distributedCache.GetStringAsync(key);

        if (json is null) return BadRequest("Data not found!");

        var deserializedJson = JsonConvert.DeserializeObject<ComplexTypeCachingModel>(json);

        return Ok(deserializedJson);
    }
    
    [HttpPost]
    public async Task<IActionResult> CacheComplexTypesWithByteArr([FromBody] ComplexTypeCachingModel model)
    {
        // Byte'a çevirmeden önce json serialize yapmak gerekiyor.
        string json = JsonConvert.SerializeObject(model);
        // Sonrasında byte array değerine çeviriliyor.
        byte[] bytes = Encoding.UTF8.GetBytes(json);
        await _distributedCache.SetAsync($"complexType:{model.Id}", bytes);

        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> GetComplexTypesWithByteArr(string key)
    {
        byte[]? bytes = await _distributedCache.GetAsync(key);
        if (bytes is null) return BadRequest("Data not found!");

        string json = Encoding.UTF8.GetString(bytes);
        var deserializedJson = JsonConvert.DeserializeObject<ComplexTypeCachingModel>(json);
        return Ok(deserializedJson);
    }

    [HttpPost]
    public async Task<IActionResult> CacheImage()
    {
        string path = Path.Combine(Directory.GetCurrentDirectory(), "sample-img.png");
        byte[] imageByte = await System.IO.File.ReadAllBytesAsync(path);
        await _distributedCache.SetAsync("image", imageByte);
        return Ok("cached");
    }

    [HttpGet]
    public async Task<IActionResult> GetImage()
    {
        byte[]? imageByte = await _distributedCache.GetAsync("image");

        if (imageByte is null) return BadRequest("Data not found!");

        return File(imageByte, "image/png");
    }
}