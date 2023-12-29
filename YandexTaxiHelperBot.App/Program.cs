using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using YandexTaxiHelperBot.App;
using YandexTaxiHelperBot.App.Extensions;
using YandexTaxiHelperBot.App.Services;
using YandexTaxiHelperBot.App.TgBotWrapper;
using YandexTaxiHelperBot.Contracts;
using YandexTaxiHelperBot.Core.Services.RoutesService;
using YandexTaxiHelperBot.Infrastructure;
using YandexTaxiHelperBot.Integrations.YandexGoApi;
using YandexTaxiHelperBot.Repository;
using YandexTaxiHelperBot.Repository.Routes;
using YandexTaxiHelperBot.Repository.Users;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddHostedService<StartHostedService>();
builder.Services.AddSingleton<BotMessageHandler>();
builder.Services.AddSingleton<HandleUpdateService>();
builder.Services.AddSingleton<SenderService>();
builder.Services.AddSingleton<UsersService>();
builder.Services.AddSingleton<YandexGoApi>();
builder.Services.AddSingleton<RoutesService>();
builder.Services.AddSingleton<IUserDatabase, UserMongoDatabase>();
builder.Services.AddSingleton<IRoutesDatabase, RoutesMongoDatabase>();
builder.Services.AddSingleton<INotificationService, NotificationService>();

builder.Services.AddAllCommandsAsSingleton();

builder.Services.AddInfrastructure();

builder.Services.Configure<BotConfiguration>(builder.Configuration.GetSection("BotConfiguration"));
builder.Services.Configure<YandexGoApiConfiguration>(builder.Configuration.GetSection("YandexGoApiConfiguration"));
builder.Services.Configure<UserDbConfig>(builder.Configuration.GetSection(UserDbConfig.Section));
builder.Services.Configure<RouteForTrackingDbConfig>(builder.Configuration.GetSection(RouteForTrackingDbConfig.Section));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();