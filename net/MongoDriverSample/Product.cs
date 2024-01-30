using MongoDB.Bson;

public class Product {
    public ObjectId Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public decimal? Price { get; set; }
}
