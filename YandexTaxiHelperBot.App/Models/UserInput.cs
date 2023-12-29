using Location = YandexTaxiHelperBot.Contracts.Location;

namespace YandexTaxiHelperBot.App.Models;

public class UserInput
{
    public string? Raw { get; set; } = string.Empty;
    public Location? Location { get; set; }
}