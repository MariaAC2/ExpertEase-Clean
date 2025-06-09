using ExpertEase.Application.Services;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Infrastructure.Services;

public class TransactionSummaryGenerator : ITransactionSummaryGenerator
{
    public string GenerateTransferSummary(ServiceTask serviceTask)
    {
        return $"User {serviceTask.Reply.Request.SenderUser.FullName} has a problem with the following description: {serviceTask.Description}." +
               $"{Environment.NewLine}Specialist {serviceTask.Reply.Request.ReceiverUser.FullName} accepted solving the problem." +
               $"{Environment.NewLine}The service is at address {serviceTask.Address}, from {serviceTask.StartDate:yyyy-MM-dd HH:mm} to {serviceTask.EndDate:yyyy-MM-dd HH:mm} with a price of {serviceTask.Price:C}." +
               $"{Environment.NewLine}User contact information: {serviceTask.Reply.Request.SenderUser.Email}, {serviceTask.Reply.Request.SenderUser.ContactInfo.PhoneNumber}." +
               $"{Environment.NewLine}Specialist contact information: {serviceTask.Reply.Request.ReceiverUser.Email}" +
               (serviceTask.Reply.Request.ReceiverUser.SpecialistProfile != null ? $", {serviceTask.Reply.Request.ReceiverUser.ContactInfo.PhoneNumber}" : "") + ".";
    }

    // public string GenerateTransactionDetails(Transaction transaction)
    // {
    //     return
    //         $"User {transaction.SenderUser.FullName} initiated a transaction with the following details:" +
    //         (transaction.ExternalSource != null ? $" External source {transaction.ExternalSource}," : "") +
    //         (transaction.Description != null ? $" Description {transaction.Description}," : "") +
    //         $" Amount {transaction.Amount}.";
    // }
    //
    // public string GenerateInvalidTransactionSummary(Transaction transaction)
    // {
    //     return
    //         $"{Environment.NewLine}The transaction of type {transaction.TransactionType} was rejected by system." +
    //         $"{Environment.NewLine}Transaction invalidated at {{(transaction.RejectedAt?.ToString(\"yyyy-MM-dd HH:mm\") ?? \"N/A\")}}." +
    //         $"{Environment.NewLine}Rejection code {transaction.RejectionCode} Rejection details {transaction.RejectionDetails}" +
    //         $"{Environment.NewLine}Transaction details:{Environment.NewLine}" +
    //         (transaction.TransactionType == TransactionEnum.Transfer ? transaction.Summary : GenerateTransactionDetails(transaction));
    // }
    //
    // public string GenerateRejectedTransactionSummary(Transaction transaction)
    // {
    //     return
    //         $"{Environment.NewLine}The transaction of type {transaction.TransactionType} was rejected by admin." +
    //         $"{Environment.NewLine}Transaction rejected at {{(transaction.RejectedAt?.ToString(\"yyyy-MM-dd HH:mm\") ?? \"N/A\")}}." +
    //         $"{Environment.NewLine}Rejection code {transaction.RejectionCode} Rejection details {transaction.RejectionDetails}" +
    //         $"{Environment.NewLine}Transaction details:{Environment.NewLine}" +
    //         (transaction.TransactionType == TransactionEnum.Transfer ? transaction.Summary : GenerateTransactionDetails(transaction));
    // }
    //
    // public string GenerateAcceptedTransactionSummary(Transaction transaction)
    // {
    //     return
    //         $"{Environment.NewLine}The transaction of type {transaction.TransactionType} was accepted by admin." +
    //         $"{Environment.NewLine}Transaction accepted at {transaction.UpdatedAt:yyyy-MM-dd HH:mm}." +
    //         $"{Environment.NewLine}Transaction details:{Environment.NewLine}" +
    //         (transaction.TransactionType == TransactionEnum.Transfer ? transaction.Summary : GenerateTransactionDetails(transaction));
    // }
    //
    // public string GenerateCancelledTransactionSummary(Transaction transaction)
    // {
    //     return
    //         $"{Environment.NewLine}The transaction of type {transaction.TransactionType} was cancelled by user." +
    //         $"{Environment.NewLine}Transaction cancelled at {transaction.UpdatedAt:yyyy-MM-dd HH:mm}." +
    //         $"{Environment.NewLine}Rejection code {transaction.RejectionCode} Rejection details {transaction.RejectionDetails}" +
    //         $"{Environment.NewLine}Transaction details:{Environment.NewLine}" +
    //         (transaction.TransactionType == TransactionEnum.Transfer ? transaction.Summary : GenerateTransactionDetails(transaction));
    // }
}