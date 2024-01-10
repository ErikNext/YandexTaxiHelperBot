using Telegram.Bot;
using YandexTaxiHelperBot.App.Models;
using YandexTaxiHelperBot.App.Services;
using YandexTaxiHelperBot.Repository.Address;

namespace YandexTaxiHelperBot.App.BotCommands;

public class UserAddressesCommand : CommandBase
{
    private readonly IAddressesDatabase _database;
    
    public UserAddressesCommand(SenderService sender, IAddressesDatabase database) : base(sender)
    {
        _database = database;
    }

    public override string Title => "Мои адреса";
    public override string Key => nameof(UserAddressesCommand);
    
    protected override Task ExecuteInternal(ITelegramBotClient telegramBotClient, UserModel user, string? data = default)
    {
        throw new NotImplementedException();
    }

    public override Task<bool> Filter(UserModel user)
    {
        return Task.FromResult(false);
    }
}