using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using YandexTaxiHelperBot.App.BotCommands;

namespace YandexTaxiHelperBot.App.Extensions
{
    public static class ServiceCollectionExtension
    {
        public static void AddAllCommandsAsSingleton(this IServiceCollection services)
        {
            var allTypes = Assembly.GetAssembly(typeof(CommandBase)).GetTypes();
            foreach (Type type in allTypes
                .Where(myType =>
                   myType.IsSubclassOf(typeof(CommandBase))
                && myType.IsClass
                && !myType.IsAbstract
                ))
            {
                services.AddSingleton(type);
            }
        }
    }
}
