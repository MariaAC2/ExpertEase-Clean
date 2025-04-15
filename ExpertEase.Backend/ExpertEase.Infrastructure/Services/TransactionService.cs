using System.Net;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.AccountDTOs;
using ExpertEase.Application.DataTransferObjects.TransactionDTOs;
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

public class TransactionService(IRepository<WebAppDatabaseContext> repository): ITransactionService
{
    public async Task<ServiceResponse> AddTransaction(TransactionAddDTO transaction, UserDTO? requestingUser, CancellationToken cancellationToken = default)
    {
        if (requestingUser == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "User cannot add transaction because it doesn't exist!", ErrorCodes.CannotAdd));
        }
        switch (transaction.TransactionType)
        {
            case TransactionEnum.Deposit:
            {
                if (transaction.ReceiverUserId == null)
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest,
                        "You can't deposit without a receiver id!"));

                if (transaction.ReceiverUserId != requestingUser.Id)
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                        "You can't access this user's account!", ErrorCodes.WrongUser));

                var receiver = await repository.GetAsync(new UserSpec(requestingUser.Id), cancellationToken);
                
                if (receiver == null)
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "User with this id not found!", ErrorCodes.EntityNotFound));
                
                var receiverAccount = await repository.GetAsync(new AccountUserSpec(requestingUser.Id), cancellationToken);

                if (receiverAccount == null)
                {
                    return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.NotFound,
                        "Account not found!", ErrorCodes.EntityNotFound));
                }

                if (requestingUser.Id != receiverAccount.UserId)
                {
                    return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.Forbidden,
                        "You are not allowed to access this account!", ErrorCodes.WrongUser));
                }

                var transactionToValidate = new Transaction
                {
                    InitiatorUserId = receiver.Id,
                    InitiatorUser = receiver,
                    ReceiverUserId = receiver.Id,
                    ReceiverUser = receiver,
                    ReceiverAccountId = receiverAccount.Id,
                    ReceiverAccount = receiverAccount,
                    ExternalSource = transaction.ExternalSource,
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    TransactionType = transaction.TransactionType
                };

                var validation = RejectReasonMessage.CreateRejectReasonMessage(transactionToValidate);

                if (!validation.IsValid)
                {
                    transactionToValidate.Status = StatusEnum.Rejected;
                    transactionToValidate.RejectionCode = validation.Reason;
                    transactionToValidate.RejectionDetails = validation.Message;

                    await repository.AddAsync(transactionToValidate, cancellationToken);
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest, validation.Message));
                }

                transactionToValidate.Status = StatusEnum.Pending;
                await repository.AddAsync(transactionToValidate, cancellationToken);
                break;
            }
            case TransactionEnum.Withdraw:
            {
                if (transaction.SenderUserId == null)
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest,
                        "You can't withdraw without a sender id!"));

                if (transaction.SenderUserId != requestingUser.Id)
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                        "You can't access this user's account!", ErrorCodes.WrongUser));

                var sender = await repository.GetAsync(new UserSpec(requestingUser.Id), cancellationToken);
                
                if (sender == null)
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "User with this id not found!", ErrorCodes.EntityNotFound));
                
                var senderAccount = await repository.GetAsync(new AccountUserSpec(requestingUser.Id), cancellationToken);

                if (senderAccount == null)
                {
                    return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.NotFound,
                        "Account not found!", ErrorCodes.EntityNotFound));
                }

                if (requestingUser.Id != senderAccount.UserId)
                {
                    return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.Forbidden,
                        "You are not allowed to access this account!", ErrorCodes.WrongUser));
                }

                var transactionToValidate = new Transaction
                {
                    InitiatorUserId = sender.Id,
                    InitiatorUser = sender,
                    SenderUserId = sender.Id,
                    SenderUser = sender,
                    SenderAccountId = senderAccount.Id,
                    SenderAccount = senderAccount,
                    Amount = transaction.Amount,
                    ExternalSource = transaction.ExternalSource,
                    Description = transaction.Description,
                    TransactionType = transaction.TransactionType
                };

                var validation = RejectReasonMessage.CreateRejectReasonMessage(transactionToValidate);

                if (!validation.IsValid)
                {
                    transactionToValidate.Status = StatusEnum.Rejected;
                    transactionToValidate.RejectionCode = validation.Reason;
                    transactionToValidate.RejectionDetails = validation.Message;

                    await repository.AddAsync(transactionToValidate, cancellationToken);
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest, validation.Message));
                }

                transactionToValidate.Status = StatusEnum.Pending;
                await repository.AddAsync(transactionToValidate, cancellationToken);
                break;
            }
            case TransactionEnum.Transfer:
            {
                if (transaction.SenderUserId == null || transaction.ReceiverUserId == null)
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest,
                        "You can't transfer without a sender or receiver id!"));

                if (transaction.SenderUserId != requestingUser.Id)
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                        "You can't access this user's account!", ErrorCodes.WrongUser));

                var sender = await repository.GetAsync(new UserSpec(requestingUser.Id), cancellationToken);
                
                if (sender == null)
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "User with this id not found!", ErrorCodes.EntityNotFound));
                
                var senderAccount = await repository.GetAsync(new AccountUserSpec(requestingUser.Id), cancellationToken);

                if (senderAccount == null)
                {
                    return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.NotFound,
                        "Account not found!", ErrorCodes.EntityNotFound));
                }

                if (requestingUser.Id != senderAccount.UserId)
                {
                    return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.Forbidden,
                        "You are not allowed to access this account!", ErrorCodes.WrongUser));
                }

                var receiver = await repository.GetAsync(new UserSpec(transaction.ReceiverUserId.Value), cancellationToken);
                
                if (receiver == null)
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "User with this id not found!", ErrorCodes.EntityNotFound));

                if (receiver.Role != UserRoleEnum.Specialist)
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Receiver should be a specialist!", ErrorCodes.CannotAdd));

                var receiverAccount = await repository.GetAsync(new AccountUserSpec(receiver.Id), cancellationToken);
                if (receiverAccount == null)
                {
                    return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.NotFound,
                        "Receiver's account not found!", ErrorCodes.EntityNotFound));
                }

                var transactionToValidate = new Transaction
                {
                    InitiatorUserId = sender.Id,
                    InitiatorUser = sender,
                    SenderUserId = sender.Id,
                    SenderUser = sender,
                    ReceiverUserId = receiver.Id,
                    ReceiverUser = receiver,
                    SenderAccountId = senderAccount.Id,
                    SenderAccount = senderAccount,
                    ReceiverAccountId = receiverAccount.Id,
                    ReceiverAccount = receiverAccount,
                    Amount = transaction.Amount,
                    Description = transaction.Description,
                    TransactionType = transaction.TransactionType
                };

                var validation = RejectReasonMessage.CreateRejectReasonMessage(transactionToValidate);

                if (!validation.IsValid)
                {
                    transactionToValidate.Status = StatusEnum.Rejected;
                    transactionToValidate.RejectionCode = validation.Reason;
                    transactionToValidate.RejectionDetails = validation.Message;

                    await repository.AddAsync(transactionToValidate, cancellationToken);
                    return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest, validation.Message));
                }

                transactionToValidate.Status = StatusEnum.Pending;
                await repository.AddAsync(transactionToValidate, cancellationToken);
                break;
            }

        }
        
        // add mail service (transaction added)
        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse<TransactionDTO>> GetTransaction(Guid id, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(new TransactionProjectionSpec(id), cancellationToken);
        return result != null ? 
            ServiceResponse.CreateSuccessResponse(result) : 
            ServiceResponse.CreateErrorResponse<TransactionDTO>(new(HttpStatusCode.NotFound, "Transaction not found!", ErrorCodes.EntityNotFound));
    }
    
    public async Task<ServiceResponse<PagedResponse<TransactionDTO>>> GetTransactions(PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    {
        var result = await repository.PageAsync(pagination, new TransactionProjectionSpec(pagination.Search), cancellationToken); // Use the specification and pagination API to get only some entities from the database.

        return ServiceResponse.CreateSuccessResponse(result);
    }

    public async Task<ServiceResponse<PagedResponse<TransactionDTO>>> GetTransactionsByUser(Guid userId,
        PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    {
        var result = await repository.PageAsync(pagination, new TransactionUserProjectionSpec(pagination.Search), cancellationToken); // Use the specification and pagination API to get only some entities from the database.

        return ServiceResponse.CreateSuccessResponse(result);
    }
    
    public async Task<ServiceResponse<int>> GetTransactionCount(CancellationToken cancellationToken = default) => 
        ServiceResponse.CreateSuccessResponse(await repository.GetCountAsync<Transaction>(cancellationToken));

    public async Task<ServiceResponse> UpdateTransaction(TransactionUpdateDTO transaction, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin && 
            (transaction.Status == StatusEnum.Accepted || transaction.Status == StatusEnum.Rejected))
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the admin can accept or reject transaction!", ErrorCodes.CannotUpdate));
        }

        if (requestingUser != null && requestingUser.Role == UserRoleEnum.Admin &&
            transaction.Status == StatusEnum.Cancelled)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the user can cancel the transaction!", ErrorCodes.CannotUpdate));
        }
        
        if (transaction.Status == StatusEnum.Rejected && transaction.Description == null)
        {
            return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.BadRequest, "Description cannot be null", ErrorCodes.CannotUpdate));
        }
        
        var transactionToUpdate = await repository.GetAsync(new TransactionIdSpec(transaction.Id), cancellationToken);
        
        if (transactionToUpdate == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Transaction not found!", ErrorCodes.EntityNotFound));
        }
        
        if (requestingUser != null && requestingUser.Role != UserRoleEnum.Admin && 
            transaction.Status == StatusEnum.Cancelled && transactionToUpdate.InitiatorUserId != requestingUser.Id)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the own user can cancel the transaction!", ErrorCodes.CannotUpdate));
        }
        
        if (transactionToUpdate.Status != StatusEnum.Pending)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest, "Transaction is already processed!", ErrorCodes.CannotUpdate));
        }

        if (transaction.Status == StatusEnum.Rejected)
        {
            transactionToUpdate.Status = StatusEnum.Rejected;
            transactionToUpdate.RejectionCode = RejectionReason.ManualReview;
            transactionToUpdate.RejectionDetails = transaction.Description;
        } else if (transaction.Status == StatusEnum.Accepted)
        {
            transactionToUpdate.Status = StatusEnum.Accepted;
            transactionToUpdate.RejectionCode = null;
            transactionToUpdate.RejectionDetails = null;
            
            switch (transactionToUpdate.TransactionType)
            {
                case TransactionEnum.Deposit:
                {
                    if (transactionToUpdate.ReceiverUserId == null)
                        return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest,
                            "You can't deposit without a receiver id!"));
                    
                    var receiverAccount = await repository.GetAsync(new AccountUserSpec(transactionToUpdate.ReceiverUserId.Value), 
                        cancellationToken);
                    if (receiverAccount == null)
                    {
                        return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.NotFound,
                            "Receiver's account not found!", ErrorCodes.EntityNotFound));
                    }
                    receiverAccount.Balance += transactionToUpdate.Amount;
                    await repository.UpdateAsync(receiverAccount, cancellationToken);
                } 
                    break;
                case TransactionEnum.Withdraw:
                {
                    if (transactionToUpdate.SenderUserId == null)
                        return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest,
                            "You can't withdraw without a sender id!"));
                    var senderAccount = await repository.GetAsync(new AccountUserSpec(transactionToUpdate.SenderUserId.Value), 
                        cancellationToken);
                    if (senderAccount == null)
                    {
                        return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.NotFound,
                            "Sender's account not found!", ErrorCodes.EntityNotFound));
                    }
                    senderAccount.Balance -= transactionToUpdate.Amount;
                    await repository.UpdateAsync(senderAccount, cancellationToken);
                }
                    break;

                case TransactionEnum.Transfer:
                {
                    if (transactionToUpdate.SenderUserId == null || transactionToUpdate.ReceiverUserId == null)
                        return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest,
                            "You can't transfer without a sender or receiver id!"));
                    var senderAccount = await repository.GetAsync(new AccountSpec(transactionToUpdate.SenderUserId.Value),
                        cancellationToken);
                    var receiverAccount = await repository.GetAsync(new AccountSpec(transactionToUpdate.ReceiverUserId.Value),
                        cancellationToken);
                    if (senderAccount == null)
                    {
                        return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.NotFound,
                            "Sender's account not found!", ErrorCodes.EntityNotFound));
                    }
                    if (receiverAccount == null)
                    {
                        return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.NotFound,
                            "Receiver's account not found!", ErrorCodes.EntityNotFound));
                    }
                    senderAccount.Balance -= transactionToUpdate.Amount;
                    receiverAccount.Balance += transactionToUpdate.Amount;
                    await repository.UpdateAsync(senderAccount, cancellationToken);
                    await repository.UpdateAsync(receiverAccount, cancellationToken);
                }
                    break;
            }
        } else
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest, "Invalid status!", ErrorCodes.CannotUpdate));
        }
        
        await repository.UpdateAsync(transactionToUpdate, cancellationToken);
        
        // add mail service (transaction accepted / rejected)
        return ServiceResponse.CreateSuccessResponse();
    }
    
    public async Task<ServiceResponse> CancelTransaction(Guid id, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser == null || requestingUser.Role == UserRoleEnum.Admin)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                "Only users are allowed to cancel transactions!", ErrorCodes.CannotUpdate));
        }

        var result = await repository.GetAsync(new TransactionIdSpec(id), cancellationToken);

        if (result == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Transaction not found!", ErrorCodes.EntityNotFound));
        }

        if (result.InitiatorUserId != requestingUser.Id)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                "You are not allowed to cancel this transaction!", ErrorCodes.CannotUpdate));
        }

        result.Status = StatusEnum.Cancelled;
        result.RejectionCode = RejectionReason.UserRequest;
        result.RejectionDetails = "Transaction cancelled by user request";
        
        await repository.UpdateAsync(result, cancellationToken);

        // add mail service (transaction cancelled)
        return ServiceResponse.CreateSuccessResponse();
    }
    
    public async Task<ServiceResponse> DeleteTransaction(Guid id, UserDTO? requestingUser = null,
        CancellationToken cancellationToken = default)
    {
        if (requestingUser == null || requestingUser.Role != UserRoleEnum.Admin)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                "Only admins are allowed to delete transactions!", ErrorCodes.CannotDelete));
        }

        var result = await repository.GetAsync(new TransactionIdSpec(id), cancellationToken);

        if (result == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Transaction not found!", ErrorCodes.EntityNotFound));
        }

        await repository.DeleteAsync<Transaction>(id, cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }
}