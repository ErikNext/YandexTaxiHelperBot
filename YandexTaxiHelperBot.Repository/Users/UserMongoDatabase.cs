using Microsoft.Extensions.Options;
using MongoDB.Driver;

namespace YandexTaxiHelperBot.Repository.Users;

public interface IUserDatabase
{
    Task Create(UserDbModel model);
    Task<UserDbModel> GetByTgId(long telegramId);
    Task<UserDbModel> Get(string id);
}

public class UserMongoDatabase : IUserDatabase
{
    private IMongoCollection<UserDbModel> _collection;
    
    public UserMongoDatabase(IOptions<UserDbConfig> options)
    {
        var client = new MongoClient(options.Value.ConnectionString);
        var database = client.GetDatabase(options.Value.DatabaseName);
        _collection = database.GetCollection<UserDbModel>(options.Value.CollectionName);
    }
    
    public Task Create(UserDbModel model)
    {
        return _collection.ReplaceOneAsync(x => x.TelegramId == model.TelegramId, model,
            new ReplaceOptions() { IsUpsert = true });
    }
    
    public async Task<UserDbModel> Get(string id)
    {
        return await (await _collection.FindAsync(x => x.Id == id))
            .FirstOrDefaultAsync();
    }

    public async Task<UserDbModel> GetByTgId(long telegramId)
    {
        return await (await _collection.FindAsync(x => x.TelegramId == telegramId))
            .FirstOrDefaultAsync();
    }
}