using System.Net;
using ExpertEase.Application.DataTransferObjects;
using ExpertEase.Application.DataTransferObjects.PaymentDTOs;
using ExpertEase.Application.DataTransferObjects.StripeAccountDTOs;
using ExpertEase.Application.Responses;
using ExpertEase.Application.Services;
using ExpertEase.Infrastructure.Configurations;
using Microsoft.Extensions.Options;
using Stripe;

namespace ExpertEase.Infrastructure.Services;

public class StripeAccountService : IStripeAccountService
{
    private readonly StripeSettings _stripeConfiguration;

    public StripeAccountService(IOptions<StripeSettings> stripeConfiguration)
    {
        _stripeConfiguration = stripeConfiguration.Value;
        StripeConfiguration.ApiKey = _stripeConfiguration.SecretKey;
    }

    public async Task<string> CreateConnectedAccount(string email)
    {
        var service = new AccountService();

        var options = new AccountCreateOptions
        {
            Type = "express",
            Country = "RO",
            Email = email,
            Capabilities = new AccountCapabilitiesOptions
            {
                Transfers = new AccountCapabilitiesTransfersOptions { Requested = true },
                CardPayments = new AccountCapabilitiesCardPaymentsOptions { Requested = true }
            }
        };

        var account = await service.CreateAsync(options);
        return account.Id;
    }

    public async Task<ServiceResponse<StripeAccountLinkResponseDTO>> GenerateOnboardingLink(string accountId)
    {
        var linkService = new AccountLinkService();
        var link = await linkService.CreateAsync(new AccountLinkCreateOptions
        {
            Account = accountId,
            ReturnUrl = $"http://localhost:5241/return/{accountId}",
            RefreshUrl = $"http://localhost:5241/return/{accountId}",
            Type = "account_onboarding"
        });
        
        if (link == null || string.IsNullOrEmpty(link.Url))
        {
            return ServiceResponse.CreateErrorResponse<StripeAccountLinkResponseDTO>(
                new(HttpStatusCode.Forbidden ,"Failed to create account link. Please try again later."));
        }

        return ServiceResponse.CreateSuccessResponse(new StripeAccountLinkResponseDTO
        {
            Url = link.Url
        });
    }
    
    public async Task<ServiceResponse<StripeAccountLinkResponseDTO>> GenerateDashboardLink(string accountId)
    {
        var linkService = new AccountLinkService();
        var link = await linkService.CreateAsync(new AccountLinkCreateOptions
        {
            Account = accountId,
            ReturnUrl = $"http://localhost:5241/dashboard/{accountId}",
            RefreshUrl = $"http://localhost:5241/dashboard/{accountId}",
            Type = "account_update"
        });
        
        if (link == null || string.IsNullOrEmpty(link.Url))
        {
            return ServiceResponse.CreateErrorResponse<StripeAccountLinkResponseDTO>(
                new(HttpStatusCode.Forbidden ,"Failed to create dashboard link. Please try again later."));
        }

        return ServiceResponse.CreateSuccessResponse(new StripeAccountLinkResponseDTO
        {
            Url = link.Url
        });
    }

    /// <summary>
    /// ✅ DEPRECATED: Keep for backward compatibility
    /// </summary>
    [Obsolete("Use CreatePaymentIntent(CreatePaymentIntentDTO) instead")]
    public async Task<string> CreatePaymentIntent(decimal amount, string stripeAccountId)
    {
        var dto = new CreatePaymentIntentDTO
        {
            TotalAmount = amount,
            ServiceAmount = amount, // Assume no fee for backward compatibility
            ProtectionFee = 0,
            SpecialistAccountId = stripeAccountId,
            Description = "Plată pentru serviciu ExpertEase"
        };

        var result = await CreatePaymentIntent(dto);
        return result.ClientSecret;
    }

