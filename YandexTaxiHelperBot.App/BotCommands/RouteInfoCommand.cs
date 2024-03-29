﻿using NUlid;
using Telegram.Bot;
using Telegram.Bot.Types.Enums;
using YandexTaxiHelperBot.App.Models;
using YandexTaxiHelperBot.App.Services;
using YandexTaxiHelperBot.Contracts;
using YandexTaxiHelperBot.Core.Services.AddressesService;
using YandexTaxiHelperBot.Core.Services.RoutesService;
using YandexTaxiHelperBot.Integrations.YandexGeoApi;
using YandexTaxiHelperBot.Integrations.YandexGoApi;
using YandexTaxiHelperBot.Repository.Address;

namespace YandexTaxiHelperBot.App.BotCommands;

public class RouteInfoCommand : CommandBase
{
    private readonly YandexGoApi _yandexGoApi;
    private readonly RoutesService _routesService;
    private readonly AddressesService _addressesService;
    private readonly YandexGeoApi _yandexGeoApi;
    
    public RouteInfoCommand(SenderService sender, YandexGoApi yandexGoApi, 
        RoutesService routesService, 
        AddressesService addressesService, YandexGeoApi yandexGeoApi) : base(sender)
    {
        _yandexGoApi = yandexGoApi;
        _routesService = routesService;
        _addressesService = addressesService;
        _yandexGeoApi = yandexGeoApi;
    }

    public override string Title => "Отслеживать маршрут";
    public override string Key => nameof(RouteInfoCommand);
    protected override Task ExecuteInternal(ITelegramBotClient telegramBotClient, UserModel user, string? data = default)
    {
        user.CurrentMode = new RouteInfoMode(Sender, telegramBotClient, _yandexGoApi, _routesService, 
            _addressesService, _yandexGeoApi);
        
        return user.CurrentMode.Execute(user);
    }

    public override Task<bool> Filter(UserModel user)
    {
        return Task.FromResult(true);
    }
}

public class RouteInfoMode : ModeBase
{
    private readonly YandexGoApi _yandexGoApi;
    private readonly YandexGeoApi _yandexGeoApi;
    private readonly RoutesService _routesService;
    private readonly AddressesService _addressesService;
    
    private List<AddressDbModel>? _addresses = null!;
    private TrackRouteStep _step = TrackRouteStep.Initial;
    private RouteModel _route { get; set; }

    private List<InlineKeyboardElement> _taxiClassesElements = new List<InlineKeyboardElement>()
    {
        new ("Эконом", TaxiClass.econom.ToString()),
        new ("Комфорт", TaxiClass.business.ToString()),
        new ("Комфорт+", TaxiClass.comfortplus.ToString()),
        new ("Бизнес", TaxiClass.vip.ToString()),
    };
    
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
            TrackRouteStep.SetAddressFromHistory => SetAddressFromSaved(user),
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
            await _routesService.Delete(userTracking.Id);

        _route = new RouteModel(Ulid.NewUlid().ToString());
        _addresses = await _addressesService.GetUserAddresses(user.Id);

        var elements = new List<InlineKeyboardElement>();

        if (_addresses.Count != 0)
            elements.Add(new("История", "HistoryAddresses"));
        
        await SenderService.SendOrEditInlineKeyboard(user, 
            "*Пожалуйста, укажите начальную точку маршрута*\n" +
            "_скрепка \u27a1\ufe0f локация_ или _введите адрес вручную_",
            elements, true, ParseMode.Markdown);

