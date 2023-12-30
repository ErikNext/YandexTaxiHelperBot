namespace YandexTaxiHelperBot.Contracts;

public interface INotificationService
{
    Task SendMessage(string userId, string message);

    Task SendMessageWithButtons(
        string userId,
        string message, 
        List<InlineKeyboardElement> elements,
        bool removeLastUserMessage = false);
}