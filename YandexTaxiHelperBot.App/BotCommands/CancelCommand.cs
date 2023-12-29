using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types;
using YandexTaxiHelperBot.App.Models;
using YandexTaxiHelperBot.App.Services;

namespace YandexTaxiHelperBot.App.BotCommands;

public class CancelCommand : CommandBase
{
    public CancelCommand(SenderService sender) : base(sender) { }

    private CommandBase? _assignmentCommand;
    public override string Title => "Отменить";
    public override string Key => nameof(CancelCommand);
    protected override Task ExecuteInternal(ITelegramBotClient telegramBotClient, UserModel user, string? data = default)
    {
        if (_assignmentCommand == null)
        {
            StorageCommands.Commands.TryGetValue(nameof(MainMenuCommand), out var mainMenuCommand);
            _assignmentCommand = mainMenuCommand;
        }
        
        user.CurrentMode = null;
        return _assignmentCommand?.Execute(telegramBotClient, user, data) ?? Task.CompletedTask;
    }

    public void SetAssignmentCommand(CommandBase command)
    {
        _assignmentCommand = command;
    }

    public override Task<bool> Filter(UserModel user)
    {
        if (user.CurrentMode != null)
            return Task.FromResult(true);
        
        return Task.FromResult(false);
    }
}