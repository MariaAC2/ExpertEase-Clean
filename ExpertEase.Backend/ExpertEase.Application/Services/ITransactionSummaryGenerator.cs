using ExpertEase.Application.DataTransferObjects.UserDTOs;
using ExpertEase.Domain.Entities;

namespace ExpertEase.Application.Services;

public interface ITransactionSummaryGenerator
{
    string GenerateTransferSummary(ServiceTask serviceTask);
    // string GenerateTransactionDetails(Transaction transaction);
    // string GenerateInvalidTransactionSummary(Transaction transaction);
    // string GenerateAcceptedTransactionSummary(Transaction transaction);
    // string GenerateRejectedTransactionSummary(Transaction transaction);
    // string GenerateCancelledTransactionSummary(Transaction transaction);
}