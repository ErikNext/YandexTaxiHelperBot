using System.Text.Json.Serialization;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Converters;
using YandexTaxiHelperBot.Contracts;

namespace YandexTaxiHelperBot.Repository.Routes;

public class RouteForTrackingDbModel
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public Location DeparturePoint { get; set; }
    public Location DestinationPoint { get; set; }
    [JsonConverter(typeof(StringEnumConverter))]
    [BsonRepresentation(BsonType.String)]
    public TaxiClass Class { get; set; }
    public double LastPrice { get; set; }
    public double MinimalPrice { get; set; }
    public double TrackingPrice { get; set; }
    [JsonConverter(typeof(StringEnumConverter))] 
    [BsonRepresentation(BsonType.String)]
    public TrackingMethod Method { get; set; }
    public DateTime CreatedDate { get; set; }
    

    public RouteForTrackingDbModel(string id,
        string userId,
        Location departurePoint, 
        Location destinationPoint, 
        TaxiClass taxiClass, 
        double lastPrice, 
        double trackingPrice, 
        double minimalPrice,
        TrackingMethod method,
        DateTime createdDate)
    {
        Id = id;
        UserId = userId;
        DeparturePoint = departurePoint;
        DestinationPoint = destinationPoint;
        Class = taxiClass;
        LastPrice = lastPrice;
        TrackingPrice = trackingPrice;
        MinimalPrice = minimalPrice;
        Method = method;
        CreatedDate = createdDate;
    }
}
