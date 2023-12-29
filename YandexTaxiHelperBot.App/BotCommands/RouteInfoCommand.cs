using NUlid;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using YandexTaxiHelperBot.App.Extensions;
using YandexTaxiHelperBot.App.Models;
using YandexTaxiHelperBot.App.Services;
using YandexTaxiHelperBot.Contracts;
using YandexTaxiHelperBot.Core.Services.RoutesService;
using YandexTaxiHelperBot.Integrations.YandexGoApi;

namespace YandexTaxiHelperBot.App.BotCommands;

public class RouteInfoCommand : CommandBase
{
    private readonly YandexGoApi _yandexGoApi;
    private readonly RoutesService _routesService;
    
    public RouteInfoCommand(SenderService sender, YandexGoApi yandexGoApi, RoutesService routesService) : base(sender)
    {
        _yandexGoApi = yandexGoApi;
        _routesService = routesService;
    }

    public override string Title => "Отслеживать маршрут";
    public override string Key => nameof(RouteInfoCommand);
    protected override Task ExecuteInternal(ITelegramBotClient telegramBotClient, UserModel user, string? data = default)
    {
        user.CurrentMode = new RouteInfoMode(Sender, telegramBotClient, _yandexGoApi, _routesService);
        return user.CurrentMode.Execute(user);
    }

    public override Task<bool> Filter(UserModel user)
    {
        return Task.FromResult(true);
    }
}

public class RouteInfoMode : ModeBase
{
    private TrackRouteStep _step = TrackRouteStep.Initial;
    private RouteModel _route { get; set; }

    private readonly YandexGoApi _yandexGoApi;
    private readonly RoutesService _routesService;
    
    public override Task Execute(UserModel user)
    {
        var task = _step switch
        {
            TrackRouteStep.Initial => Initial(user),
            TrackRouteStep.SetDeparturePoint => SetDeparturePoint(user),
            TrackRouteStep.SetDestinationPoint => SetDestinationPoint(user),
            TrackRouteStep.SetTaxiClass => SetTaxiClass(user),
            TrackRouteStep.SetTrackingMethod => SetTrackingMethod(user),
            TrackRouteStep.SetPrice => SetPrice(user),
            _ => throw new ArgumentException($"Invalid step value '{_step}' in {nameof(RouteInfoMode)}")
        };

        return task;
    }
    
    private async Task Initial(UserModel user)
    {
        var userTracking = await _routesService.GetUserTracking(user.Id);
        
        if (userTracking is not null && user.Input.Raw != "startNew")
        {
            var keyboard = new List<InlineKeyboardElement>()
            {
                new ("Начать новое", "startNew"),
            };
            
            await SenderService.SendOrEditInlineKeyboard(user, 
                "У вас уже есть отслежвание", 
                keyboard, true);
            return;
        }

        if (user.Input.Raw == "startNew" && userTracking is not null)
        {
            await _routesService.Delete(userTracking.Id);
        }

        _route = new RouteModel(Ulid.NewUlid().ToString());
        
        await SenderService.SendOrEditInlineKeyboard(user, 
            "*Отправьте пункт отправления*\n" +
            "_скрепка \u27a1\ufe0f локация_", 
            null, true, ParseMode.Markdown);

        _step++;
    }
    
    private async Task SetDeparturePoint(UserModel user)
    {
        if (user.Input.Location == null) 
        {
            await SenderService.SendOrEditInlineKeyboard(user, 
                $"Некорректная локация, попробуйте еще раз", 
                null, true);
            return;
        }
        
        _route.DeparturePoint = user.Input.Location;

        await SenderService.RemoveMessage(user, user.LastSendMessage.MessageId);
        
        await SenderService.SendOrEditInlineKeyboard(user, 
            "*Отлично! Отправьте пункт назначения*\n" +
            "_скрепка \u27a1\ufe0f локация_", 
            null, true, ParseMode.Markdown);
        
        _step++;
    }
    
