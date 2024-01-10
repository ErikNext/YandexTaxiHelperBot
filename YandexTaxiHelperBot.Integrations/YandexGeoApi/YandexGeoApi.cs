using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using YandexTaxiHelperBot.Contracts;

namespace YandexTaxiHelperBot.Integrations.YandexGeoApi;

public class YandexGeoApi
{
    private static HttpClient _client = new();
    private readonly string _baseUrl = "https://geocode-maps.yandex.ru/1.x/";
    private readonly string _apiKey;
    private readonly ILogger<YandexGeoApi> _logger;

    public YandexGeoApi(ILogger<YandexGeoApi> logger, IOptions<YandexGeoApiConfiguration> configuration)
    {
        _apiKey = configuration.Value.ApiToken;
        _logger = logger;
    }

    public async Task<string?> GetAddressByLocation(Location location)
    {
        string fullUrl = _baseUrl + "?" +
                         $"geocode={location.Longitude}, {location.Latitude}&" +
                         $"results=1&" +
                         $"format=json&" +
                         $"apikey={_apiKey}";
        
        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(content);
                    
                    string name = jsonObject["response"]["GeoObjectCollection"]
                        ["featureMember"][0]["GeoObject"]["name"]
                        .ToString().Replace("улица", "").Trim();
                    
                    return name;
                }
                else
                {
                    _logger.LogInformation($"Exception: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception: {ex.Message}");
            }
        }

        return null;
    }
    
    public async Task<Location?> GetLocationByAddress(string request)
    {
        string fullUrl = _baseUrl + "?" +
                         $"geocode=москва {request}&" +
                         $"results=1&" +
                         $"format=json&" +
                         $"apikey={_apiKey}";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(content);
                    JObject optionsArray = (JObject)jsonObject["Point"];
                    
                    string coordinates = jsonObject["response"]["GeoObjectCollection"]["featureMember"][0]["GeoObject"]["Point"]["pos"].ToString();

                    string[] coordinateValues = coordinates.Split(' ');
        
                    double longitude = double.Parse(coordinateValues[0], CultureInfo.InvariantCulture);
                    double latitude = double.Parse(coordinateValues[1], CultureInfo.InvariantCulture);

                    return new Location(latitude, longitude);
                }
                else
                {
                    _logger.LogInformation($"Exception: {response.StatusCode}");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception: {ex.Message}");
            }
        }
        return null;
    }
}