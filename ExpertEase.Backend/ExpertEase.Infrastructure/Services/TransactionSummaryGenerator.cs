using ExpertEase.Application.Services;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Infrastructure.Services;

public class TransactionSummaryGenerator : ITransactionSummaryGenerator
{
    public string GenerateTransferSummary(Request request, Reply reply)
    {
        return $"User {request.SenderUser.FirstName} {request.SenderUser.LastName} has a problem with the following description: {request.Description}." +
               $"{Environment.NewLine}Specialist {request.ReceiverUser.FirstName} {request.ReceiverUser.LastName} accepted solving the problem." +
               $"{Environment.NewLine}The service is at address {request.Address}, from {reply.StartDate:yyyy-MM-dd} to {reply.EndDate:yyyy-MM-dd} with a price of {reply.Price:C}." +
               $"{Environment.NewLine}User contact information: {request.SenderUser.Email}, {request.PhoneNumber}." +
               $"{Environment.NewLine}Specialist contact information: {request.ReceiverUser.Email}" +
               (request.ReceiverUser.Specialist != null ? $", {request.ReceiverUser.Specialist.PhoneNumber}" : "") + ".";
    }

    public string GenerateTransactionDetails(Transaction transaction)
    {
        return
            $"User {transaction.InitiatorUser.FirstName} {transaction.InitiatorUser.LastName} initiated a transaction with the following details:" +
            $"{Environment.NewLine}Id {transaction.Id}" +
            (transaction.ExternalSource != null ? $" External source {transaction.ExternalSource}," : "") +
            (transaction.Description != null ? $" Description {transaction.Description}," : "") +
            $" Amount {transaction.Amount}.";
    }

    public string GenerateInvalidTransactionSummary(Transaction transaction)
    {
        return
            $"{Environment.NewLine}The transaction of type {transaction.TransactionType} was rejected by system." +
            $"{Environment.NewLine}Transaction invalidated at {transaction.RejectedAt.Value:yyyy-MM-dd}." +
            $"{Environment.NewLine}Rejection code {transaction.RejectionCode} Rejection details {transaction.RejectionDetails}" +
            $"{Environment.NewLine}Transaction details:{Environment.NewLine}" +
            (transaction.TransactionType == TransactionEnum.Transfer ? transaction.Summary : GenerateTransactionDetails(transaction));
    }
    
    public string GenerateRejectedTransactionSummary(Transaction transaction)
    {
        return
            $"{Environment.NewLine}The transaction of type {transaction.TransactionType} was rejected by admin." +
            $"{Environment.NewLine}Transaction rejected at {transaction.RejectedAt.Value:yyyy-MM-dd}." +
            $"{Environment.NewLine}Rejection code {transaction.RejectionCode} Rejection details {transaction.RejectionDetails}" +
            $"{Environment.NewLine}Transaction details:{Environment.NewLine}" +
            (transaction.TransactionType == TransactionEnum.Transfer ? transaction.Summary : GenerateTransactionDetails(transaction));
    }
    
    public string GenerateAcceptedTransactionSummary(Transaction transaction)
    {
        return
            $"{Environment.NewLine}The transaction of type {transaction.TransactionType} was accepted by admin." +
            $"{Environment.NewLine}Transaction accepted at {transaction.UpdatedAt:yyyy-MM-dd}." +
            $"{Environment.NewLine}Transaction details:{Environment.NewLine}" +
            (transaction.TransactionType == TransactionEnum.Transfer ? transaction.Summary : GenerateTransactionDetails(transaction));
    }
    
    public string GenerateCancelledTransactionSummary(Transaction transaction)
    {
        return
            $"{Environment.NewLine}The transaction of type {transaction.TransactionType} was cancelled by user." +
            $"{Environment.NewLine}Transaction cancelled at {transaction.UpdatedAt:yyyy-MM-dd}." +
            $"{Environment.NewLine}Rejection code {transaction.RejectionCode} Rejection details {transaction.RejectionDetails}" +
            $"{Environment.NewLine}Transaction details:{Environment.NewLine}" +
            (transaction.TransactionType == TransactionEnum.Transfer ? transaction.Summary : GenerateTransactionDetails(transaction));
    }
}