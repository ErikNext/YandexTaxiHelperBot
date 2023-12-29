using System.Globalization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using YandexTaxiHelperBot.Contracts;

namespace YandexTaxiHelperBot.Integrations.YandexGoApi;

public class YandexGoApi
{
    private static HttpClient _client = new();
    private readonly string _baseUrl = "https://taxi-routeinfo.taxi.yandex.net/taxi_info";
    private readonly string _apiKey;
    private readonly string _clid;
    private readonly ILogger<YandexGoApi> _logger;

    public YandexGoApi(ILogger<YandexGoApi> logger, IOptions<YandexGoApiConfiguration> configuration)
    {
        _apiKey = configuration.Value.ApiToken;
        _clid = configuration.Value.Clid;
        _logger = logger;
    }

    public async Task<RouteInfo?> GetRouteInfo(
        (double longitude, double latitude) departurePoint,
        (double longitude, double latitude) destinationPoint,
        TaxiClass taxiClass)
    {
        RouteInfo? routeInfo = null;

        string fullUrl = _baseUrl + "?" +
                         $"rll={departurePoint.longitude.ToStringWithPoint()}," +
                         $"{departurePoint.latitude.ToStringWithPoint()}~" +
                         $"{destinationPoint.longitude.ToStringWithPoint()}," +
                         $"{destinationPoint.latitude.ToStringWithPoint()}" +
                         $"&clid={_clid}" +
                         $"&apikey={_apiKey}" +
                         $"&class={taxiClass.ToString()}";

        using (HttpClient client = new HttpClient())
        {
            try
            {
                HttpResponseMessage response = await client.GetAsync(fullUrl);

                if (response.IsSuccessStatusCode)
                {
                    string content = await response.Content.ReadAsStringAsync();
                    JObject jsonObject = JObject.Parse(content);
                    JArray optionsArray = (JArray)jsonObject["options"];
                    JObject firstOption = (JObject)optionsArray.First;



                    double price = (double)firstOption["price"];
                    double minPrice = (double)firstOption["min_price"];
                    double waitingTime = (double)firstOption["waiting_time"];
                    string className = (string)firstOption["class_name"];
                    string classText = (string)firstOption["class_text"];
                    string classLevel = (string)firstOption["class_level"];
                    string priceText = (string)firstOption["price_text"];

                    routeInfo = new RouteInfo(price, minPrice, waitingTime, className, classText, classLevel,
                        priceText);
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

        return routeInfo;
    }
}

public static class Extensions
{
    public static string ToStringWithPoint(this double number)
    {
        return number.ToString("G", CultureInfo.InvariantCulture);
    }
}

public class RouteInfo
{
    public double Price { get; set; }
    public double MinPrice { get; set; }
    public double WaitingTime { get; set; }
    public string ClassName { get; set; }
    public string ClassText { get; set; }
    public string ClassLevel { get; set; }
    public string PriceText { get; set; }

    
    public RouteInfo(double price, double minPrice, double waitingTime, string className, string classText, 
        string classLevel, string priceText)
    {
        Price = price;
        MinPrice = minPrice;
        WaitingTime = waitingTime;
        ClassName = className;
        ClassText = classText;
        ClassLevel = classLevel;
        PriceText = priceText;
    }
}