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
    InvalidReceiver,
    InvalidAmount,
    InsufficientFunds,
    ExceedsLimit,
    UserRequest,
    ManualReview,
    None
}