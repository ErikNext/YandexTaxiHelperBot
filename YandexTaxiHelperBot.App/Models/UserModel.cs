using System.Drawing;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using Telegram.Bot.Types;
using YandexTaxiHelperBot.App.BotCommands;

namespace YandexTaxiHelperBot.App.Models;

public class UserModel
{
    public string Id { get; set; }
    public long TelegramId { get; }
    public string Username { get; }
    public Message? LastSendMessage { get; set; }
    public Message? LastReceivedMessage { get; set; }
    public ModeBase? CurrentMode { get; set; }
    public UserInput Input { get; set; } = new();

    public UserModel(string id, long telegramId, string username)
    {
        Id = id;
        TelegramId = telegramId;
        Username = username;
    }
}