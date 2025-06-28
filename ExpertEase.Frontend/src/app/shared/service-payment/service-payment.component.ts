// service-payment.component.ts - Main component structure
import { Component, ElementRef, OnInit, ViewChild, OnDestroy, ChangeDetectorRef, NgZone } from '@angular/core';
import { loadStripe, Stripe, StripeElements, StripeCardNumberElement, StripeCardExpiryElement, StripeCardCvcElement } from '@stripe/stripe-js';
import { firstValueFrom } from 'rxjs';
import {
  PaymentConfirmationDTO,
  PaymentIntentCreateDTO,
  CalculateProtectionFeeRequestDTO,
  CustomerPaymentMethodDto,
  SaveCustomerPaymentMethodDto
} from '../../models/api.models';
import { PaymentService } from '../../services/payment.service';
import { PaymentFlowService } from '../../services/payment-flow.service';
import { CustomerPaymentMethodService } from '../../services/payment_method.service';
import { NotificationService } from '../../services/notification.service';
import { CurrencyPipe, DatePipe, NgIf, NgForOf } from '@angular/common';
import { FormsModule } from '@angular/forms';

interface PaymentMethodData {
  paymentMethodId: string;
  cardLast4: string;
  cardBrand: string;
  cardholderName: string;
}

@Component({
  selector: 'app-service-payment',
  templateUrl: './service-payment.component.html',
  imports: [
    DatePipe,
    CurrencyPipe,
    NgIf,
    NgForOf,
    FormsModule
  ],
  styleUrls: ['./service-payment.component.scss']
})
export class ServicePaymentComponent implements OnInit, OnDestroy {
  @ViewChild('cardNumberElement') cardNumberElement!: ElementRef;
  @ViewChild('cardExpiryElement') cardExpiryElement!: ElementRef;
  @ViewChild('cardCvcElement') cardCvcElement!: ElementRef;

  // Stripe variables
  stripe: Stripe | null = null;
  elements: StripeElements | null = null;
  cardNumber: StripeCardNumberElement | null = null;
  cardExpiry: StripeCardExpiryElement | null = null;
  cardCvc: StripeCardCvcElement | null = null;

  // DOM tracking
  private elementMountRetries = 0;
  private maxMountRetries = 5;
  private isElementsDestroyed = false;
  private isModalOpen = false;
  private mountingInProgress = false;

  // Store payment method data separately
  private storedPaymentMethodData: PaymentMethodData | null = null;

  // Saved payment methods
  savedPaymentMethods: CustomerPaymentMethodDto[] = [];
  selectedSavedCard: CustomerPaymentMethodDto | null = null;
  showSavedCards = false;
  isLoadingSavedCards = false;

  // Modal and form state
  showCardModal = false;
  paymentMethodSelected = false;
  cardDetailsEntered = false;
  isSavingCard = false;
  saveCardForFuture = true;

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

  // Escrow payment amounts
  serviceAmount = 0;
  protectionFee = 0;
  totalAmount = 0;
  isProcessing = false;

  // Protection fee details
  protectionFeeDetails: any = null;
  feeCalculationLoaded = false;

  constructor(
    private readonly paymentService: PaymentService,
    private readonly paymentFlowService: PaymentFlowService,
    private readonly customerPaymentMethodService: CustomerPaymentMethodService,
    private readonly notificationService: NotificationService,
    private readonly cdr: ChangeDetectorRef,
    private readonly ngZone: NgZone
  ) {}

  async ngOnInit() {
    await this.initializeStripe();
    this.loadPaymentFlowData();
    await this.calculateProtectionFee();
    await this.loadSavedPaymentMethods();
  }

  ngOnDestroy() {
    this.isModalOpen = false;
    this.safeDestroyElements();
  }

  // ===== INITIALIZATION =====

