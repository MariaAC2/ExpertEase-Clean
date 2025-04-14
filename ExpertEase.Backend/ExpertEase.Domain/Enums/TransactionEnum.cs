namespace ExpertEase.Domain.Enums;

public enum TransactionEnum
{
    Initial,
    Deposit,
    Withdraw,
    Transfer
}

public enum RejectionReason
{
    InvalidSender,
    InvalidRecipient,
    InvalidAmount,
    InsufficientFunds,
    ExceedsLimit,
    UserRequest,
    ManualReview,
    None
}