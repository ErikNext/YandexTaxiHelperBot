namespace YandexTaxiHelperBot.Contracts;

public class Location
{
    public string? Title { get; set; } = null!;
    public double Latitude { get; set; }
    public double Longitude { get; set; }

    public Location(double latitude, double longitude)
    {
        Latitude = latitude;
        Longitude = longitude;
    }
    
    public bool Equals(Location other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;

        return Latitude.Equals(other.Latitude) && Longitude.Equals(other.Longitude);
    }
    
    public override bool Equals(object obj)
    {
        return Equals(obj as Location);
    }
}