        _step++;
    }
    
    private async Task SetDeparturePoint(UserModel user)
    {
        if (user.Input.Raw == "HistoryAddresses")
        {
            _step = TrackRouteStep.SetAddressFromHistory;
            await PrintAddressesFromSaved(user);
            return;
        }
        
        if (user.Input.Location == null && String.IsNullOrEmpty(user.Input.Raw))
        {
            await SenderService.SendOrEditInlineKeyboard(user, 
                $"Некорректная локация, попробуйте еще раз", 
                null, true);
            return;
        }
        
        if (user.Input.Location == null && user.Input.Raw != null)
        {
            var location = await _yandexGeoApi.GetLocationByAddress(user.Input.Raw);

            if (location == null)
            {
                await SenderService.SendOrEditInlineKeyboard(user, 
                    $"Не удалось найти локацию по запросу: {user.Input.Raw}", 
                    null, true);
                return;
            }
            _route.DeparturePoint = location;
        }
        
        else if (user.Input.Location != null)
            _route.DeparturePoint = user.Input.Location;
        
        _route.DeparturePoint.Title = await _yandexGeoApi.GetAddressByLocation(_route.DeparturePoint);
        
        await SenderService.RemoveMessage(user, user.LastSendMessage.MessageId);
        
        await SenderService.SendOrEditInlineKeyboard(user, 
            "*Отлично! Теперь укажите конечную точку маршрута*\n" +
            "_скрепка \u27a1\ufe0f локация_ или _введите адрес вручную_",
            null, true, ParseMode.Markdown);
        
        _step++;
    }
    
    private async Task SetDestinationPoint(UserModel user)
    {
        if (user.Input.Location == null && String.IsNullOrEmpty(user.Input.Raw))
        {
            await SenderService.SendOrEditInlineKeyboard(user, 
                $"Некорректная локация, попробуйте еще раз", 
                null, true);
            return;
        }
        
        if (user.Input.Location == null && user.Input.Raw != null)
        {
            var location = await _yandexGeoApi.GetLocationByAddress(user.Input.Raw);

            if (location == null)
            {
                await SenderService.SendOrEditInlineKeyboard(user, 
                    $"Не удалось найти локацию по запросу: {user.Input.Raw}", 
                    null, true);
                return;
            }
            
            _route.DestinationPoint = location;
        }
        else if (user.Input.Location != null)
            _route.DestinationPoint = user.Input.Location;

        _route.DestinationPoint.Title = await _yandexGeoApi.GetAddressByLocation(_route.DestinationPoint);
        
        await SenderService.RemoveMessage(user, user.LastSendMessage.MessageId);
        await SenderService.SendOrEditInlineKeyboard(user,
            $"Точки заданы! Пожалуйста выберите *класс такси:*",
            _taxiClassesElements, true, ParseMode.Markdown);

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
            $"\n\nВыберите метод отслеживания\n\n" +
            $"*По изменению цены* - вы будете уведомлены, если цена начнет изменяться (расти или падать).\n" +
            $"*По лимиту* - вы будете уведомлены, когда цена достигнет заданного вами значения.",
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
            message = $"Вы будете уведомлены при изменении цены в {_route.TrackingPrice} руб.";

        await _routesService.Create(_route, user.Id);

        await _addressesService.Create(new AddressDbModel(Ulid.NewUlid().ToString(), user.Id, 
            $"{_route.DeparturePoint.Title}➡️{_route.DestinationPoint.Title}",
            _route.DeparturePoint, _route.DestinationPoint, DateTime.UtcNow));
        
        await SenderService.SendOrEditInlineKeyboard(user,
            message,
            null, 
            true);
    }

    private async Task PrintAddressesFromSaved(UserModel user)
    {
        var keyboardWithAddresses = new List<InlineKeyboardElement>();
        
        _addresses?.ForEach(x
            => keyboardWithAddresses.Add(new(x.Title, x.Id)));
        
        await SenderService.SendOrEditInlineKeyboard(user, "Ваши адреса",
            keyboardWithAddresses, true, ParseMode.Markdown);

        _step = TrackRouteStep.SetAddressFromHistory;
    }

    private async Task SetAddressFromSaved(UserModel user)
    {
        if(user.Input.Raw == null)
            return;

        var addressId = user.Input.Raw;

        var address = _addresses?.FirstOrDefault(x => x.Id == addressId);

        if (address == null)
            return;

        _route.DestinationPoint = address.DestinationPoint;
        _route.DeparturePoint = address.DeparturePoint;

        _step = TrackRouteStep.SetTaxiClass;
        
        await SenderService.SendOrEditInlineKeyboard(user,
            $"Адрес задан! Пожалуйста выберите *класс такси:*",
            _taxiClassesElements, true, ParseMode.Markdown);
    }
    
    public RouteInfoMode(SenderService sender, ITelegramBotClient tgBotClient, YandexGoApi yandexGoApi, 
        RoutesService routesService, AddressesService addressesService, YandexGeoApi yandexGeoApi) 
        : base(sender, tgBotClient)
    {
        _yandexGoApi = yandexGoApi;
        _routesService = routesService;
        _addressesService = addressesService;
        _yandexGeoApi = yandexGeoApi;
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
    SetPrice,
    SetAddressFromHistory
}