    /// <summary>
    /// ✅ NEW: Enhanced payment intent creation with escrow support
    /// Creates payment intent that holds money on platform account until released
    /// </summary>
    public async Task<PaymentIntentResponseDTO> CreatePaymentIntent(CreatePaymentIntentDTO dto)
    {
        try
        {
            var service = new PaymentIntentService();

            // ✅ CRITICAL: Create payment intent WITHOUT TransferData
            // This keeps money on platform account (escrow)
            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(dto.TotalAmount * 100), // Total amount in cents (service + fee)
                Currency = dto.Currency?.ToLower() ?? "ron",
                PaymentMethodTypes = new List<string> { "card" },

                // ✅ NO TransferData - money stays on platform for escrow
                // TransferData = new PaymentIntentTransferDataOptions
                // {
                //     Destination = dto.SpecialistAccountId // ❌ DON'T do this
                // },

                Description = dto.Description ?? "Plată pentru serviciu ExpertEase",
                Metadata = new Dictionary<string, string>
                {
                    { "platform", "ExpertEase" },
                    { "specialist_account_id", dto.SpecialistAccountId },
                    { "service_amount", (dto.ServiceAmount * 100).ToString() }, // Store for later transfer
                    { "protection_fee", (dto.ProtectionFee * 100).ToString() },
                    { "payment_type", "escrow" },
                    { "created_at", DateTime.UtcNow.ToString("O") }
                }
            };

            Console.WriteLine($"💳 Creating Stripe PaymentIntent:");
            Console.WriteLine($"   - Total Amount: {dto.TotalAmount} RON ({options.Amount} bani)");
            Console.WriteLine($"   - Service Amount: {dto.ServiceAmount} RON (for specialist)");
            Console.WriteLine($"   - Protection Fee: {dto.ProtectionFee} RON (platform revenue)");
            Console.WriteLine($"   - Specialist Account: {dto.SpecialistAccountId}");
            Console.WriteLine($"   - Mode: ESCROW (money held on platform)");

            var paymentIntent = await service.CreateAsync(options);

            // ✅ UPDATED: Return PaymentIntentResponseDTO instead of CreatePaymentIntentResponseDTO
            var response = new PaymentIntentResponseDTO
            {
                ClientSecret = paymentIntent.ClientSecret,
                PaymentIntentId = paymentIntent.Id,
                StripeAccountId = dto.SpecialistAccountId, // ✅ Added this field
                ServiceAmount = dto.ServiceAmount,
                ProtectionFee = dto.ProtectionFee,
                TotalAmount = dto.TotalAmount,
                ProtectionFeeDetails = null // Will be set by caller if needed
            };

            Console.WriteLine($"✅ PaymentIntent created successfully: {paymentIntent.Id}");
            
            return response;
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"❌ Stripe error creating PaymentIntent: {ex.Message}");
            throw new InvalidOperationException($"Stripe payment intent creation failed: {ex.Message}", ex);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ General error creating PaymentIntent: {ex.Message}");
            throw;
        }
    }

    /// <summary>
    /// ✅ NEW: Transfer money to specialist when service is completed
    /// </summary>
    public async Task<ServiceResponse<string>> TransferToSpecialist(
        string paymentIntentId, 
        string specialistAccountId, 
        decimal amount, 
        string reason = "Service completed")
    {
        try
        {
            // 🆕 Check if we're in test mode and ensure sufficient funds
            var isTestMode = _stripeConfiguration.SecretKey.StartsWith("sk_test_");
            
            if (isTestMode)
            {
                Console.WriteLine($"🧪 Test mode detected, ensuring sufficient funds...");
                var fundsResult = await EnsureSufficientFunds(amount);
                if (!fundsResult.IsSuccess)
                {
                    return ServiceResponse.CreateErrorResponse<string>(fundsResult.Error);
                }
                
                // Wait a moment for funds to be available
                await Task.Delay(1000);
            }

            Console.WriteLine($"🔄 Transferring {amount} RON to specialist {specialistAccountId}");
            Console.WriteLine($"📝 Reason: {reason}");

            var transferService = new TransferService();
            
            var transferOptions = new TransferCreateOptions
            {
                Amount = (long)(amount * 100), // Amount in cents
                Currency = "ron",
                Destination = specialistAccountId,
                Description = reason,
                Metadata = new Dictionary<string, string>
                {
                    { "payment_intent_id", paymentIntentId },
                    { "transfer_reason", reason },
                    { "platform", "ExpertEase" },
                    { "transfer_type", "service_completion" },
                    { "test_mode", isTestMode.ToString() },
                    { "created_at", DateTime.UtcNow.ToString("O") }
                }
            };

            var transfer = await transferService.CreateAsync(transferOptions);
            
            Console.WriteLine($"✅ Transfer successful: {transfer.Id}");
            Console.WriteLine($"💰 Amount: {amount} RON transferred to {specialistAccountId}");
            
            return ServiceResponse.CreateSuccessResponse(transfer.Id);
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"❌ Stripe transfer failed: {ex.Message}");
            
            // 🆕 If insufficient funds error, try to create funds and retry once
            if (ex.Message.Contains("insufficient available funds") && _stripeConfiguration.SecretKey.StartsWith("sk_test_"))
            {
                Console.WriteLine($"🔄 Insufficient funds detected, creating test funds and retrying...");
                
                var fundsResult = await EnsureSufficientFunds(amount);
                if (fundsResult.IsSuccess)
                {
                    await Task.Delay(2000); // Wait for funds to be available
                    
                    // Retry the transfer once
                    try
                    {
                        var transferService = new TransferService();
                        var transferOptions = new TransferCreateOptions
                        {
                            Amount = (long)(amount * 100),
                            Currency = "ron",
                            Destination = specialistAccountId,
                            Description = reason + " (retry after funding)",
                            Metadata = new Dictionary<string, string>
                            {
                                { "payment_intent_id", paymentIntentId },
                                { "transfer_reason", reason },
                                { "platform", "ExpertEase" },
                                { "transfer_type", "service_completion_retry" },
                                { "created_at", DateTime.UtcNow.ToString("O") }
                            }
                        };

                        var transfer = await transferService.CreateAsync(transferOptions);
                        Console.WriteLine($"✅ Transfer successful on retry: {transfer.Id}");
                        return ServiceResponse.CreateSuccessResponse(transfer.Id);
                    }
                    catch (StripeException retryEx)
                    {
                        Console.WriteLine($"❌ Transfer failed even after funding: {retryEx.Message}");
                        return ServiceResponse.CreateErrorResponse<string>(
                            new(HttpStatusCode.BadRequest, $"Transfer failed after retry: {retryEx.Message}"));
                    }
                }
            }
            
            return ServiceResponse.CreateErrorResponse<string>(
                new(HttpStatusCode.BadRequest, $"Transfer failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Transfer error: {ex.Message}");
            return ServiceResponse.CreateErrorResponse<string>(
                new(HttpStatusCode.InternalServerError, $"Transfer error: {ex.Message}"));
        }
    }

    /// <summary>
    /// ✅ NEW: Refund money to client if service fails or is cancelled
    /// </summary>
    public async Task<ServiceResponse<string>> RefundPayment(
        string paymentIntentId, 
        decimal refundAmount, 
        string reason = "Service cancelled")
    {
        try
        {
            Console.WriteLine($"💸 Processing refund: {refundAmount} RON");
            Console.WriteLine($"📝 Reason: {reason}");

            var refundService = new RefundService();
            
            var refundOptions = new RefundCreateOptions
            {
                PaymentIntent = paymentIntentId,
                Amount = (long)(refundAmount * 100),
                Reason = "requested_by_customer",
                Metadata = new Dictionary<string, string>
                {
                    { "refund_reason", reason },
                    { "platform", "ExpertEase" },
                    { "refund_type", "service_cancellation" },
                    { "created_at", DateTime.UtcNow.ToString("O") }
                }
            };

            var refund = await refundService.CreateAsync(refundOptions);
            
            Console.WriteLine($"✅ Refund successful: {refund.Id}");
            Console.WriteLine($"💰 Amount: {refundAmount} RON refunded");
            
            return ServiceResponse.CreateSuccessResponse(refund.Id);
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"❌ Stripe refund failed: {ex.Message}");
            return ServiceResponse.CreateErrorResponse<string>(
                new(HttpStatusCode.BadRequest, $"Refund failed: {ex.Message}"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ Refund error: {ex.Message}");
            return ServiceResponse.CreateErrorResponse<string>(
                new(HttpStatusCode.InternalServerError, $"Refund error: {ex.Message}"));
        }
    }

    /// <summary>
    /// ✅ UPDATED: Enhanced account status with test mode detection
    /// </summary>
    public async Task<ServiceResponse<StripeAccountStatusDTO>> GetAccountStatus(string accountId)
    {
        try
        {
            var service = new AccountService();
            var account = await service.GetAsync(accountId);
        
            if (account == null)
            {
                return ServiceResponse.CreateErrorResponse<StripeAccountStatusDTO>(
                    new(HttpStatusCode.NotFound, "Stripe account not found"));
            }

            // ✅ Detect test mode
            var isTestMode = _stripeConfiguration.SecretKey.StartsWith("sk_test_");
            
            // ✅ For test accounts, consider enabled accounts as "complete" for payment purposes
            var isReadyForPayments = account.ChargesEnabled && 
                (isTestMode || (account.PayoutsEnabled && account.DetailsSubmitted));

            var status = new StripeAccountStatusDTO
            {
                AccountId = account.Id,
                IsActive = isReadyForPayments,
                ChargesEnabled = account.ChargesEnabled,
                PayoutsEnabled = account.PayoutsEnabled,
                DetailsSubmitted = account.DetailsSubmitted,
                RequirementsCurrentlyDue = account.Requirements?.CurrentlyDue?.ToList() ?? new List<string>(),
                RequirementsEventuallyDue = account.Requirements?.EventuallyDue?.ToList() ?? new List<string>(),
                DisabledReason = account.Requirements?.DisabledReason,
                IsTestMode = isTestMode,
                CanReceivePayments = account.ChargesEnabled
            };

            return ServiceResponse.CreateSuccessResponse(status);
        }
        catch (Exception ex)
        {
            return ServiceResponse.CreateErrorResponse<StripeAccountStatusDTO>(
                new(HttpStatusCode.InternalServerError, $"Error checking account status: {ex.Message}"));
        }
    }

    /// <summary>
    /// ✅ NEW: Extract PaymentIntent ID from client secret
    /// </summary>
    public static string ExtractPaymentIntentIdFromClientSecret(string clientSecret)
    {
        if (string.IsNullOrWhiteSpace(clientSecret))
            throw new ArgumentException("Client secret cannot be null or empty", nameof(clientSecret));

        // Client secret format: "pi_1234567890_secret_abcdef"
        // We want: "pi_1234567890"
        var parts = clientSecret.Split('_');
        
        if (parts.Length < 2 || !parts[0].StartsWith("pi"))
            throw new ArgumentException("Invalid client secret format", nameof(clientSecret));

        return $"{parts[0]}_{parts[1]}";
    }
    
    public async Task<ServiceResponse<string>> CreateCustomer(string email, string fullName, Guid userId)
    {
        try
        {
            Console.WriteLine($"👤 Creating Stripe customer for: {email}");
        
            var customerService = new CustomerService();
        
            var customerOptions = new CustomerCreateOptions
            {
                Email = email,
                Name = fullName,
                Description = $"ExpertEase platform user: {fullName}",
                Metadata = new Dictionary<string, string>
                {
                    { "user_id", userId.ToString() },
                    { "platform", "ExpertEase" },
                    { "created_at", DateTime.UtcNow.ToString("O") }
                }
            };

            var customer = await customerService.CreateAsync(customerOptions);
        
            Console.WriteLine($"✅ Stripe customer created successfully: {customer.Id}");
            Console.WriteLine($"📧 Email: {customer.Email}");
            Console.WriteLine($"👤 Name: {customer.Name}");
        
            return ServiceResponse.CreateSuccessResponse(customer.Id);
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"❌ Stripe error creating customer: {ex.Message}");
            return ServiceResponse.CreateErrorResponse<string>(
                new(HttpStatusCode.BadRequest, $"Failed to create Stripe customer: {ex.Message}"));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"❌ General error creating customer: {ex.Message}");
            return ServiceResponse.CreateErrorResponse<string>(
                new(HttpStatusCode.InternalServerError, $"Error creating customer: {ex.Message}"));
        }
    }
    
    // Add this method to your StripeAccountService for testing
    public async Task<ServiceResponse<string>> CreateTestFunds(decimal amount)
    {
        try
        {
            Console.WriteLine($"💰 Creating test funds: {amount} RON");
            
            var paymentMethodService = new PaymentMethodService();
            var paymentIntentService = new PaymentIntentService();
            
            // Create a payment method with the special test card
            var paymentMethod = await paymentMethodService.CreateAsync(new PaymentMethodCreateOptions
            {
                Type = "card",
                Card = new PaymentMethodCardOptions
                {
                    Number = "4000000000000077", // Special card that adds to available balance
                    ExpMonth = 12,
                    ExpYear = DateTime.Now.Year + 1,
                    Cvc = "123"
                }
            });
            
            // Create and confirm a payment intent
            var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
            {
                Amount = (long)(amount * 100), // Convert to cents
                Currency = "ron",
                PaymentMethod = paymentMethod.Id,
                Description = "Test funds for platform transfers",
                ConfirmationMethod = "manual",
                Confirm = true,
                Metadata = new Dictionary<string, string>
                {
                    { "purpose", "test_funds" },
                    { "platform", "ExpertEase" }
                }
            });
            
            Console.WriteLine($"✅ Test funds created: {paymentIntent.Id}");
            Console.WriteLine($"💰 Amount: {amount} RON added to platform balance");
            
            return ServiceResponse.CreateSuccessResponse(paymentIntent.Id);
        }
        catch (StripeException ex)
        {
            Console.WriteLine($"❌ Error creating test funds: {ex.Message}");
            return ServiceResponse.CreateErrorResponse<string>(
                new(HttpStatusCode.BadRequest, $"Test funds creation failed: {ex.Message}"));
        }
    }

    // Call this before doing transfers in test mode
    public async Task<ServiceResponse> EnsureSufficientFunds(decimal transferAmount)
    {
        try
        {
            // In test mode, create funds that are 2x the transfer amount to be safe
            var fundsNeeded = transferAmount * 2;
            
            Console.WriteLine($"🧪 Test mode: Ensuring sufficient funds for transfer of {transferAmount} RON");
            var result = await CreateTestFunds(fundsNeeded);
            
            if (result.IsSuccess)
            {
                Console.WriteLine($"✅ Test funds created, ready for transfer");
                return ServiceResponse.CreateSuccessResponse();
            }
            else
            {
                return ServiceResponse.CreateErrorResponse(result.Error);
            }
        }
        catch (Exception ex)
        {
            return ServiceResponse.CreateErrorResponse(new(
                HttpStatusCode.InternalServerError, 
                $"Error ensuring funds: {ex.Message}"));
        }
    }
}