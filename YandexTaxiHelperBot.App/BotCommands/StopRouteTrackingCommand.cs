using Telegram.Bot;
using YandexTaxiHelperBot.App.Models;
using YandexTaxiHelperBot.App.Services;
using YandexTaxiHelperBot.Core.Services.RoutesService;

namespace YandexTaxiHelperBot.App.BotCommands;

public class StopRouteTrackingCommand : CommandBase
{
    private readonly RoutesService _routesService;
    
    public StopRouteTrackingCommand(SenderService sender, RoutesService routesService) : base(sender)
    {
        _routesService = routesService;
    }

    public override string Title => "Остановить";
    public override string Key => nameof(StopRouteTrackingCommand);
    protected override async Task ExecuteInternal(ITelegramBotClient telegramBotClient, UserModel user, string? data = default)
    {
        var userTracking = await _routesService.GetUserTracking(user.Id);

        if (userTracking == null)
        {
            await Sender.SendOrEditInlineKeyboard(user, 
                "Отслеживание устарело..", 
                null, true);
            return;
        } 
        
        await _routesService.Delete(userTracking.Id);
        
        await Sender.SendOrEditInlineKeyboard(user, 
            "Отслеживание было остановлено..", 
            null, true);
    }

    public override Task<bool> Filter(UserModel user)
    {
        return Task.FromResult(false);
    }
}