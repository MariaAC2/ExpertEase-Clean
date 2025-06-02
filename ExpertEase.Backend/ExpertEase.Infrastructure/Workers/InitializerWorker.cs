using ExpertEase.Application.Services;
using ExpertEase.Domain.Enums;
using ExpertEase.Infrastructure.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace ExpertEase.Infrastructure.Workers;

/// <summary>
/// This is an example of a worker service, this service is called on the applications start to do some asynchronous work.
/// The logger instance is injected here to provide logging.
/// The service provider is injected to request other components on runtime at request.
/// </summary>
public class InitializerWorker(ILogger<InitializerWorker> logger, IServiceProvider serviceProvider) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        try
        {
            await using var scope = serviceProvider.CreateAsyncScope(); // Here a new scope is created, this is useful to get new scoped instances.
            var userService = scope.ServiceProvider.GetService<IUserService>(); // Here an instance for a service is requested, it may fail if the component is not declared or
                                                                                // an exception is thrown on its creation.

            if (userService == null)
            {
                logger.LogInformation("Couldn't create the user service!");

                return;
            }

            var count = await userService.GetUserCount(cancellationToken);

            if (count.Result == 0)
            {
                logger.LogInformation("No user found, adding default user!");

                await userService.AddUser(new()
                {
                    Email = "admin@default.com",
                    FullName = "Admin",
                    Role = UserRoleEnum.SuperAdmin,
                    Password = PasswordUtils.HashPassword("default")
                }, cancellationToken: cancellationToken);
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "An error occurred while initializing database!");
        }
    }
}