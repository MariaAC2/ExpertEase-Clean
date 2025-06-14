using ExpertEase.Domain.Enums;

namespace ExpertEase.Infrastructure.Helpers;

public static class PaymentHelper
{
    public static class StripeEvents
    {
        public const string PaymentIntentSucceeded = "payment_intent.succeeded";
        public const string PaymentIntentFailed = "payment_intent.payment_failed";
        public const string PaymentIntentCanceled = "payment_intent.canceled";
        public const string ChargeDisputed = "charge.dispute.created";
        public const string InvoicePaymentSucceeded = "invoice.payment_succeeded";
        public const string InvoicePaymentFailed = "invoice.payment_failed";
    }

    public static class PaymentMethods
    {
        public const string Card = "Card bancar";
        public const string BankTransfer = "Transfer bancar";
        public const string Cash = "Numerar";
    }

    public static class RefundReasons
    {
        public const string RequestedByCustomer = "requested_by_customer";
        public const string Duplicate = "duplicate";
        public const string Fraudulent = "fraudulent";
        public const string ServiceNotProvided = "service_not_provided";
        public const string ServiceUnsatisfactory = "service_unsatisfactory";
    }

    /// <summary>
    /// Validates payment amount
    /// </summary>
    public static bool IsValidAmount(decimal amount)
    {
        return amount > 0 && amount <= 10000; // Max 10,000 RON per transaction
    }

    /// <summary>
    /// Validates currency code
    /// </summary>
    public static bool IsValidCurrency(string currency)
    {
        var supportedCurrencies = new[] { "ron", "eur", "usd" };
        return supportedCurrencies.Contains(currency.ToLower());
    }

    /// <summary>
    /// Gets payment status from Stripe status
    /// </summary>
    public static PaymentStatusEnum GetPaymentStatusFromStripe(string stripeStatus)
    {
        return stripeStatus.ToLower() switch
        {
            "requires_payment_method" => PaymentStatusEnum.Pending,
            "requires_confirmation" => PaymentStatusEnum.Pending,
            "requires_action" => PaymentStatusEnum.Processing,
            "processing" => PaymentStatusEnum.Processing,
            "succeeded" => PaymentStatusEnum.Completed,
            "canceled" => PaymentStatusEnum.Cancelled,
            _ => PaymentStatusEnum.Failed
        };
    }

    /// <summary>
    /// Formats amount for display
    /// </summary>
    public static string FormatAmount(decimal amount, string currency = "RON")
    {
        return currency.ToUpper() switch
        {
            "RON" => $"{amount:N2} lei",
            "EUR" => $"€{amount:N2}",
            "USD" => $"${amount:N2}",
            _ => $"{amount:N2} {currency}"
        };
    }
}