  private async initializeStripe() {
    try {
      this.stripe = await loadStripe('pk_test_51RY4TaRP4R8qcMUlWSJwbW6GBjetIiG7jc4fLrsUcl7xMS8uTMcI2mfDHId8YRAku8lllViJiAY0mVPObrvgLYke00QP5RVa7S');
      if (!this.stripe) {
        throw new Error('Failed to load Stripe');
      }
      console.log('✅ Stripe initialized successfully');
    } catch (error) {
      console.error('Error initializing Stripe:', error);
      this.notificationService.showNotification({
        type: 'error',
        message: 'Failed to initialize payment system'
      });
    }
  }

  private loadPaymentFlowData() {
    const paymentFlowState = this.paymentFlowService.getCurrentState();
    this.serviceDetails = paymentFlowState.serviceDetails;
    this.userDetails = paymentFlowState.userDetails;
    this.specialistDetails = paymentFlowState.specialistDetails;
    this.extractServiceAmount();
  }

  private extractServiceAmount() {
    if (!this.serviceDetails) {
      this.serviceAmount = 0;
      return;
    }

    const priceFields = ['price', 'amount', 'cost', 'servicePrice', 'totalPrice'];
    let foundPrice = false;

    for (const field of priceFields) {
      if (this.serviceDetails[field] !== undefined && this.serviceDetails[field] !== null) {
        this.serviceAmount = Number(this.serviceDetails[field]);
        foundPrice = true;
        break;
      }
    }

    if (!foundPrice) {
      this.serviceAmount = 0;
    }

    if (this.serviceAmount < 0) {
      this.serviceAmount = 0;
    }
  }

  // ===== SIMPLE MODAL MANAGEMENT =====

  openCardDetailsModal() {
    console.log('🔓 Opening card details modal...');
    this.showCardModal = true;
    this.isModalOpen = true;
    this.isElementsDestroyed = false;
    this.elementMountRetries = 0;
    this.mountingInProgress = false;

    this.cdr.detectChanges();

    // Use simple timeout approach for DOM readiness
    setTimeout(() => {
      this.setupStripeElementsWithRetry();
    }, 200);
  }

  closeCardModal(event?: Event) {
    if (event) {
      event.preventDefault();
    }

    console.log('🔒 Closing card modal...');
    this.isModalOpen = false;
    this.showCardModal = false;
    this.mountingInProgress = false;
    this.clearErrors();

    if (!this.cardDetailsEntered) {
      this.safeDestroyElements();
    }
  }

  // ===== SIMPLE ELEMENT SETUP WITH RETRY =====

  private async setupStripeElementsWithRetry() {
    if (!this.stripe || this.isElementsDestroyed || !this.isModalOpen) {
      return;
    }

    try {
      // Check if elements are available
      if (!this.cardNumberElement?.nativeElement ||
        !this.cardExpiryElement?.nativeElement ||
        !this.cardCvcElement?.nativeElement) {

        if (this.elementMountRetries < this.maxMountRetries) {
          this.elementMountRetries++;
          console.log(`🔄 Retrying element setup (${this.elementMountRetries}/${this.maxMountRetries})...`);
          setTimeout(() => this.setupStripeElementsWithRetry(), 200);
          return;
        } else {
          throw new Error('DOM elements not available after retries');
        }
      }

      await this.createAndMountElements();

    } catch (error) {
      console.error('❌ Error setting up Stripe elements:', error);
      this.notificationService.showNotification({
        type: 'error',
        message: 'Failed to load payment form. Please try again.'
      });
    }
  }

