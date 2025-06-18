// service-payment.component.ts
import { Component, ElementRef, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { loadStripe, Stripe, StripeElements, StripeCardNumberElement, StripeCardExpiryElement, StripeCardCvcElement } from '@stripe/stripe-js';
import {firstValueFrom} from 'rxjs';
import {PaymentConfirmationDTO, PaymentIntentCreateDTO} from '../../models/api.models';
import {PaymentService} from '../../services/payment.service';
import {PaymentFlowService} from '../../services/payment-flow.service';
import {NotificationService} from '../../services/notification.service';
import {CurrencyPipe, DatePipe, NgIf} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {ProtectionFeeService} from '../../services/protection-fee-service';
interface ProtectionFeeConfig {
  type: 'percentage' | 'fixed';    // How to calculate the fee
  percentage: number;              // Percentage rate (e.g., 10%)
  fixedAmount: number;             // Fixed amount (e.g., 15 lei)
  minFee: number;                  // Minimum fee (e.g., 5 lei)
  maxFee: number;                  // Maximum fee (e.g., 100 lei)
  enabled: boolean;                // Turn protection fee on/off
}
@Component({
  selector: 'app-service-payment',
  templateUrl: './service-payment.component.html',
  imports: [
    DatePipe,
    CurrencyPipe,
    NgIf,
    FormsModule
  ],
  styleUrls: ['./service-payment.component.scss']
})
export class ServicePaymentComponent implements OnInit, AfterViewInit, OnDestroy {
  @ViewChild('cardNumberElement') cardNumberElement!: ElementRef;
  @ViewChild('cardExpiryElement') cardExpiryElement!: ElementRef;
  @ViewChild('cardCvcElement') cardCvcElement!: ElementRef;

  // Stripe variables
  stripe: Stripe | null = null;
  elements: StripeElements | null = null;
  cardNumber: StripeCardNumberElement | null = null;
  cardExpiry: StripeCardExpiryElement | null = null;
  cardCvc: StripeCardCvcElement | null = null;

  // Modal and form state
  showCardModal = false;
  paymentMethodSelected = false;
  cardDetailsEntered = false;
  isSavingCard = false;

  // Card form data
  cardholderName = '';
  cardLast4 = '';
  cardBrand = '';

  // Validation states
  cardNumberComplete = false;
  cardExpiryComplete = false;
  cardCvcComplete = false;
  cardholderNameValid = false;

  // Error states
  cardNumberError: string | null = null;
  cardExpiryError: string | null = null;
  cardCvcError: string | null = null;
  cardholderNameError: string | null = null;
  showError = false;

  // Payment flow properties
  serviceDetails: any;
  specialistDetails: any;
  userDetails: any;

  // ✅ UPDATED: Dynamic protection fee calculation
  baseServicePrice = 0;
  protectionFeePercentage = 10; // 10% default - can be configured
  protectionFeeFixed = 0; // Fixed fee option
  protectionFeeType: 'percentage' | 'fixed' = 'percentage'; // Can be toggled
  minProtectionFee = 5; // Minimum protection fee
  maxProtectionFee = 100; // Maximum protection fee

  // Calculated values
  protectionFee = 0;
  totalAmount = 0;
  isProcessing = false;

  constructor(
    private readonly paymentService: PaymentService,
    private readonly paymentFlowService: PaymentFlowService,
    private readonly notificationService: NotificationService,
    private readonly protectionFeeService: ProtectionFeeService
  ) {}

  async ngOnInit() {
    await this.initializeStripe();
    this.loadPaymentFlowData();
    this.calculateTotal();
  }

  ngAfterViewInit() {
    // Elements will be created when modal opens
  }

  ngOnDestroy() {
    this.destroyStripeElements();
  }

  private loadPaymentFlowData() {
    const paymentFlowState = this.paymentFlowService.getCurrentState();
    this.serviceDetails = paymentFlowState.serviceDetails;
    this.userDetails = paymentFlowState.userDetails;
    this.specialistDetails = paymentFlowState.specialistDetails;

    // ✅ UPDATED: Extract price from service details
    this.extractServicePrice();
  }

  // ✅ NEW: Extract and validate service price
  private extractServicePrice() {
    if (!this.serviceDetails) {
      console.warn('⚠️ No service details found');
      this.baseServicePrice = 0;
      return;
    }

    // Try different possible price field names
    const priceFields = ['price', 'amount', 'cost', 'servicePrice', 'totalPrice'];
    let foundPrice = false;

    for (const field of priceFields) {
      if (this.serviceDetails[field] !== undefined && this.serviceDetails[field] !== null) {
        this.baseServicePrice = Number(this.serviceDetails[field]);
        foundPrice = true;
        console.log(`✅ Found price in field '${field}':`, this.baseServicePrice);
        break;
      }
    }

    if (!foundPrice) {
      console.warn('⚠️ No price field found in serviceDetails:', this.serviceDetails);
      this.baseServicePrice = 0;
    }

    // Validate price
    if (this.baseServicePrice < 0) {
      console.warn('⚠️ Invalid negative price, setting to 0');
      this.baseServicePrice = 0;
    }
  }

  // ✅ NEW: Calculate protection fee dynamically
  private calculateProtectionFee(): number {
    if (this.baseServicePrice <= 0) {
      return 0;
    }

    let calculatedFee: number;

    if (this.protectionFeeType === 'percentage') {
      // Calculate percentage-based fee
      calculatedFee = (this.baseServicePrice * this.protectionFeePercentage) / 100;

      // Apply min/max limits
      calculatedFee = Math.max(this.minProtectionFee, calculatedFee);
      calculatedFee = Math.min(this.maxProtectionFee, calculatedFee);
    } else {
      // Use fixed fee
      calculatedFee = this.protectionFeeFixed;
    }

    return Math.round(calculatedFee * 100) / 100; // Round to 2 decimal places
  }

  // ✅ UPDATED: Calculate total with dynamic protection fee
  private calculateTotal() {
    // Get service price
    this.extractServicePrice();

    // Calculate protection fee using the service
    const feeBreakdown = this.protectionFeeService.getProtectionFeeBreakdown(this.baseServicePrice);
    this.protectionFee = feeBreakdown.fee;
    this.totalAmount = this.baseServicePrice + this.protectionFee;

    console.log('💰 Payment calculation:', {
      baseServicePrice: this.baseServicePrice,
      protectionFeeBreakdown: feeBreakdown,
      totalAmount: this.totalAmount
    });
  }

  // ✅ NEW: Methods to update protection fee configuration
  updateProtectionFeePercentage(percentage: number) {
    this.protectionFeePercentage = Math.max(0, Math.min(50, percentage)); // 0-50% range
    this.calculateTotal();
  }

  updateProtectionFeeFixed(amount: number) {
    this.protectionFeeFixed = Math.max(0, amount);
    this.calculateTotal();
  }

  setProtectionFeeType(type: 'percentage' | 'fixed') {
    this.protectionFeeType = type;
    this.calculateTotal();
  }

  updateMinMaxProtectionFee(min: number, max: number) {
    this.minProtectionFee = Math.max(0, min);
    this.maxProtectionFee = Math.max(this.minProtectionFee, max);
    this.calculateTotal();
  }

  // ✅ NEW: Getters for template usage
  get protectionFeeFormatted(): string {
    if (this.protectionFeeType === 'percentage') {
      return `${this.protectionFeePercentage}% (${this.protectionFee} lei)`;
    } else {
      return `${this.protectionFee} lei (fixed)`;
    }
  }

  get hasValidPrice(): boolean {
    return this.baseServicePrice > 0;
  }

  // ... rest of your existing methods remain the same ...

  get allFieldsComplete(): boolean {
    return this.cardNumberComplete &&
      this.cardExpiryComplete &&
      this.cardCvcComplete &&
      this.cardholderNameValid;
  }

  private async initializeStripe() {
    try {
      this.stripe = await loadStripe('pk_test_51RY4TaRP4R8qcMUlWSJwbW6GBjetIiG7jc4fLrsUcl7xMS8uTMcI2mfDHId8YRAku8lllViJiAY0mVPObrvgLYke00QP5RVa7S');
      if (!this.stripe) {
        throw new Error('Failed to load Stripe');
      }
    } catch (error) {
      console.error('Error initializing Stripe:', error);
      this.notificationService.showNotification({
        type: 'error',
        message: 'Failed to initialize payment system'
      });
    }
  }

  openCardDetailsModal() {
    this.showCardModal = true;
    setTimeout(() => {
      this.setupStripeElements();
    }, 100);
  }

  closeCardModal(event?: Event) {
    if (event) {
      event.preventDefault();
    }
    this.showCardModal = false;
    this.destroyStripeElements();
    this.clearErrors();
  }

  private setupStripeElements() {
    if (!this.stripe || !this.cardNumberElement) return;

    this.elements = this.stripe.elements();

    const elementStyle = {
      base: {
        fontSize: '16px',
        color: '#424770',
        fontFamily: '"Helvetica Neue", Helvetica, sans-serif',
        '::placeholder': {
          color: '#aab7c4',
        },
      },
      invalid: {
        color: '#9e2146',
      },
    };

    this.cardNumber = this.elements.create('cardNumber', {
      style: elementStyle,
      placeholder: '1234 5678 9012 3456'
    });

    this.cardExpiry = this.elements.create('cardExpiry', {
      style: elementStyle,
      placeholder: 'MM/YY'
    });

    this.cardCvc = this.elements.create('cardCvc', {
      style: elementStyle,
      placeholder: '123'
    });

    this.cardNumber.mount(this.cardNumberElement.nativeElement);
    this.cardExpiry.mount(this.cardExpiryElement.nativeElement);
    this.cardCvc.mount(this.cardCvcElement.nativeElement);

    this.cardNumber.on('change', (event) => {
      this.cardNumberComplete = event.complete;
      this.cardNumberError = event.error ? event.error.message : null;
      if (event.brand) {
        this.cardBrand = event.brand.toUpperCase();
      }
    });

    this.cardExpiry.on('change', (event) => {
      this.cardExpiryComplete = event.complete;
      this.cardExpiryError = event.error ? event.error.message : null;
    });

    this.cardCvc.on('change', (event) => {
      this.cardCvcComplete = event.complete;
      this.cardCvcError = event.error ? event.error.message : null;
    });

    if (this.userDetails?.userFullName && !this.cardholderName) {
      this.cardholderName = this.userDetails.userFullName;
      this.validateCardholderName();
    }
  }

  private destroyStripeElements() {
    if (this.cardNumber) {
      this.cardNumber.destroy();
      this.cardNumber = null;
    }
    if (this.cardExpiry) {
      this.cardExpiry.destroy();
      this.cardExpiry = null;
    }
    if (this.cardCvc) {
      this.cardCvc.destroy();
      this.cardCvc = null;
    }
  }

  onCardholderNameChange() {
    this.validateCardholderName();
  }

  private validateCardholderName() {
    const name = this.cardholderName.trim();
    if (!name) {
      this.cardholderNameError = 'Numele titularului este obligatoriu';
      this.cardholderNameValid = false;
    } else if (name.length < 2) {
      this.cardholderNameError = 'Numele trebuie să aibă cel puțin 2 caractere';
      this.cardholderNameValid = false;
    } else {
      this.cardholderNameError = null;
      this.cardholderNameValid = true;
    }
  }

  async saveCardDetails() {
    if (!this.allFieldsComplete) {
      this.validateAllFields();
      return;
    }

    this.isSavingCard = true;

    try {
      const { error, paymentMethod } = await this.stripe!.createPaymentMethod({
        type: 'card',
        card: this.cardNumber!,
        billing_details: {
          name: this.cardholderName,
        },
      });

      if (error) {
        this.cardNumberError = error.message || 'Invalid card details';
        return;
      }

      this.cardLast4 = paymentMethod.card?.last4 || '';
      this.cardBrand = paymentMethod.card?.brand?.toUpperCase() || '';
      this.cardDetailsEntered = true;
      this.paymentMethodSelected = true;
      this.showError = false;
      this.closeCardModal();

    } catch (error) {
      console.error('Error validating card:', error);
      this.cardNumberError = 'Failed to validate card details';
    } finally {
      this.isSavingCard = false;
    }
  }

  private validateAllFields() {
    this.validateCardholderName();
    if (!this.cardNumberComplete) {
      this.cardNumberError = 'Numărul cardului este incomplet';
    }
    if (!this.cardExpiryComplete) {
      this.cardExpiryError = 'Data de expirare este incompletă';
    }
    if (!this.cardCvcComplete) {
      this.cardCvcError = 'Codul cardului este incomplet';
    }
  }

  private clearErrors() {
    this.cardNumberError = null;
    this.cardExpiryError = null;
    this.cardCvcError = null;
    this.cardholderNameError = null;
  }

  async processPayment() {
    if (!this.cardDetailsEntered || !this.serviceDetails || !this.userDetails) {
      this.showError = true;
      return;
    }

    this.isProcessing = true;

    try {
      console.log('🚀 Starting payment process...');
      const paymentIntentResponse = await this.createPaymentIntent();

      if (!paymentIntentResponse.response) {
        throw new Error(paymentIntentResponse.errorMessage?.message || 'Failed to create payment intent');
      }

      const { clientSecret, paymentIntentId } = paymentIntentResponse.response;
      console.log('✅ Payment intent created:', paymentIntentId);

      const { error, paymentIntent } = await this.stripe!.confirmCardPayment(clientSecret, {
        payment_method: {
          card: this.cardNumber!,
          billing_details: {
            name: this.cardholderName,
            email: this.userDetails?.email,
            phone: this.userDetails?.phoneNumber,
          },
        },
      });

      if (error) {
        console.error('❌ Stripe payment failed:', error);
        this.notificationService.showNotification({
          type: 'error',
          message: error.message || 'Payment failed. Please try again.'
        });
        return;
      }

      if (paymentIntent?.status === 'succeeded') {
        console.log('✅ Stripe payment succeeded');
        await this.confirmPaymentWithBackend(paymentIntentId);
        console.log('✅ Payment process completed successfully');
      }

    } catch (error) {
      console.error('❌ Payment error:', error);
      this.notificationService.showNotification({
        type: 'error',
        message: 'Payment failed. Please try again.'
      });
    } finally {
      this.isProcessing = false;
    }
  }

  private async createPaymentIntent() {
    const paymentFlowState = this.paymentFlowService.getCurrentState();

    const paymentIntentDto: PaymentIntentCreateDTO = {
      replyId: paymentFlowState.replyId!,
      amount: this.totalAmount,
      currency: 'ron',
      description: `Service booking: ${this.serviceDetails?.description || 'Professional service'}`,
      metadata: {
        replyId: paymentFlowState.replyId!,
        conversationId: paymentFlowState.conversationId!,
        userId: this.userDetails?.userId || '',
        specialistId: this.specialistDetails?.userId || '',
        baseServicePrice: this.baseServicePrice.toString(),
        protectionFee: this.protectionFee.toString(),
        protectionFeeType: this.protectionFeeType
      }
    };

    return await firstValueFrom(this.paymentService.createPaymentIntent(paymentIntentDto));
  }

  private async confirmPaymentWithBackend(paymentIntentId: string): Promise<void> {
    const paymentFlowState = this.paymentFlowService.getCurrentState();

    const confirmationDto: PaymentConfirmationDTO = {
      paymentIntentId: paymentIntentId,
      replyId: paymentFlowState.replyId!,
      amount: this.totalAmount,
      paymentMethod: `${this.cardBrand} •••• ${this.cardLast4}`
    };

    try {
      const confirmResponse = await firstValueFrom(
        this.paymentService.confirmPayment(confirmationDto)
      );

      if (confirmResponse.response !== undefined) {
        this.notificationService.showNotification({
          type: 'success',
          message: 'Payment completed successfully!'
        });

        this.paymentFlowService.completePayment();
        console.log('💳 Payment flow completed - service task will be created');
      } else {
        throw new Error(confirmResponse.errorMessage?.message || 'Failed to confirm payment');
      }
    } catch (error) {
      console.error('❌ Backend confirmation failed:', error);
      throw error;
    }
  }

  cancelPayment() {
    console.log('🚫 Payment cancelled by user');
    this.paymentFlowService.cancelPaymentFlow();
    this.notificationService.showNotification({
      type: 'info',
      message: 'Payment cancelled'
    });
  }

  // ✅ NEW: Get protection fee configuration for display
  get protectionFeeConfig() {
    return this.protectionFeeService.getConfig();
  }

// ✅ NEW: Get protection fee breakdown for display
  get protectionFeeBreakdown() {
    return this.protectionFeeService.getProtectionFeeBreakdown(this.baseServicePrice);
  }

// ✅ NEW: Admin methods to update protection fee (if user is admin)
  updateProtectionFeeSettings(config: Partial<ProtectionFeeConfig>) {
    this.protectionFeeService.updateConfig(config);
    this.calculateTotal(); // Recalculate with new settings
  }
}
