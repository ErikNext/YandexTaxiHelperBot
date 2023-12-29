using YandexTaxiHelperBot.App.Extensions;

namespace YandexTaxiHelperBot.Contracts;

public interface INotificationService
{
    Task SendMessage(string userId, string message);
    Task SendMessageWithButton(string userId, string message, InlineKeyboardElement element);
}