    private async Task SetDestinationPoint(UserModel user)
    {
        if (user.Input.Location == null)
        {
            await SenderService.SendOrEditInlineKeyboard(user, 
                $"Некорректная локация, попробуйте еще раз", 
                null, true);
            return;
        }
        
        _route.DestinationPoint = user.Input.Location;

        var taxiClassesElements = new List<InlineKeyboardElement>()
        {
            new ("Эконом", TaxiClass.econom.ToString()),
            new ("Бизнес", TaxiClass.business.ToString()),
        };
        
        await SenderService.RemoveMessage(user, user.LastSendMessage.MessageId);
        await SenderService.SendOrEditInlineKeyboard(user,
            $"Локации заданы! Выберите *класс такси:*",
            taxiClassesElements, true, ParseMode.Markdown);

        _step++;
    }

    private async Task SetTaxiClass(UserModel user)
    {
        if (!Enum.TryParse(user.Input.Raw, out TaxiClass taxiClass))
        { 
            await SenderService.SendOrEditInlineKeyboard(user,
                $"Не удалось определить класс такси",
                null, true);
            return;
        }
        
        _route.Class = taxiClass;
        
        var routeInfo = await _yandexGoApi.GetRouteInfo(
            (_route.DeparturePoint.Longitude, _route.DeparturePoint.Latitude), 
            (_route.DestinationPoint.Longitude, _route.DestinationPoint.Latitude), 
            _route.Class);

        if (routeInfo == null)
        {
            await SenderService.SendOrEditInlineKeyboard(user,
                $"Не удалось получить информацию по вашим данным!",
                null, true);

            user.CurrentMode = null;
            return;
        }

        _route.LastPrice = routeInfo.Price;
        _route.MinimalPrice = routeInfo.MinPrice;
        
        var trackingMethodsElements = new List<InlineKeyboardElement>()
        {
            new ("По изменению цены", TrackingMethod.ByPriceChange.ToString()),
            new ("По лимиту", TrackingMethod.ByLimit.ToString()),
        };

        await SenderService.SendOrEditInlineKeyboard(user, 
            $"Цена в данную минуту: *{_route.LastPrice} руб.*\n" +
            $"Минимально возможная цена: *{_route.MinimalPrice} руб.*" +
            $"\n\nВыберите метод отслеживания",
            trackingMethodsElements, true, ParseMode.Markdown);

        _step++;
    }
    
    private async Task SetTrackingMethod(UserModel user)
    {
        if (!Enum.TryParse(user.Input.Raw, out TrackingMethod method))
        { 
            await SenderService.SendOrEditInlineKeyboard(user,
                $"Не удалось задать метод отслеживания",
                null, true);
            return;
        }

        _route.Method = method;
        
        await SenderService.SendOrEditInlineKeyboard(user,
            $"Введите цену числом. _Например: 50_",
            null, true, ParseMode.Markdown);
        
        _step++;
    }
    
    private async Task SetPrice(UserModel user)
    {
        await SenderService.RemoveMessage(user, user.LastSendMessage.MessageId);
        
        if (!double.TryParse(user.Input.Raw, out var trackingPrice))
        {
            await SenderService.SendOrEditInlineKeyboard(user,
                $"Не удалось считать цену",
                null, true);
            return;
        }

        _route.TrackingPrice = trackingPrice;

        string message = string.Empty;
        
        if (_route.Method == TrackingMethod.ByLimit)
            message = $"Вы будете уведомлены, когда цена опустится до {_route.TrackingPrice} руб.";
        else if (_route.Method == TrackingMethod.ByPriceChange)
            message = $"Вы будете уведомлены, при изменении цены в {_route.TrackingPrice} руб.";

        await _routesService.Create(_route, user.Id);
        
        await SenderService.SendOrEditInlineKeyboard(user,
            message,
            null, true);
    }
    
    public RouteInfoMode(SenderService sender, ITelegramBotClient tgBotClient, YandexGoApi yandexGoApi, 
        RoutesService routesService) 
        : base(sender, tgBotClient)
    {
        _yandexGoApi = yandexGoApi;
        _routesService = routesService;
    }
}

enum TrackRouteStep : Byte
{
    Unknown,
    Initial,
    SetDeparturePoint,
    SetDestinationPoint,
    SetTaxiClass,
    SetTrackingMethod,
    SetPrice
}