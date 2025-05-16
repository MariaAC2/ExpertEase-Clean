using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class ReviewServices(IRepository<WebAppDatabaseContext> repository): IReviewService
{
    
}