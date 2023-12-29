using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using YandexTaxiHelperBot.App.BotCommands;
using YandexTaxiHelperBot.App.TgBotWrapper;

namespace YandexTaxiHelperBot.App.Services
{
    public class StartHostedService : IHostedService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly BotMessageReceiver _receiver;
        private readonly SenderService _sender;
        
        public StartHostedService(IServiceProvider serviceProvider, BotMessageHandler handler,
            IOptions<BotConfiguration> configuration, SenderService sender)
        {
            _serviceProvider = serviceProvider;
            _sender = sender;
            _receiver = new BotMessageReceiver(handler.HandleMessage, handler.HandleError, configuration.Value);
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            StorageCommands.Init(_serviceProvider);
            _receiver.StartReceive(new CancellationTokenSource());
            
            _sender.SetTelegramBotClient(_receiver.TelegramBotClient);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _receiver.EndReceive();
            return Task.CompletedTask;
        }
    }
}