namespace InMemoryCaching.Models;

public record ComplexTypeCachingModel
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public int Value { get; set; }
}