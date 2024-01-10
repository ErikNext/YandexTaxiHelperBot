using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace YandexTaxiHelperBot.Repository.Address;

public interface IAddressesDatabase
{
    Task Create(AddressDbModel model);
    Task<List<AddressDbModel>> GetUserAddresses(string userId);
    Task Delete(string userId, string addressId);
}

public class AddressesMongoDatabase : IAddressesDatabase
{
    private IMongoCollection<AddressDbModel> _collection;
    
    public AddressesMongoDatabase(IOptions<AddressDbConfig> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase(options.Value.DatabaseName);
        _collection = database.GetCollection<AddressDbModel>(options.Value.CollectionName);
    }
    
    public Task Create(AddressDbModel model)
    {
        return _collection.ReplaceOneAsync(x => x.Id == model.Id, model,
            new ReplaceOptions() { IsUpsert = true });
    }
    
    public Task<List<AddressDbModel>> GetUserAddresses(string userId)
    {
        return _collection.Find(x => x.UserId == userId).ToListAsync();
    }
    
    public async Task Delete(string userId, string addressId)
    {
        await _collection.DeleteOneAsync(x => x.UserId == userId && x.Id == addressId);
    }
}