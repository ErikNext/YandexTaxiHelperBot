using YandexTaxiHelperBot.Contracts;

namespace YandexTaxiHelperBot.Core.Services.RoutesService;

public class RouteModel
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public Location DeparturePoint { get; set; }
    public Location DestinationPoint { get; set; }
    public TaxiClass Class { get; set; }
    public double TrackingPrice { get; set; }
    public double LastPrice { get; set; }
    public double MinimalPrice { get; set; }
    public TrackingMethod Method { get; set; }
    public DateTime CreatedDate { get; set; }

    public RouteModel(string id)
    {
        Id = id;
    }

    public RouteModel(
        string id, 
        string userId,
        Location departurePoint,
        Location destinationPoint,
        TaxiClass @class, 
        double trackingPrice, 
        double lastPrice, 
        double minimalPrice,
        TrackingMethod method,
        DateTime createdDate)
    {
        Id = id;
        UserId = userId;
        DeparturePoint = departurePoint;
        DestinationPoint = destinationPoint;
        Class = @class;
        TrackingPrice = trackingPrice;
        MinimalPrice = minimalPrice;
        LastPrice = lastPrice;
        Method = method;
        CreatedDate = createdDate;
    }
}