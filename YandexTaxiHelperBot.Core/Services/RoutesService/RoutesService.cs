using System.Diagnostics.Contracts;
using YandexTaxiHelperBot.Repository.Routes;

namespace YandexTaxiHelperBot.Core.Services.RoutesService;

public class RoutesService
{
    private readonly IRoutesDatabase _database;

    public RoutesService(IRoutesDatabase database)
    {
        _database = database;
    }
    
    public async Task Create(RouteModel route, string userId)
    {
        var userTracking = await _database.GetUserTracking(userId);

        if (userTracking != null)
            throw new ArgumentException($"Can`t create one more tracking for user: {userId}");
        
        
        var routeDbModel = new RouteForTrackingDbModel(route.Id,
            userId,
            route.DeparturePoint,
            route.DestinationPoint,
            route.Class,
            route.LastPrice,
            route.TrackingPrice,
            route.MinimalPrice,
            route.Method,
            DateTime.Now);


        await _database.Create(routeDbModel);
    }
    
    public async Task<List<RouteModel>> GetAll()
    {
        var models = await _database.GetAll();

        return models.Select(x => Map(x)).ToList();
    }
    
    public async Task<RouteModel?> GetUserTracking(string userId)
    {
        var tracking = await _database.GetUserTracking(userId);

        if (tracking is null)
        {
            return null;
        }
        
        return Map(tracking);
    }
    
    public Task DeleteRoutesCreatedSometimeAgo(double hours)
    { 
        return _database.DeleteRoutesCreatedSometimeAgo(hours);
    }
    
    public Task Get()
    {
        return Task.CompletedTask;
    }

    private Task Update(RouteModel model)
    {
        return _database.Update(new RouteForTrackingDbModel(
            model.Id,
            model.UserId, 
            model.DeparturePoint,
            model.DestinationPoint, 
            model.Class,
            model.LastPrice,
            model.TrackingPrice, 
            model.MinimalPrice,
            model.Method,
            model.CreatedDate));
    }
    
    public Task Delete(string id)
    {
        return _database.Delete(id);
    }
    
    public Task UpdatePrice(string id, double price)
    {
        return _database.UpdatePrice(id, price);
    }
    
    private static RouteModel Map(RouteForTrackingDbModel model)
    {
        return new RouteModel(model.Id, 
            model.UserId,
            model.DeparturePoint,
            model.DestinationPoint,
            model.Class,
            model.TrackingPrice,
            model.LastPrice, 
            model.MinimalPrice,
            model.Method, 
            model.CreatedDate);
    }
}