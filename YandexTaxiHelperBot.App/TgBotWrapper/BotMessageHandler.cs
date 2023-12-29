using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using YandexTaxiHelperBot.App.Services;

namespace YandexTaxiHelperBot.App.TgBotWrapper;

public sealed class BotMessageHandler
{
    private readonly ILogger<BotMessageHandler> _logger;
    private readonly HandleUpdateService _handleUpdateService;

    public BotMessageHandler(ILogger<BotMessageHandler> logger, HandleUpdateService handleUpdateService)
    {
        _logger = logger;
        _handleUpdateService = handleUpdateService;
    }

    public Task HandleMessage(ITelegramBotClient botClient, Update update, CancellationToken token)
    {
        return _handleUpdateService.EchoAsync(update, botClient);
    }

    public Task HandleError(ITelegramBotClient client, Exception exception, CancellationToken token)
    {
        var errorMessage = exception switch
        {
            ApiRequestException apiRequestException
                => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogError(errorMessage);

        return Task.CompletedTask;
    }
}