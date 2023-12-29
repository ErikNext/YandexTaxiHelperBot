using System.Collections.Concurrent;
using NUlid;
using YandexTaxiHelperBot.App.Models;
using YandexTaxiHelperBot.Core.Services.RoutesService;
using YandexTaxiHelperBot.Repository.Users;

namespace YandexTaxiHelperBot.App.Services;

public class UsersService
{
    private readonly IUserDatabase _userDatabase;
    private static ConcurrentDictionary<long, UserModel> _users { get; } = new();
    private readonly RoutesService _routesService;

    public UsersService(IUserDatabase userDatabase, RoutesService routesService)
    {
        _userDatabase = userDatabase;
        _routesService = routesService;
    }
    
    private async Task<UserModel> Create(long tgUserId, string username)
    {
        var userDbModel = await _userDatabase.GetByTgId(tgUserId);

        if (userDbModel == null)
        {
            userDbModel = new UserDbModel(
                Ulid.NewUlid().ToString(),
                tgUserId,
                username, 
                DateTime.Now);

            await _userDatabase.Create(userDbModel);
        }

        var user = new UserModel(userDbModel.Id,
            userDbModel.TelegramId, 
            username);
        
        _users.TryAdd(userDbModel.TelegramId, user);

        return user;
    }

    public async Task<UserModel> GetOrCreate(long userId, string username)
    {
        _users.TryGetValue(userId, out var user);

        if (user == null)
            return await Create(userId, username);
        
        var userDbModel = await _userDatabase.GetByTgId(user.TelegramId);  
        
        if (userDbModel == null)
            return await Create(userId, username);

        return user;
    }
    
    public async Task<UserModel> GetOrCreate(string id)
    {
        var userModelDb = await _userDatabase.Get(id);

        return (await GetOrCreate(userModelDb.TelegramId, userModelDb.Username));
    }
}