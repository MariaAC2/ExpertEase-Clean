using System.Net;
using ExpertEase.Application.DataTransferObjects.ReviewDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Application.Specifications;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class ReviewService(IRepository<WebAppDatabaseContext> repository): IReviewService
{
    public async Task<ServiceResponse> AddReview(Guid serviceTaskId, ReviewAddDTO review, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
        {
            return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.Forbidden, "User not found", ErrorCodes.CannotAdd));
        }
        if (requestingUser.Role == UserRoleEnum.Admin)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only users can create reviews", ErrorCodes.CannotAdd));
        }
        
        var sender = await repository.GetAsync(new UserSpec(requestingUser.Id), cancellationToken);

        if (sender == null)
        {
            return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.NotFound, "User not found", ErrorCodes.EntityNotFound));
        }

        if (sender.Id != requestingUser.Id)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the own user can create reviews", ErrorCodes.CannotAdd));
        }
        
        var receiver = await repository.GetAsync(new UserSpec(review.ReceiverUserId), cancellationToken);
        
        if (receiver == null)
        {
            return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.NotFound, "User with this ID not found", ErrorCodes.EntityNotFound));
        }
        
        var serviceTask = await repository.GetAsync(new ServiceTaskSpec(serviceTaskId), cancellationToken);
        
        if (serviceTask == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Service task not found", ErrorCodes.EntityNotFound));
        }
        
        var reviewEntity = new Review
        {
            SenderUserId = requestingUser.Id,
            SenderUser = sender,
            ReceiverUserId = review.ReceiverUserId,
            ReceiverUser = receiver,
            ServiceTaskId = serviceTaskId,
            ServiceTask = serviceTask,
            Content = review.Content,
            Rating = review.Rating
        };
        
        var existingRequest = await repository.GetAsync(new ReviewSearchSpec(reviewEntity), cancellationToken);
        
        if (existingRequest != null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Conflict, "Request already exists", ErrorCodes.CannotAdd));
        }
        
        await repository.AddAsync(reviewEntity, cancellationToken);
        receiver.Reviews.Add(reviewEntity);
        
        var allReviews = await repository.ListAsync(new ReviewProjectionSpec(reviewEntity.ReceiverUserId), cancellationToken);
        var total = allReviews.Count;
        var average = allReviews.Average(r => r.Rating);
        receiver.Rating = (int)Math.Round(average, MidpointRounding.AwayFromZero);
        await repository.UpdateAsync(receiver, cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse<ReviewDTO>> GetReview(Guid id, Guid userId, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new ReviewProjectionSpec(id, userId), cancellationToken);
        
        if (result == null)
        {
            return ServiceResponse.CreateErrorResponse<ReviewDTO>(new (HttpStatusCode.NotFound, "Request not found", ErrorCodes.EntityNotFound));
        }

        return ServiceResponse.CreateSuccessResponse(result);
    }
    
    public async Task<ServiceResponse<ReviewAdminDTO>> GetReviewAdmin(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new ReviewAdminProjectionSpec(id), cancellationToken);
        
        if (result == null)
        {
            return ServiceResponse.CreateErrorResponse<ReviewAdminDTO>(new (HttpStatusCode.NotFound, "Request not found", ErrorCodes.EntityNotFound));
        }

        return ServiceResponse.CreateSuccessResponse(result);
    }
    
    public async Task<ServiceResponse<PagedResponse<ReviewDTO>>> GetReviews(Guid userId, PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    {
        var result = await repository.PageAsync(pagination, new ReviewProjectionSpec(pagination.Search, userId),  cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse(result);
    }
    
    public async Task<ServiceResponse<PagedResponse<ReviewAdminDTO>>> GetReviewsAdmin(PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    {
        var result = await repository.PageAsync(pagination, new ReviewAdminProjectionSpec(pagination.Search),  cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse(result);
    }

    public async Task<ServiceResponse> UpdateRequest(ReviewUpdateDTO review, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetAsync(new ReviewSpec(review.Id), cancellationToken);

        if (entity == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Request not found", ErrorCodes.EntityNotFound));
        }
        
        entity.Content = review.Content ?? entity.Content;
        entity.Rating = review.Rating ?? entity.Rating;
        
        await repository.UpdateAsync(entity, cancellationToken);
        return ServiceResponse.CreateSuccessResponse();
    }
    
    public async Task<ServiceResponse> DeleteReview(Guid id, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        var entity = await repository.GetAsync(new ReviewSpec(id), cancellationToken);

        if (entity == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Request not found", ErrorCodes.EntityNotFound));
        }
        
        await repository.DeleteAsync<Review>(id, cancellationToken);
        return ServiceResponse.CreateSuccessResponse();
    }

}