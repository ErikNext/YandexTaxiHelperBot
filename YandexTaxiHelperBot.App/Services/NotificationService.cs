using YandexTaxiHelperBot.Contracts;

namespace YandexTaxiHelperBot.App.Services;

public class NotificationService : INotificationService
{
    private readonly SenderService _sender;
    private readonly UsersService _usersService;

    public NotificationService(SenderService sender, UsersService usersService)
    {
        _sender = sender;
        _usersService = usersService;
    }

    public async Task SendMessage(string userId, string message)
    {
        var user = await _usersService.GetOrCreate(userId);

        await _sender.SendMessage(user.TelegramId, message);
    }
    
    public async Task SendMessageWithButtons(string userId, string message, List<InlineKeyboardElement> elements)
    {
        var user = await _usersService.GetOrCreate(userId);

        await _sender.SendInlineKeyboard(user, message, true, elements);
    }
}