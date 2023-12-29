using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using YandexTaxiHelperBot.App.Models;
using YandexTaxiHelperBot.App.Services;

namespace YandexTaxiHelperBot.App.BotCommands;

public abstract class CommandBase
{
    public abstract string Title { get; }
    public abstract string Key { get; }
    protected SenderService Sender { get; }

    protected CommandBase(SenderService sender)
    {
        Sender = sender;
    }

    public Task Execute(ITelegramBotClient telegramBotClient, UserModel user, string? data = default)
    {
        Sender.SetTelegramBotClient(telegramBotClient);
        return ExecuteInternal(telegramBotClient, user, data);
    }

    protected abstract Task ExecuteInternal(ITelegramBotClient telegramBotClient, UserModel user, string? data = default);

    public abstract Task<bool> Filter(UserModel user);
}