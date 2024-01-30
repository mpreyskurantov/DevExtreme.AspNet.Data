using DevExtreme.AspNet.Data;

using MongoDB.Bson.Serialization.Conventions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

var collection = await App.PopulateData();
App.DoGrouping(collection);

public static class App {
    public static async Task<IMongoCollection<Product>> PopulateData() {
        ConventionPack conventionPack = new ConventionPack { new CamelCaseElementNameConvention() };
        ConventionRegistry.Register("camelCase", conventionPack, t => true);

        var connectionString = "mongodb://localhost:27017";
        var clientSettings = MongoClientSettings.FromConnectionString(connectionString);
        //clientSettings.LinqProvider = LinqProvider.V2;//Works
        clientSettings.LinqProvider = LinqProvider.V3;//Fails
        var client = new MongoClient(clientSettings);

        var database = client.GetDatabase("Products");
        //await database.DropCollectionAsync("Products");
        var collection = database.GetCollection<Product>("Products");
        /*
        var products = new List<Product> {
            new() { Name = "Product 1", Type = "Type 1", Price = (decimal)1.50 },
            new() { Name = "Product 2", Type = "Type 1", Price = (decimal)2.00 },
            new() { Name = "Product 3", Type = "Type 1", Price = (decimal)2.50 },
            new() { Name = "Product 4", Type = "Type 2", Price = (decimal)3.00 },
            new() { Name = "Product 5", Type = "Type 2", Price = (decimal)3.50 },
            new() { Name = "Product 6", Type = "Type 2", Price = (decimal)4.00 },
            new() { Name = "Product 7", Type = "Type 3", Price = (decimal)4.50 },
            new() { Name = "Product 8", Type = "Type 4", Price = (decimal)5.00 },
            new() { Name = "Product 9", Type = "Type 4", Price = (decimal)5.50 },
            new() { Name = "Product 10", Type = "Type 4", Price = (decimal)6.00 }
        };
        for (int i = 0; i < 100; i++) {
            var index = Random.Shared.Next(0, products.Count - 1);
            products[index].Id = new ObjectId();
            await collection.InsertOneAsync(products[index]);
        }
        */

        return collection;
    }

    public static void DoGrouping(IMongoCollection<Product> collection) {
        var query = collection.AsQueryable();

        var loadOptions = new DataSourceLoadOptionsBase {
            Group = new GroupingInfo[1] {
                new() { Desc = false, IsExpanded = false, Selector = "Type" }
            }
        };

        var loadResult = DataSourceLoader.Load(query, loadOptions);
        foreach(var group in loadResult.data) {
            var item = group as DevExtreme.AspNet.Data.ResponseModel.Group;
            Console.WriteLine(item?.key);
        }
    }
}

