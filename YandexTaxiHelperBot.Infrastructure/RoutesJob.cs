﻿using System.Diagnostics;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using Quartz;
using YandexTaxiHelperBot.App.Extensions;
using YandexTaxiHelperBot.Contracts;
using YandexTaxiHelperBot.Core.Services.RoutesService;
using YandexTaxiHelperBot.Integrations.YandexGoApi;

namespace YandexTaxiHelperBot.Infrastructure;

public class RoutesJob : IJob
{
    private readonly ILogger<RoutesJob> _logger;
    private readonly RoutesService _service;
    private readonly INotificationService _notificationService;
    private readonly RoutesService _routesService;
    private readonly YandexGoApi _yandexGoApi;

    private readonly InlineKeyboardElement _commandCancelTrackingElemenet =
        new InlineKeyboardElement("Остановить", "StopRouteTrackingCommand");

    public RoutesJob(ILogger<RoutesJob> logger, 
        RoutesService service, 
        INotificationService notificationService,
        RoutesService routesService, YandexGoApi yandexGoApi)
    {
        _logger = logger;
        _service = service;
        _notificationService = notificationService;
        _routesService = routesService;
        _yandexGoApi = yandexGoApi;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var routes = await _routesService.GetAll();

        foreach (var route in routes)
        {
            var currentRouteInfo = await GetRouteInfoFromApi(route);

            if (currentRouteInfo == null)
                break;
            
            var process = route.Method switch
            {
                TrackingMethod.ByPriceChange => ProcessByPriceChange(route, currentRouteInfo),
                TrackingMethod.ByLimit => ProcessByLimit(route, currentRouteInfo),
                _ => throw new ArgumentException(
                    $"Invalid route.Method value '{route.Method}' in {nameof(RoutesJob)}")
            };

            await process;
        }
    }
    
    private async Task ProcessByPriceChange(RouteModel route, RouteInfo currentRouteInfo)
    {
        var priceChange = route.LastPrice - currentRouteInfo.Price;
        
        if (Math.Abs(priceChange) >= route.TrackingPrice)
        {
            string message = "";
            
            if (priceChange > 0)
            {
                message = $"Цена упала \u2b07\ufe0f более чем на {route.TrackingPrice} руб." +
                                 $"\nНовая цена: {currentRouteInfo.Price}\n" +
                                 $"\nСтарая цена: {route.LastPrice}";  
            }
            else if (priceChange < 0)
            {
                message = $"Цена поднялась \u2b06\ufe0f более чем на {route.TrackingPrice} руб." +
                                 $"\nНовая цена: {currentRouteInfo.Price}\n" +
                                 $"\nСтарая цена: {route.LastPrice}";  
            }

            await _notificationService.SendMessageWithButton(route.UserId, message, _commandCancelTrackingElemenet);
        }
        
        route.LastPrice = currentRouteInfo.Price;
        await _routesService.UpdatePrice(route.Id, currentRouteInfo.Price);
    }
    
    private async Task ProcessByLimit(RouteModel route, RouteInfo currentRouteInfo)
    {
        route.LastPrice = currentRouteInfo.Price;
        await _routesService.UpdatePrice(route.Id, currentRouteInfo.Price);
        
        if (currentRouteInfo.Price <= route.TrackingPrice)
        {
            await _notificationService.SendMessageWithButton(route.UserId, 
                $"Цена достигла заданного лимита. Текущая цена: {currentRouteInfo.Price} руб.", 
                _commandCancelTrackingElemenet);

            await _routesService.Delete(route.Id);
        }
    }
    
    private async Task<RouteInfo?> GetRouteInfoFromApi(RouteModel route)
    {
        return (await _yandexGoApi.GetRouteInfo(
            (route.DeparturePoint.Longitude, route.DeparturePoint.Latitude),
            (route.DestinationPoint.Longitude, route.DestinationPoint.Latitude),
            route.Class));
    }
}