using System.Net;
using System.Text.Json.Serialization;
using ExpertEase.Domain.Entities;
using ExpertEase.Domain.Enums;

namespace ExpertEase.Application.Errors;

/// <summary>
/// This is a simple class to transmit the error information to the client.
/// It includes the message, custom error code to identify te specific error and the HTTP status code to be set on the HTTP response.
/// </summary>
public class ErrorMessage
{
    public string Message { get; }
    public ErrorCodes Code { get; }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public HttpStatusCode Status { get; }

    public ErrorMessage(HttpStatusCode status, string message, ErrorCodes code = ErrorCodes.Unknown)
    {
        Message = message;
        Status = status;
        Code = code;
    }

    public static ErrorMessage FromException(ServerException exception) => new(exception.Status, exception.Message);
}

public class RejectReasonMessage
{
    public string Message { get; }
    public RejectionReason Reason { get; }
    public bool IsValid { get; }
    
    public RejectReasonMessage(string message, RejectionReason reason, bool isValid)
    {
        Message = message;
        Reason = reason;
        IsValid = isValid;
    }

    public static RejectReasonMessage CreateRejectReasonMessage(Transaction transaction)
    {
        if (transaction.SenderUser != null && transaction.SenderUser.Id != transaction.SenderUserId)
        {
            return new RejectReasonMessage("Invalid sender", RejectionReason.InvalidSender, false);
        }
        if (transaction.ReceiverUser != null && transaction.ReceiverUser.Id != transaction.ReceiverUserId)
        {
            return new RejectReasonMessage("Invalid recipient", RejectionReason.InvalidRecipient, false);
        }
        if (transaction.Amount <= 0)
        {
            return new RejectReasonMessage("Invalid amount", RejectionReason.InvalidAmount, false);
        }
        if (transaction.TransactionType == TransactionEnum.Deposit && transaction.Amount > 10000)
        {
            return new RejectReasonMessage("Exceeds limit", RejectionReason.ExceedsLimit, false);
        }
        if (transaction.TransactionType == TransactionEnum.Withdraw && transaction.Amount > 5000)
        {
            return new RejectReasonMessage("Exceeds limit", RejectionReason.ExceedsLimit, false);
        }
        if ((transaction.TransactionType == TransactionEnum.Withdraw || transaction.TransactionType == TransactionEnum.Transfer) 
            && transaction.SenderUser.Account.Balance < transaction.Amount)
        {
            return new RejectReasonMessage("Insufficient funds", RejectionReason.InsufficientFunds, false);
        }
        
        return new RejectReasonMessage("Valid transaction", RejectionReason.None, true);
    }
}
