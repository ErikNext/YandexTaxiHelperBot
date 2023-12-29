using System.Threading.Tasks;
using Telegram.Bot;
using YandexTaxiHelperBot.App.Models;
using YandexTaxiHelperBot.App.Services;

namespace YandexTaxiHelperBot.App.BotCommands;

public class MainMenuCommand : CommandBase
{
    public MainMenuCommand(SenderService sender) : base(sender)
    {
    }

    public override string Title => "Меню";
    public override string Key => nameof(MainMenuCommand);
    protected override Task ExecuteInternal(ITelegramBotClient telegramBotClient, UserModel user, string? data = default)
    {
        user.CurrentMode = null;
        return Sender.SendAllAvailableCommands(user, "Все команды: ");
    }

    public override Task<bool> Filter(UserModel user)
    {
        return Task.FromResult(false);
    }
}