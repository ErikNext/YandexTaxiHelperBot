using Telegram.Bot;
using YandexTaxiHelperBot.App.Models;
using YandexTaxiHelperBot.App.Services;
using YandexTaxiHelperBot.Integrations.YandexGeoApi;

namespace YandexTaxiHelperBot.App.BotCommands;

public class MainMenuCommand : CommandBase
{
    private readonly YandexGeoApi geoApi;
    
    public MainMenuCommand(SenderService sender, YandexGeoApi geoApi) : base(sender)
    {
        this.geoApi = geoApi;
    }

    public override string Title => "Меню";
    public override string Key => nameof(MainMenuCommand);
    protected override Task ExecuteInternal(ITelegramBotClient telegramBotClient, UserModel user, string? data = default)
    {
      // return geoApi.GetLocationByAddress("Маршала федоренко 8к2");
        
        user.CurrentMode = null;
        return Sender.SendAllAvailableCommands(user, "Бот отследит стоимость на поездку в YandexTaxi " +
                                                     "и уведомит вас об изменении цены выбранным способом!");
    }

    public override Task<bool> Filter(UserModel user)
    {
        return Task.FromResult(false);
    }
}