  private async createAndMountElements() {
    // Destroy existing elements
    await this.safeDestroyElements(false);

    if (!this.isModalOpen) return;

    // Create new elements
    this.elements = this.stripe!.elements();

    const elementStyle = {
      base: {
        fontSize: '16px',
        color: '#424770',
        fontFamily: '"Helvetica Neue", Helvetica, sans-serif',
        '::placeholder': { color: '#aab7c4' }
      },
      invalid: { color: '#9e2146' }
    };

    this.cardNumber = this.elements.create('cardNumber', { style: elementStyle });
    this.cardExpiry = this.elements.create('cardExpiry', { style: elementStyle });
    this.cardCvc = this.elements.create('cardCvc', { style: elementStyle });

    // Mount elements
    this.cardNumber.mount(this.cardNumberElement.nativeElement);
    this.cardExpiry.mount(this.cardExpiryElement.nativeElement);
    this.cardCvc.mount(this.cardCvcElement.nativeElement);

    // Setup event listeners
    this.cardNumber.on('change', (event) => {
      this.cardNumberComplete = event.complete;
      this.cardNumberError = event.error ? event.error.message : null;
      if (event.brand) this.cardBrand = event.brand.toUpperCase();
    });

    this.cardExpiry.on('change', (event) => {
      this.cardExpiryComplete = event.complete;
      this.cardExpiryError = event.error ? event.error.message : null;
    });

    this.cardCvc.on('change', (event) => {
      this.cardCvcComplete = event.complete;
      this.cardCvcError = event.error ? event.error.message : null;
    });

    // Set default name
    if (this.userDetails?.userFullName && !this.cardholderName) {
      this.cardholderName = this.userDetails.userFullName;
      this.validateCardholderName();
    }

    console.log('✅ Stripe elements setup completed');
  }

  private async safeDestroyElements(markAsDestroyed = true) {
    try {
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

      if (markAsDestroyed) {
        this.isElementsDestroyed = true;
      }

    } catch (error) {
      console.error('❌ Error destroying elements:', error);
      this.cardNumber = null;
      this.cardExpiry = null;
      this.cardCvc = null;
      if (markAsDestroyed) this.isElementsDestroyed = true;
    }
  }

  // ===== VALIDATION =====

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

  // ===== GETTERS =====

  get allFieldsComplete(): boolean {
    if (this.selectedSavedCard) return true;
    return this.cardNumberComplete && this.cardExpiryComplete &&
      this.cardCvcComplete && this.cardholderNameValid;
  }

  get hasValidServiceAmount(): boolean {
    return this.serviceAmount > 0;
  }

  get isLoadingFeeCalculation(): boolean {
    return !this.feeCalculationLoaded && this.serviceAmount > 0;
  }

  get protectionFeeDisplayText(): string {
    if (!this.feeCalculationLoaded) return 'Calculating...';
    if (this.protectionFee === 0) return 'No protection fee';
    const percentage = (this.protectionFee / this.serviceAmount) * 100;
    return `${percentage.toFixed(1)}% protection fee`;
  }

  get protectionFeeExplanation(): string {
    if (!this.protectionFeeDetails) return 'Protection fee helps ensure service quality';
    return this.protectionFeeDetails.description || 'Client protection fee for service quality assurance';
  }

  get hasSavedCards(): boolean {
    return this.savedPaymentMethods.length > 0;
  }

