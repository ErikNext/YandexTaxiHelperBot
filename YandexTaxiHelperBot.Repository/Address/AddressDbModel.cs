using YandexTaxiHelperBot.Contracts;

namespace YandexTaxiHelperBot.Repository.Address;

public class AddressDbModel
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string Title { get; set; }
    public Location DeparturePoint { get; set; }
    public Location DestinationPoint { get; set; }

    public AddressDbModel(string id, string userId, string title, Location departurePoint, Location destinationPoint)
    {
        Id = id;
        UserId = userId;
        Title = title;
        DeparturePoint = departurePoint;
        DestinationPoint = destinationPoint;
    }
}