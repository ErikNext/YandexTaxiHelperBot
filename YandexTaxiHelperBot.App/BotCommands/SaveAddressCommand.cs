using NUlid;
using Telegram.Bot;
using YandexTaxiHelperBot.App.Models;
using YandexTaxiHelperBot.App.Services;
using YandexTaxiHelperBot.Core.Services.RoutesService;
using YandexTaxiHelperBot.Repository.Address;

namespace YandexTaxiHelperBot.App.BotCommands;

public class SaveAddressCommand : CommandBase
{
    private IAddressesDatabase _addressesDatabase;
    private RoutesService _routesService;
    
    public SaveAddressCommand(SenderService sender,
        IAddressesDatabase addressesDatabase, 
        RoutesService routesService) : base(sender)
    {
        _addressesDatabase = addressesDatabase;
        _routesService = routesService;
    }

    public override string Title => "Сохранить адрес";
    public override string Key => nameof(SaveAddressCommand);
    protected override Task ExecuteInternal(ITelegramBotClient telegramBotClient, UserModel user, string? data = default)
    {
        user.CurrentMode = new SaveAddressMode(Sender, telegramBotClient, _addressesDatabase, _routesService);
        return user.CurrentMode.Execute(user);
    }

    public override Task<bool> Filter(UserModel user)
    {
        return Task.FromResult(false);
    }
}

public class SaveAddressMode : ModeBase
{
    private SaveAddressStep _step = SaveAddressStep.Initial;
    private IAddressesDatabase _database;
    private RoutesService _routesService;
    public override Task Execute(UserModel user)
    {
        var task = _step switch
        {
            SaveAddressStep.Initial => Initial(user),
            SaveAddressStep.SetTitle => SetTitle(user),
            _ => throw new ArgumentException($"Invalid step value '{_step}' in {nameof(SaveAddressMode)}")
        };

        return task;
    }

    private Task Initial(UserModel user)
    {
        _step++;
        
        return SenderService.SendOrEditInlineKeyboard(user, 
            "Задайте название адреса", 
            null, 
            true);
    }

    private async Task SetTitle(UserModel user)
    {
        await SenderService.RemoveMessage(user, user.LastSendMessage.MessageId);
        
        if (String.IsNullOrEmpty(user.Input.Raw))
            return;

        var userTracking = await _routesService.GetUserTracking(user.Id);

        if (userTracking == null)
            return;

        var addressModel = new AddressDbModel(
            Ulid.NewUlid().ToString(),
            user.Id,
            user.Input.Raw,
            userTracking.DeparturePoint,
            userTracking.DestinationPoint);

        await _database.Create(addressModel);
        
        await SenderService.SendOrEditInlineKeyboard(user, 
            "Адресс сохранен", 
            null, 
            true);
    }
    
    public SaveAddressMode(SenderService sender, ITelegramBotClient tgBotClient, IAddressesDatabase database, RoutesService routesService) : base(sender, tgBotClient)
    {
        _database = database;
        _routesService = routesService;
    }
}

public enum SaveAddressStep : byte
{
    Unknown,
    Initial,
    SetTitle,
    Save
}
