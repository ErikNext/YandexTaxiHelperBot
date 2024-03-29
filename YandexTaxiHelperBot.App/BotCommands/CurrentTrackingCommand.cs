﻿using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using YandexTaxiHelperBot.App.Models;
using YandexTaxiHelperBot.App.Services;
using YandexTaxiHelperBot.Contracts;
using YandexTaxiHelperBot.Core.Services.RoutesService;

namespace YandexTaxiHelperBot.App.BotCommands;

public class CurrentTrackingCommand : CommandBase
{
    private readonly RoutesService _routesService;
    
    public CurrentTrackingCommand(SenderService sender, RoutesService routesService) : base(sender)
    {
        _routesService = routesService;
    }

    public override string Title => "Текущее отслеживание";
    public override string Key => nameof(CurrentTrackingCommand);
    protected override async Task ExecuteInternal(ITelegramBotClient telegramBotClient, 
        UserModel user, string? data = default)
    {
        var route = await _routesService.GetUserTracking(user.Id);

        if (route == null)
        {
            await Sender.SendOrEditInlineKeyboard(user, "Отслеживание истекло..",
                null,
                true, ParseMode.Markdown);
            
            return;
        }
        
        string trackingMethodStr = route.Method switch
        {
            TrackingMethod.ByLimit => $"_По лимиту:_ *{route.TrackingPrice} руб.*",
            TrackingMethod.ByPriceChange => $"_По изменению цены:_ *{route.TrackingPrice} руб.*",
            _ => ""
        };

        string message = "Текущее отслеживание:\n\n" +
                         $"Цена: *{route.LastPrice} руб.*\n" +
                         $"Минимальная цена: *{route.MinimalPrice} руб.*\n\n" +
                         $"Метод трекинга:\n{trackingMethodStr}";

        var keyboardElements = new List<InlineKeyboardElement>();
        keyboardElements.Add(new InlineKeyboardElement("Заказать", "", route.LinkForOrder));
        
        if (StorageCommands.Commands.TryGetValue(nameof(StopRouteTrackingCommand), out var command))
            keyboardElements.Add(new InlineKeyboardElement(command.Title, command.Key));
        
        await Sender.SendOrEditInlineKeyboard(user, message, keyboardElements, true, ParseMode.Markdown);
    }

    public override async Task<bool> Filter(UserModel user)
    { 
        var route = await _routesService.GetUserTracking(user.Id);

        if (route != null)
            return await Task.FromResult(true);
        
        return await Task.FromResult(false);
    }
}