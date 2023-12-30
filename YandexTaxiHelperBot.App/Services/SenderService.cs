using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using YandexTaxiHelperBot.App.BotCommands;
using YandexTaxiHelperBot.App.Extensions;
using YandexTaxiHelperBot.App.Models;
using YandexTaxiHelperBot.Contracts;

namespace YandexTaxiHelperBot.App.Services;

public class SenderService
{
    private ITelegramBotClient _botClient;
    private readonly ILogger<HandleUpdateService> _logger;

    public SenderService(ILogger<HandleUpdateService> logger)
    {
        _logger = logger;
    }

    public void SetTelegramBotClient(ITelegramBotClient botClient)
    {
        _botClient = botClient;
    }

    public async Task<Message> SendMessage(long userId, string message)
    {
        _logger.LogInformation($"Send message: '{message}' to user with chat id: {userId}");

        var sentMessage = await _botClient.SendTextMessageAsync(
            chatId: userId,
            text: message);

        return sentMessage;
    }

    public async Task SendInlineKeyboard(
        UserModel user,
        string message,
        bool withMenu,
        ICollection<InlineKeyboardElement>? keyboardElements = null,
        ParseMode parseMode = ParseMode.Html)
    {
        keyboardElements ??= new List<InlineKeyboardElement>();
        
        if(withMenu)
            keyboardElements.Add(new InlineKeyboardElement("Меню", nameof(MainMenuCommand)));
        
        var buttons = new List<InlineKeyboardButton[]>();

        foreach (var item in keyboardElements)
        {
            var button = InlineKeyboardButton.WithCallbackData(item.Text, item.CallbackData);
            
            if (item.Url != null)
                button.Url = item.Url;
            
            buttons.Add(new[] { button });
        }

        var sentMessage = await _botClient.SendTextMessageAsync(chatId: user.TelegramId,
            text: message,
            replyMarkup: new InlineKeyboardMarkup(buttons),
            parseMode: parseMode);

        user.LastReceivedMessage = sentMessage;
    }

    public async Task RemoveMessage(UserModel user, int messageId)
    {
        await _botClient.DeleteMessageAsync(user.TelegramId, messageId);
    }

    public async Task SendAllAvailableCommands(
        UserModel user,
        string message,
        ParseMode parseMode = ParseMode.Html)
    {
        var commands = StorageCommands.Commands;
        var elements = new List<InlineKeyboardElement>();

        foreach (var command in commands)
        {
            if (await command.Value.Filter(user))
            {
                elements.Add(new InlineKeyboardElement(command.Value.Title, command.Value.Key));
            }
        }
        
        await SendOrEditInlineKeyboard(user, message, elements);
    }

    public async Task SendOrEditInlineKeyboard(
        UserModel user,
        string message,
        ICollection<InlineKeyboardElement>? keyboardElements = null,
        bool withMenu = false,
        ParseMode parseMode = ParseMode.Html)
    {
        keyboardElements ??= new List<InlineKeyboardElement>();
        
        if (user.LastReceivedMessage == null || user.LastReceivedMessage.Text == message)
        {
            await SendInlineKeyboard(user, message, withMenu, keyboardElements);
            return;
        }
        
        if(withMenu)
            keyboardElements.Add(new InlineKeyboardElement("Меню", nameof(MainMenuCommand)));

        var buttons = new List<InlineKeyboardButton[]>();

        foreach (var item in keyboardElements)
        {
            var button = InlineKeyboardButton.WithCallbackData(item.Text, item.CallbackData);

            if (item.Url != null)
                button.Url = item.Url;
            
            buttons.Add(new[] { button });
        }

        try
        {
            var sentMessage = await _botClient.EditMessageTextAsync(chatId: user.TelegramId,
                messageId: user.LastReceivedMessage.MessageId,
                text: message,
                replyMarkup: new InlineKeyboardMarkup(buttons),
                parseMode: parseMode);
            
            user.LastReceivedMessage = sentMessage;
        }
        catch
        {
            var sentMessage = await _botClient.SendTextMessageAsync(chatId: user.TelegramId,
                text: message,
                replyMarkup: new InlineKeyboardMarkup(buttons),
                parseMode: parseMode);
            
            user.LastReceivedMessage = sentMessage;
        }
    }

    public async Task EditMessage(
        long chatId,
        int messageId,
        string message)
    {
        await _botClient.EditMessageTextAsync(chatId: chatId,
            messageId: messageId,
            text: message);
    }
}