using System;
using System.Threading;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;

namespace YandexTaxiHelperBot.App.TgBotWrapper;

public class BotMessageReceiver
{
    private readonly Func<ITelegramBotClient, Update, CancellationToken, Task> _receiveMessage;
    private readonly Func<ITelegramBotClient, Exception, CancellationToken, Task> _handleError;
    private readonly TelegramBotClient _botClient;

    public BotMessageReceiver(Func<ITelegramBotClient, Update, CancellationToken, Task> receiveMessage,
        Func<ITelegramBotClient, Exception, CancellationToken, Task> handleError,
        BotConfiguration configuration)
    {
        _receiveMessage = receiveMessage;
        _handleError = handleError;
        _botClient = new TelegramBotClient(configuration.BotToken);
    }

    public ITelegramBotClient TelegramBotClient => _botClient;
    
    public void StartReceive(CancellationTokenSource cts, ReceiverOptions? options = null)
    {
        if (options == null)
        {
            options = new ReceiverOptions { AllowedUpdates = { } };
        }

        _botClient.DeleteWebhookAsync(true);
        _botClient.StartReceiving(_receiveMessage, _handleError, options, cancellationToken: cts.Token);
    }

    public void EndReceive()
    {
        _botClient.DeleteWebhookAsync(true);
    }
}