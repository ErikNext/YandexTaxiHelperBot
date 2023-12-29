using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace YandexTaxiHelperBot.Repository.Routes;

public interface IRoutesDatabase
{
    Task Create(RouteForTrackingDbModel model);
    Task<RouteForTrackingDbModel> Get(string id);
    Task<List<RouteForTrackingDbModel>> GetAll();
    Task Update(RouteForTrackingDbModel updatedModel);
    Task Delete(string id);
    Task UpdatePrice(string id, double newPrice);
    Task DeleteRoutesCreatedSometimeAgo(double hours);
    Task<RouteForTrackingDbModel> GetUserTracking(string userId);
}

public class RoutesMongoDatabase : IRoutesDatabase
{
    private IMongoCollection<RouteForTrackingDbModel> _collection;
    
    public RoutesMongoDatabase(IOptions<RouteForTrackingDbConfig> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase(options.Value.DatabaseName);
        _collection = database.GetCollection<RouteForTrackingDbModel>(options.Value.CollectionName);
    }
    
    public Task Create(RouteForTrackingDbModel model)
    {
        return _collection.ReplaceOneAsync(x => x.UserId == model.UserId, model,
            new ReplaceOptions() { IsUpsert = true });
    }
    
    public async Task<List<RouteForTrackingDbModel>> GetAll()
    {
        var filter = new BsonDocument(); 
        return (await _collection.Find(filter).ToListAsync());
    }
    
    public async Task<RouteForTrackingDbModel> GetUserTracking(string userId)
    {
        return await (await _collection.FindAsync(x => x.UserId == userId))
            .FirstOrDefaultAsync();
    }
    
    public async Task<RouteForTrackingDbModel> Get(string id)
    {
        return await (await _collection.FindAsync(x => x.Id == id))
            .FirstOrDefaultAsync();
    }
    
    public async Task DeleteRoutesCreatedSometimeAgo(double hours)
    {
        DateTime sometimeAgo = DateTime.UtcNow.AddHours(-hours);

        var filter = Builders<RouteForTrackingDbModel>.Filter.Lt(r => r.CreatedDate,
            sometimeAgo);
        
        await _collection.DeleteManyAsync(filter);
    }
    
    public Task Update(RouteForTrackingDbModel updatedModel)
    {
        var filter = Builders<RouteForTrackingDbModel>.Filter.Eq(x => x.Id, updatedModel.Id);
        return _collection.ReplaceOneAsync(filter, updatedModel);
    }

    public Task Delete(string id)
    {
        return _collection.DeleteOneAsync(x => x.Id == id);
    }

    public async Task UpdatePrice(string id, double newPrice)
    {
        var filter = Builders<RouteForTrackingDbModel>.Filter.Eq(x => x.Id, id);
        var update = Builders<RouteForTrackingDbModel>.Update.Set(x => x.LastPrice, newPrice);

        await _collection.UpdateOneAsync(filter, update);
    }
}