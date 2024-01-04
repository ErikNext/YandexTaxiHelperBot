namespace YandexTaxiHelperBot.Repository
{
    public class UserDbConfig
    {
        public static readonly string Section = "UserDb";
        public string ConnectionString { get; set; } = null!;
        public string CollectionName { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
    }
    
    public class RouteForTrackingDbConfig
    {
        public static readonly string Section = "RouteDb";
        public string ConnectionString { get; set; } = null!;
        public string CollectionName { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
    }
    
    public class AddressDbConfig
    {
        public static readonly string Section = "AddressDb";
        public string ConnectionString { get; set; } = null!;
        public string CollectionName { get; set; } = null!;
        public string DatabaseName { get; set; } = null!;
    }
}