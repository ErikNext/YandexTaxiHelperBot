namespace YandexTaxiHelperBot.Repository.Users;

public class UserDbModel
{
    public string Id { get; set; }
    public long TelegramId { get; set; }
    public string Username { get; set; }
    public DateTime CreatedDate { get; set; }

    public UserDbModel(
        string id, 
        long telegramId,
        string username,
        DateTime createdDate)
    {
        Id = id;
        TelegramId = telegramId;
        Username = username;
        CreatedDate = createdDate;
    }
}
