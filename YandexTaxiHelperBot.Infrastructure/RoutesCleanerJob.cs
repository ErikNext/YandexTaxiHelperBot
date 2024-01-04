using Microsoft.Extensions.Logging;
using Quartz;
using YandexTaxiHelperBot.Core.Services.RoutesService;
using YandexTaxiHelperBot.Repository.Users;

namespace YandexTaxiHelperBot.Infrastructure;

public class RoutesCleanerJob : IJob
{
    private readonly ILogger<RoutesJob> _logger;
    private readonly RoutesService _routesService;

    public RoutesCleanerJob(ILogger<RoutesJob> logger, RoutesService routesService, IUserDatabase usersDatabase)
    {
        _logger = logger;
        _routesService = routesService;
    }

    public Task Execute(IJobExecutionContext context)
    { 
        return _routesService.DeleteRoutesCreatedSometimeAgo(3);
    }
}