using Ardalis.Specification;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.PaymentDTOs;
using ExpertEase.Application.DataTransferObjects.ReplyDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Application.Services;

public interface IReplyService
{
    Task<ServiceResponse<ReplyPaymentDetailsDTO>> GetReply(Guid replyId, CancellationToken cancellationToken = default);
    Task<ServiceResponse<PagedResponse<ReplyDTO>>> GetReplies(Specification<Reply, ReplyDTO> spec, PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default);
    // public Task<ServiceResponse<int>> GetUserCount(CancellationToken cancellationToken = default);
    Task<ServiceResponse> AddReply(Guid requestId, ReplyAddDTO reply, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> UpdateReply(ReplyUpdateDTO reply, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> UpdateReplyStatus(StatusUpdateDTO reply, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> DeleteReply(Guid id, UserDTO? requestingUser = null, CancellationToken cancellationToken = default);
    Task<ServiceResponse> ConfirmReplyPayment(Guid replyId, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default);
}