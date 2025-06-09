﻿using System.Net;
using Ardalis.Specification;
using ExpertEase.Application.Constants;
using ExpertEase.Application.DataTransferObjects.AccountDTOs;
using ExpertEase.Application.DataTransferObjects.TransactionDTOs;
using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Application.Errors;
using ExpertEase.Application.Requests;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;
using ExpertEase.Domain.Specifications;
using ExpertEase.Infrastructure.Database;
using ExpertEase.Infrastructure.Repositories;

namespace ExpertEase.Infrastructure.Services;

public class TransactionService(IRepository<WebAppDatabaseContext> repository,
    ITransactionSummaryGenerator transactionGenerator,
    IMailService mailService): ITransactionService
{
    public async Task<ServiceResponse> AddTransaction(TransactionAddDTO transaction, UserDTO? requestingUser, CancellationToken cancellationToken = default)
    {
        // if (requestingUser == null)
        // {
        //     return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "User cannot add transaction because it doesn't exist!", ErrorCodes.CannotAdd));
        // }
        //
        // var initiatorUserId = Guid.Empty;
        // if (transaction.TransactionType == TransactionEnum.Deposit)
        // {
        //     if (transaction.ReceiverUserId == null)
        //         return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest,
        //         "You can't deposit without a receiver id!"));
        //     initiatorUserId = transaction.ReceiverUserId.Value;
        // }
        // else if (transaction.TransactionType == TransactionEnum.Withdraw)
        // {
        //     if (transaction.SenderUserId == null)
        //         return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest,
        //         "You can't withdraw without a sender id!"));
        //     initiatorUserId = transaction.SenderUserId.Value;
        // }
        //
        // if (initiatorUserId != requestingUser.Id)
        //         return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
        //             "You can't access this user's account!", ErrorCodes.WrongUser));
        //
        // var initiator = await repository.GetAsync(new UserSpec(requestingUser.Id), cancellationToken);
        //
        // if (initiator == null)
        //     return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "User with this id not found!", ErrorCodes.EntityNotFound));
        //
        // var initiatorAccount = await repository.GetAsync(new AccountUserSpec(requestingUser.Id), cancellationToken);
        //
        // if (initiatorAccount == null)
        // {
        //     return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.NotFound,
        //         "Account not found!", ErrorCodes.EntityNotFound));
        // }
        //
        // if (requestingUser.Id != initiatorAccount.UserId)
        // {
        //     return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.Forbidden,
        //         "You are not allowed to access this account!", ErrorCodes.WrongUser));
        // }
        //
        // var transactionToValidate = new Transaction
        // {
        //     InitiatorUserId = initiator.Id,
        //     InitiatorUser = initiator,
        //     ExternalSource = transaction.ExternalSource,
        //     Amount = transaction.Amount,
        //     Description = transaction.Description,
        //     TransactionType = transaction.TransactionType
        // };
        //
        // if (transaction.TransactionType == TransactionEnum.Deposit)
        // {
        //     transactionToValidate.ReceiverUserId = initiator.Id;
        //     transactionToValidate.ReceiverUser = initiator;
        // } else if (transaction.TransactionType == TransactionEnum.Withdraw)
        // {
        //     transactionToValidate.SenderUserId = initiator.Id;
        //     transactionToValidate.SenderUser = initiator;
        // }
        //
        // var summary = transactionGenerator.GenerateTransactionDetails(transactionToValidate);
        // var transactionValidation = await ValidateAndAddTransaction(transactionToValidate, summary, cancellationToken);
        //
        // if (!transactionValidation.IsOk)
        // {
        //     return transactionValidation;
        // }
        
        return ServiceResponse.CreateSuccessResponse();
    }
    
    // Asta e o unealta care ne va ajuta mai tarziu
    public async Task<ServiceResponse> AddTransfer(ServiceTask serviceTask,
        CancellationToken cancellationToken = default)
    {
        // var sender = await repository.GetAsync(new UserSpec(serviceTask.Reply.Request.SenderUserId), cancellationToken);
        //
        // if (sender == null)
        //     return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "User with this id not found!", ErrorCodes.EntityNotFound));
        //
        // var senderAccount = await repository.GetAsync(new AccountUserSpec(serviceTask.Reply.Request.SenderUser.Account.Id), cancellationToken);
        //
        // if (senderAccount == null)
        // {
        //     return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound,
        //         "Sender account not found!", ErrorCodes.EntityNotFound));
        // }
        //
        // var receiver = await repository.GetAsync(new UserSpec(serviceTask.Reply.Request.ReceiverUserId), cancellationToken);
        //
        // if (receiver == null)
        //     return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "User with this id not found!", ErrorCodes.EntityNotFound));
        //
        // if (receiver.Role != UserRoleEnum.Specialist)
        //     return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Receiver should be a specialist!", ErrorCodes.CannotAdd));
        //
        // var receiverAccount = await repository.GetAsync(new AccountUserSpec(serviceTask.Reply.Request.SenderUser.Account.Id), cancellationToken);
        // if (receiverAccount == null)
        // {
        //     return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound,
        //         "Receiver account not found!", ErrorCodes.EntityNotFound));
        // }
        //
        // var transactionToValidate = new Transaction
        // {
        //     InitiatorUserId = sender.Id,
        //     InitiatorUser = sender,
        //     SenderUserId = sender.Id,
        //     SenderUser = sender,
        //     ReceiverUserId = receiver.Id,
        //     ReceiverUser = receiver,
        //     Amount = serviceTask.Price,
        //     Description = serviceTask.Description,
        //     TransactionType = TransactionEnum.Transfer,
        // };
        //
        // var summary = transactionGenerator.GenerateTransferSummary(serviceTask);
        //
        // var transactionValidation = await ValidateAndAddTransaction(transactionToValidate, summary, cancellationToken);
        //
        // if (!transactionValidation.IsOk)
        // {
        //     return transactionValidation;
        // }
        
        return ServiceResponse.CreateSuccessResponse();
    }

    private async Task<ServiceResponse> ValidateAndAddTransaction(Transaction transactionToValidate, string summary,
        CancellationToken cancellationToken = default)
    {
        var validation = RejectReasonMessage.CreateRejectReasonMessage(transactionToValidate);

        if (!validation.IsValid)
        {
            transactionToValidate.Status = StatusEnum.Rejected;
            transactionToValidate.RejectedAt = DateTime.UtcNow;
            transactionToValidate.RejectionCode = validation.Reason;
            transactionToValidate.RejectionDetails = validation.Message;

            summary = transactionGenerator.GenerateInvalidTransactionSummary(transactionToValidate);
            transactionToValidate.Summary = summary;
            
            await repository.AddAsync(transactionToValidate, cancellationToken);
            await mailService.SendMail(transactionToValidate.InitiatorUser.Email, "Transfer invalidated!", 
                MailTemplates.TransactionInvalidTemplate($"{transactionToValidate.InitiatorUser.FullName}", summary), true, "ExpertEase Team", cancellationToken);
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest, validation.Message));
        }

        transactionToValidate.Status = StatusEnum.Pending;
        transactionToValidate.Summary = summary;
        
        await repository.AddAsync(transactionToValidate, cancellationToken);
        await mailService.SendMail(transactionToValidate.InitiatorUser.Email, "Transfer added!", 
            MailTemplates.TransactionAddTemplate($"{transactionToValidate.InitiatorUser.FullName}", transactionToValidate.TransactionType.ToString(), summary), true, "ExpertEase Team", cancellationToken);
        return ServiceResponse.CreateSuccessResponse();
    }

    public async Task<ServiceResponse<TransactionDTO>> GetTransaction(Specification<Transaction, TransactionDTO> spec, Guid id, CancellationToken cancellationToken = default)
    {
        var result = await repository.GetAsync(spec, cancellationToken);
        return result != null ? 
            ServiceResponse.CreateSuccessResponse(result) : 
            ServiceResponse.CreateErrorResponse<TransactionDTO>(new(HttpStatusCode.NotFound, "Transaction not found!", ErrorCodes.EntityNotFound));
    }
    
    public async Task<ServiceResponse<PagedResponse<TransactionDTO>>> GetTransactions(Specification<Transaction, TransactionDTO> spec, PaginationSearchQueryParams pagination, CancellationToken cancellationToken = default)
    {
        var result = await repository.PageAsync(pagination, spec, cancellationToken); // Use the specification and pagination API to get only some entities from the database.

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

        if (requestingUser is { Role: UserRoleEnum.Admin } &&
            transaction.Status == StatusEnum.Cancelled)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden, "Only the user can cancel the transaction!", ErrorCodes.CannotUpdate));
        }
        
        if (transaction is { Status: StatusEnum.Rejected, Description: null })
        {
            return ServiceResponse.CreateErrorResponse(new (HttpStatusCode.BadRequest, "Description cannot be null", ErrorCodes.CannotUpdate));
        }
        
        var transactionToUpdate = await repository.GetAsync(new TransactionSpec(transaction.Id), cancellationToken);
        
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

        return transaction.Status switch
        {
            StatusEnum.Rejected => await RejectTransaction(transactionToUpdate, cancellationToken),
            StatusEnum.Accepted => await AcceptTransaction(transactionToUpdate, cancellationToken),
            _ => ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest, "Transaction status is not valid!", ErrorCodes.CannotUpdate))
        };
    }
    
    private async Task<ServiceResponse> RejectTransaction(Transaction transaction, CancellationToken cancellationToken)
    {
        transaction.Status = StatusEnum.Rejected;
        transaction.RejectedAt = DateTime.UtcNow;
        transaction.RejectionCode = RejectionReason.ManualReview;
        transaction.RejectionDetails = transaction.Description;
            
        var summary = transactionGenerator.GenerateRejectedTransactionSummary(transaction);
        transaction.Summary = summary;
            
        await repository.UpdateAsync(transaction, cancellationToken);
        await mailService.SendMail(transaction.InitiatorUser.Email, "Transfer rejected!", 
            MailTemplates.TransactionProcessedTemplate($"{transaction.InitiatorUser.FullName}", transaction.TransactionType.ToString(), transaction.Status.ToString(), summary), 
             true, "ExpertEase Team", cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse();
    }

    private async Task<ServiceResponse> AcceptTransaction(Transaction transaction, CancellationToken cancellationToken)
    {
        transaction.Status = StatusEnum.Accepted;
        transaction.RejectionCode = null;
        transaction.RejectionDetails = null;
            
        switch (transaction.TransactionType)
        {
            case TransactionEnum.Deposit:
            {
                var accountResult = await HandleDepositAccount(transaction, cancellationToken);
                if (!accountResult.IsOk)
                {
                    return accountResult;
                }
            } 
                break;
            case TransactionEnum.Withdraw:
            {
                var accountResult = await HandleWithdrawalAccount(transaction, cancellationToken);
                if (!accountResult.IsOk)
                {
                    return accountResult;
                }
            }
                break;

            case TransactionEnum.Transfer:
            {
                var accountResult = await HandleTransferAccount(transaction, cancellationToken);
                if (!accountResult.IsOk)
                {
                    return accountResult;
                }
            }
                break;
        }
            
        var summary = transactionGenerator.GenerateAcceptedTransactionSummary(transaction);
        transaction.Summary = summary;
            
        await repository.UpdateAsync(transaction, cancellationToken);
        await mailService.SendMail(transaction.InitiatorUser.Email, "Transfer accepted!", MailTemplates.TransactionProcessedTemplate($"{transaction.InitiatorUser.FullName}", transaction.TransactionType.ToString(), transaction.Status.ToString(), summary), true, "ExpertEase Team", cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse();
    }

    private async Task<ServiceResponse> HandleDepositAccount(Transaction transaction, CancellationToken cancellationToken)
    {
        // if (transaction.ReceiverUserId == null)
        //     return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest,
        //         "You can't deposit without a receiver id!"));
        //             
        // var receiverAccount = await repository.GetAsync(new AccountUserSpec(transaction.ReceiverUserId.Value), 
        //     cancellationToken);
        // if (receiverAccount == null)
        // {
        //     return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.NotFound,
        //         "Receiver's account not found!", ErrorCodes.EntityNotFound));
        // }
        // receiverAccount.Balance += transaction.Amount;
        // await repository.UpdateAsync(receiverAccount, cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }

    private async Task<ServiceResponse> HandleWithdrawalAccount(Transaction transaction, CancellationToken cancellationToken)
    {
        // if (transaction.SenderUserId == null)
        //     return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest,
        //         "You can't withdraw without a sender id!"));
        // var senderAccount = await repository.GetAsync(new AccountUserSpec(transaction.SenderUserId.Value), 
        //     cancellationToken);
        // if (senderAccount == null)
        // {
        //     return ServiceResponse.CreateErrorResponse<AccountDTO>(new(HttpStatusCode.NotFound,
        //         "Sender's account not found!", ErrorCodes.EntityNotFound));
        // }
        // senderAccount.Balance -= transaction.Amount;
        // await repository.UpdateAsync(senderAccount, cancellationToken);
        
        return ServiceResponse.CreateSuccessResponse();
    }

    private async Task<ServiceResponse> HandleTransferAccount(Transaction transaction,
        CancellationToken cancellationToken)
    {
        if (transaction.SenderUserId == null || transaction.ReceiverUserId == null)
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.BadRequest,
                "You can't transfer without a sender or receiver id!"));
        var senderAccount = await repository.GetAsync(new AccountSpec(transaction.SenderUserId.Value),
            cancellationToken);
        var receiverAccount = await repository.GetAsync(new AccountSpec(transaction.ReceiverUserId.Value),
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
        senderAccount.Balance -= transaction.Amount;
        receiverAccount.Balance += transaction.Amount;
        await repository.UpdateAsync(senderAccount, cancellationToken);
        await repository.UpdateAsync(receiverAccount, cancellationToken);
        
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

        var result = await repository.GetAsync(new TransactionSpec(id), cancellationToken);

        if (result == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Transaction not found!", ErrorCodes.EntityNotFound));
        }

        if (result.InitiatorUserId != requestingUser.Id)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                "You are not allowed to cancel this transaction!", ErrorCodes.CannotUpdate));
        }

        if (requestingUser.Role != UserRoleEnum.Admin && result.TransactionType == TransactionEnum.Transfer)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.Forbidden,
                "Only deposits and withdrawals can be cancelled!", ErrorCodes.CannotUpdate));
        }

        result.Status = StatusEnum.Cancelled;
        result.RejectionCode = RejectionReason.UserRequest;
        result.RejectionDetails = "Transaction cancelled by user request";
        
        var summary = transactionGenerator.GenerateCancelledTransactionSummary(result);
        result.Summary = summary;
        
        await repository.UpdateAsync(result, cancellationToken);
         await mailService.SendMail(result.InitiatorUser.Email, "Transfer cancelled!", 
            MailTemplates.TransactionProcessedTemplate($"{result.InitiatorUser.FullName}", result.TransactionType.ToString(), result.Status.ToString(), summary), 
            true, "ExpertEase Team", cancellationToken);
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

        var result = await repository.GetAsync(new TransactionSpec(id), cancellationToken);

        if (result == null)
        {
            return ServiceResponse.CreateErrorResponse(new(HttpStatusCode.NotFound, "Transaction not found!", ErrorCodes.EntityNotFound));
        }

        await repository.DeleteAsync<Transaction>(id, cancellationToken);

        return ServiceResponse.CreateSuccessResponse();
    }
}