  async processPayment() {
    if (!this.cardDetailsEntered || !this.serviceDetails || !this.userDetails || !this.feeCalculationLoaded) {
      this.showError = true;
      this.notificationService.showNotification({
        type: 'error',
        message: 'Please complete all required fields before proceeding.'
      });
      return;
    }

    if (!this.storedPaymentMethodData && !this.selectedSavedCard) {
      this.notificationService.showNotification({
        type: 'error',
        message: 'No payment method available. Please add a card first.'
      });
      return;
    }

    this.isProcessing = true;

    try {
      console.log('🚀 Starting escrow payment process...');

      const paymentIntentResponse = await this.createPaymentIntent();

      if (!paymentIntentResponse.response) {
        throw new Error(paymentIntentResponse.errorMessage?.message || 'Failed to create payment intent');
      }

      const { clientSecret, paymentIntentId } = paymentIntentResponse.response;
      console.log('✅ Escrow payment intent created:', paymentIntentId);

      // Use stored payment method ID instead of DOM elements
      let paymentMethodId: string;

      if (this.selectedSavedCard) {
        paymentMethodId = this.selectedSavedCard.stripePaymentMethodId;
        console.log('💳 Using saved card:', this.selectedSavedCard.cardLast4);
      } else if (this.storedPaymentMethodData) {
        paymentMethodId = this.storedPaymentMethodData.paymentMethodId;
        console.log('💳 Using new card:', this.storedPaymentMethodData.cardLast4);
      } else {
        throw new Error('No payment method available');
      }

      console.log('💳 Confirming payment with payment method ID:', paymentMethodId);

      // Use payment method ID directly - no DOM elements involved
      const { error, paymentIntent } = await this.stripe!.confirmCardPayment(clientSecret, {
        payment_method: paymentMethodId
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
        console.log('✅ Stripe payment succeeded - money now in escrow');
        await this.confirmPaymentWithBackend(paymentIntentId);
        console.log('✅ Escrow payment process completed successfully');
        this.safeDestroyElements();
      } else {
        throw new Error(`Unexpected payment status: ${paymentIntent?.status}`);
      }

    } catch (error) {
      console.error('❌ Payment error:', error);

      let errorMessage = 'Payment failed. Please try again.';
      if (error instanceof Error) {
        if (error.message.includes('network')) {
          errorMessage = 'Network error. Please check your connection and try again.';
        } else if (error.message.includes('declined')) {
          errorMessage = 'Payment was declined. Please check your card details.';
        }
      }

      this.notificationService.showNotification({
        type: 'error',
        message: errorMessage
      });

    } finally {
      this.isProcessing = false;
      this.cdr.detectChanges();
    }
  }

// ===== PAYMENT INTENT CREATION =====

  private async createPaymentIntent() {
    const paymentFlowState = this.paymentFlowService.getCurrentState();

    const paymentIntentDto: PaymentIntentCreateDTO = {
      replyId: paymentFlowState.replyId!,
      serviceAmount: this.serviceAmount,
      protectionFee: this.protectionFee,
      totalAmount: this.totalAmount,
      currency: 'ron',
      description: `Escrow payment: ${this.serviceDetails?.description || 'Professional service'}`,
      metadata: {
        replyId: paymentFlowState.replyId!,
        conversationId: paymentFlowState.conversationId!,
        userId: this.userDetails?.userId || '',
        specialistId: this.specialistDetails?.userId || '',
        paymentType: 'escrow'
      }
    };

    console.log('💳 Creating escrow payment intent:', paymentIntentDto);
    return await firstValueFrom(this.paymentService.createPaymentIntent(paymentIntentDto));
  }

// ===== PAYMENT CONFIRMATION =====

  private async confirmPaymentWithBackend(paymentIntentId: string): Promise<void> {
    const paymentFlowState = this.paymentFlowService.getCurrentState();

    const confirmationDto: PaymentConfirmationDTO = {
      paymentIntentId: paymentIntentId,
      replyId: paymentFlowState.replyId!,
      serviceAmount: this.serviceAmount,
      protectionFee: this.protectionFee,
      totalAmount: this.totalAmount,
      paymentMethod: `${this.cardBrand} •••• ${this.cardLast4}`
    };

    try {
      const confirmResponse = await firstValueFrom(
        this.paymentService.confirmPayment(confirmationDto)
      );

      if (confirmResponse.response !== undefined) {
        this.notificationService.showNotification({
          type: 'success',
          message: 'Payment completed successfully! Money is now securely held in escrow.'
        });

        this.paymentFlowService.completePayment();
        console.log('💳 Escrow payment flow completed - service task will be created');
      } else {
        throw new Error(confirmResponse.errorMessage?.message || 'Failed to confirm payment');
      }
    } catch (error) {
      console.error('❌ Backend confirmation failed:', error);
      throw error;
    }
  }

// ===== PAYMENT CANCELLATION =====

  cancelPayment() {
    console.log('🚫 Payment cancelled by user');
    this.paymentFlowService.cancelPaymentFlow();
    this.notificationService.showNotification({
      type: 'info',
      message: 'Payment cancelled'
    });
  }

  // Saved Cards Management Methods - Add these to your component

// ===== LOAD SAVED PAYMENT METHODS =====

  async loadSavedPaymentMethods(): Promise<void> {
    this.isLoadingSavedCards = true;
    try {
      const response = await firstValueFrom(
        this.customerPaymentMethodService.getPaymentMethods()
      );

      if (response.response) {
        this.savedPaymentMethods = response.response;
        console.log('✅ Loaded saved payment methods:', this.savedPaymentMethods.length);

        // Auto-select default card if available
        const defaultCard = this.savedPaymentMethods.find(card => card.isDefault);
        if (defaultCard) {
          this.selectSavedCard(defaultCard);
        }
      }
    } catch (error) {
      console.error('❌ Error loading saved payment methods:', error);
      // Don't show error notification - just continue without saved cards
    } finally {
      this.isLoadingSavedCards = false;
    }
  }

// ===== SELECT SAVED CARD =====

  selectSavedCard(card: CustomerPaymentMethodDto): void {
    this.selectedSavedCard = card;
    this.cardLast4 = card.cardLast4;
    this.cardBrand = card.cardBrand;
    this.cardholderName = card.cardholderName;
    this.cardDetailsEntered = true;
    this.paymentMethodSelected = true;
    this.showSavedCards = false;
    this.showError = false;

    // Store payment method data for later use
    this.storedPaymentMethodData = {
      paymentMethodId: card.stripePaymentMethodId,
      cardLast4: card.cardLast4,
      cardBrand: card.cardBrand,
      cardholderName: card.cardholderName
    };

    console.log('✅ Selected saved card:', card.cardLast4);
  }

// ===== TOGGLE SAVED CARDS DISPLAY =====

  toggleSavedCards(): void {
    this.showSavedCards = !this.showSavedCards;
  }

// ===== USE NEW CARD =====

  useNewCard(): void {
    this.selectedSavedCard = null;
    this.storedPaymentMethodData = null;
    this.cardDetailsEntered = false;
    this.paymentMethodSelected = false;
    this.showSavedCards = false;
    this.openCardDetailsModal();
  }

// ===== DELETE SAVED CARD =====

  async deleteSavedCard(card: CustomerPaymentMethodDto): Promise<void> {
    try {
      const response = await firstValueFrom(
        this.customerPaymentMethodService.deletePaymentMethod(card.stripePaymentMethodId)
      );

      if (response.response !== undefined) {
        // Remove from local array
        this.savedPaymentMethods = this.savedPaymentMethods.filter(c => c.id !== card.id);

        // If this was the selected card, clear selection
        if (this.selectedSavedCard?.id === card.id) {
          this.selectedSavedCard = null;
          this.storedPaymentMethodData = null;
          this.cardDetailsEntered = false;
          this.paymentMethodSelected = false;
        }

        this.notificationService.showNotification({
          type: 'success',
          message: 'Payment method deleted successfully'
        });
      }
    } catch (error) {
      console.error('❌ Error deleting payment method:', error);
      this.notificationService.showNotification({
        type: 'error',
        message: 'Failed to delete payment method'
      });
    }
  }

// ===== SAVE CARD DETAILS =====

  async saveCardDetails() {
    if (!this.allFieldsComplete) {
      this.validateAllFields();
      return;
    }

    if (!this.stripe || !this.cardNumber || this.isElementsDestroyed || !this.isModalOpen) {
      this.notificationService.showNotification({
        type: 'error',
        message: 'Payment form is not ready. Please close and reopen the modal.'
      });
      return;
    }

    this.isSavingCard = true;

    try {
      console.log('💳 Creating payment method...');

      const { error, paymentMethod } = await this.stripe.createPaymentMethod({
        type: 'card',
        card: this.cardNumber,
        billing_details: {
          name: this.cardholderName,
          email: this.userDetails?.email,
          phone: this.userDetails?.phoneNumber,
        },
      });

      if (error) {
        console.error('❌ Stripe createPaymentMethod error:', error);
        this.cardNumberError = error.message || 'Invalid card details';
        return;
      }

      if (paymentMethod?.card) {
        // Store payment method data immediately
        this.storedPaymentMethodData = {
          paymentMethodId: paymentMethod.id,
          cardLast4: paymentMethod.card.last4 || '',
          cardBrand: paymentMethod.card.brand?.toUpperCase() || '',
          cardholderName: this.cardholderName
        };

        this.cardLast4 = this.storedPaymentMethodData.cardLast4;
        this.cardBrand = this.storedPaymentMethodData.cardBrand;
        this.cardDetailsEntered = true;
        this.paymentMethodSelected = true;
        this.showError = false;

        // Save to backend if requested
        if (this.saveCardForFuture) {
          await this.saveCardToBackend(paymentMethod);
        }

        console.log('✅ Card details saved successfully');
        this.closeCardModal();
      } else {
        throw new Error('Failed to create payment method');
      }

    } catch (error) {
      console.error('❌ Error validating card:', error);

      if (error instanceof Error && (error.message.includes('Element') || error.message.includes('mounted'))) {
        this.notificationService.showNotification({
          type: 'error',
          message: 'Payment form disconnected. Please close and reopen the modal.'
        });
      } else {
        this.cardNumberError = 'Failed to validate card details';
        this.notificationService.showNotification({
          type: 'error',
          message: 'Failed to validate card. Please check your details and try again.'
        });
      }
    } finally {
      this.isSavingCard = false;
    }
  }

// ===== SAVE CARD TO BACKEND =====

  private async saveCardToBackend(paymentMethod: any): Promise<void> {
    try {
      const saveDto: SaveCustomerPaymentMethodDto = {
        paymentMethodId: paymentMethod.id,
        cardLast4: paymentMethod.card.last4,
        cardBrand: paymentMethod.card.brand.toUpperCase(),
        cardholderName: this.cardholderName,
        isDefault: this.savedPaymentMethods.length === 0 // First card becomes default
      };

      const response = await firstValueFrom(
        this.customerPaymentMethodService.savePaymentMethod(saveDto)
      );

      if (response.response) {
        this.savedPaymentMethods.push(response.response);
        console.log('✅ Card saved to backend successfully');

        this.notificationService.showNotification({
          type: 'success',
          message: 'Card saved for future payments'
        });
      }
    } catch (error) {
      console.error('❌ Error saving card to backend:', error);
      // Don't fail the payment flow if saving fails
      this.notificationService.showNotification({
        type: 'warning',
        message: 'Card validated but not saved for future use'
      });
    }
  }

  // Protection Fee Calculation Methods - Add these to your component

// ===== CALCULATE PROTECTION FEE =====

  async calculateProtectionFee() {
    if (this.serviceAmount <= 0) {
      this.protectionFee = 0;
      this.totalAmount = this.serviceAmount;
      this.feeCalculationLoaded = true;
      return;
    }

    try {
      console.log('💰 Calculating protection fee for service amount:', this.serviceAmount);

      const request: CalculateProtectionFeeRequestDTO = {
        serviceAmount: this.serviceAmount
      };

      const response = await firstValueFrom(
        this.paymentService.calculateProtectionFee(request)
      );

      if (response.response) {
        this.protectionFee = response.response.protectionFee;
        this.totalAmount = response.response.totalAmount;
        this.protectionFeeDetails = response.response.feeConfiguration;
        this.feeCalculationLoaded = true;

        console.log('✅ Protection fee calculated:', {
          serviceAmount: this.serviceAmount,
          protectionFee: this.protectionFee,
          totalAmount: this.totalAmount,
          justification: response.response.feeJustification
        });
      } else {
        throw new Error('Failed to calculate protection fee');
      }
    } catch (error) {
      console.error('❌ Error calculating protection fee:', error);

      // Fallback calculation: 10% with min 5, max 100
      this.protectionFee = Math.min(Math.max(this.serviceAmount * 0.1, 5), 100);
      this.totalAmount = this.serviceAmount + this.protectionFee;
      this.feeCalculationLoaded = true;

      this.notificationService.showNotification({
        type: 'warning',
        message: 'Using fallback protection fee calculation.'
      });
    }
  }

// ===== PROTECTION FEE UTILITIES =====

  get protectionFeePercentage(): number {
    if (!this.protectionFeeDetails || this.serviceAmount <= 0) {
      return 0;
    }
    return (this.protectionFee / this.serviceAmount) * 100;
  }
}
