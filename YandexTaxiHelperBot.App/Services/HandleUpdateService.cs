using Telegram.Bot;
using Telegram.Bot.Exceptions;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YandexTaxiHelperBot.App.BotCommands;
using YandexTaxiHelperBot.App.Models;

namespace YandexTaxiHelperBot.App.Services;

public class HandleUpdateService
{
    private readonly ILogger<HandleUpdateService> _logger;
    private readonly SenderService _sender;
    private readonly UsersService _usersService;
    private readonly Dictionary<string, CommandBase> _commands;

    public HandleUpdateService(
        ILogger<HandleUpdateService> logger,
        SenderService sender,
        UsersService usersService)
    {
        _logger = logger;
        _sender = sender;
        _commands = StorageCommands.Commands;
        _usersService = usersService;
    }

    public async Task EchoAsync(Update update, ITelegramBotClient botClient)
    {
        var handler = update.Type switch
        {
            UpdateType.Message            => BotOnMessageReceived(update.Message!, botClient),
            UpdateType.CallbackQuery      => BotOnCallbackQueryReceived(update.CallbackQuery!, botClient),
            _                             => UnknownUpdateHandlerAsync(update)
        };

        try
        {
            await handler;
        }
        catch (Exception exception)
        {
            await HandleErrorAsync(exception);
        }
    }

    private async Task BotOnMessageReceived(Message message, ITelegramBotClient botClient)
    {
        var user = await _usersService.GetOrCreate(message.From.Id, message.From.Username);
        
        if (message.Location != null)
            user.Input.Location = new Contracts.Location(message.Location.Latitude, message.Location.Longitude);
        
        user.LastSendMessage = message;
        user.Input.Raw = message.Text;

        await UserAction(user, botClient);
    }

    private async Task BotOnCallbackQueryReceived(CallbackQuery callbackQuery, ITelegramBotClient botClient)
    {
        var user = await _usersService.GetOrCreate(callbackQuery.From.Id, callbackQuery.From.Username);
        user.Input.Raw = callbackQuery.Data;
        
        await UserAction(user, botClient);
    }
    
    private Task UserAction(UserModel user, ITelegramBotClient botClient)
    {
        if (user.Input.Raw != null && _commands.TryGetValue(user.Input.Raw!, out var command))
        {
            _logger.LogInformation($"{DateTime.UtcNow} User: '@{user.Username}' execute command: " +
                                   $"'{command.Title}'"); 
            return command.Execute(botClient, user, user.Input.Raw);
        }
        
        if (user.CurrentMode is not null)
            return user.CurrentMode.Execute(user);

        if (_commands.TryGetValue(nameof(MainMenuCommand), out var mainMenucommand))
        {
            _logger.LogInformation($"{DateTime.UtcNow} User: '@{user.Username}' execute command: " +
                                   $"'{mainMenucommand.Title}'"); 
             return mainMenucommand.Execute(botClient, user, user.Input.Raw);
        }

        return Task.CompletedTask;
    }

    private Task UnknownUpdateHandlerAsync(Update update)
    {
        _logger.LogInformation("Unknown update type: {UpdateType}", update.Type);

        return Task.CompletedTask;
    }

    public Task HandleErrorAsync(Exception exception)
    {
        var ErrorMessage = exception switch
        {
            ApiRequestException apiRequestException => $"Telegram API Error:\n[{apiRequestException.ErrorCode}]\n{apiRequestException.Message}",
            _ => exception.ToString()
        };

        _logger.LogInformation("HandleError: {ErrorMessage}", ErrorMessage);
        return Task.CompletedTask;
    }
}
