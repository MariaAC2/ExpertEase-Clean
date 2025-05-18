using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class ServiceTaskService(IRepository<WebAppDatabaseContext> repository): IServiceTaskService
{
}