using ExpertEase.Domain.Entities;

namespace ExpertEase.Application.Services;

public interface ITransactionSummaryGenerator
{
    string GenerateTransferSummary(Request request, Reply reply);
    string GenerateTransactionDetails(Transaction transaction);
    string GenerateInvalidTransactionSummary(Transaction transaction);
    string GenerateAcceptedTransactionSummary(Transaction transaction);
    string GenerateRejectedTransactionSummary(Transaction transaction);
    string GenerateCancelledTransactionSummary(Transaction transaction);
}