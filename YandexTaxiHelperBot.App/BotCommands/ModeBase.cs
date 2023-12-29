using System.Threading.Tasks;
using Telegram.Bot;
using YandexTaxiHelperBot.App.Models;
using YandexTaxiHelperBot.App.Services;

namespace YandexTaxiHelperBot.App.BotCommands;

public abstract class ModeBase
{
    protected readonly SenderService SenderService;
    protected readonly ITelegramBotClient TgBotClient;
    
    public ModeBase(SenderService sender, ITelegramBotClient tgBotClient)
    {
        SenderService = sender;
        TgBotClient = tgBotClient;
        SenderService.SetTelegramBotClient(tgBotClient);
    }
    public abstract Task Execute(UserModel user);
}