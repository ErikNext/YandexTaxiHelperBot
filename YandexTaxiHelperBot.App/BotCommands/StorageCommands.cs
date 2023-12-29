using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace YandexTaxiHelperBot.App.BotCommands;

public static class StorageCommands
{
    public static Dictionary<string, CommandBase> Commands = new();
    
    public static void Init(IServiceProvider serviceProvider)
    {
        var x = Assembly.GetAssembly(typeof(CommandBase)).GetTypes();
        
        foreach (Type type in
                 Assembly.GetAssembly(typeof(CommandBase)).GetTypes()
                     .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(CommandBase))))
        {
            var command = (CommandBase)serviceProvider.GetService(type);
            Commands.Add(command.Key, command);
        }
    }
}