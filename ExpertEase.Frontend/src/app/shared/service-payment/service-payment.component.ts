// Updated service-payment.component.ts with robust Stripe element handling
import { Component, ElementRef, OnInit, ViewChild, AfterViewInit, OnDestroy } from '@angular/core';
import { loadStripe, Stripe, StripeElements, StripeCardNumberElement, StripeCardExpiryElement, StripeCardCvcElement } from '@stripe/stripe-js';
import { firstValueFrom } from 'rxjs';
import {
  PaymentConfirmationDTO,
  PaymentIntentCreateDTO,
  CalculateProtectionFeeRequestDTO
} from '../../models/api.models';
import { PaymentService } from '../../services/payment.service';
import { PaymentFlowService } from '../../services/payment-flow.service';
import { NotificationService } from '../../services/notification.service';
import { CurrencyPipe, DatePipe, NgIf } from '@angular/common';
import { FormsModule } from '@angular/forms';

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

  // ✅ NEW: Element state tracking
  isElementsMounted = false;
  private elementMountRetries = 0;
  private maxRetries = 3;

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

  // ✅ NEW: Escrow payment amounts
  serviceAmount = 0;           // Amount that goes to specialist
  protectionFee = 0;          // Platform protection fee
  totalAmount = 0;            // Total amount charged to client
  isProcessing = false;

  // ✅ NEW: Protection fee details for transparency
  protectionFeeDetails: any = null;
  feeCalculationLoaded = false;

  constructor(
    private readonly paymentService: PaymentService,
    private readonly paymentFlowService: PaymentFlowService,
    private readonly notificationService: NotificationService
  ) {}

  async ngOnInit() {
    await this.initializeStripe();
    this.loadPaymentFlowData();
    await this.calculateProtectionFee();
  }

  ngAfterViewInit() {
    // Elements will be created when modal opens
  }

  ngOnDestroy() {
    this.safeDestroyElements();
  }

  private loadPaymentFlowData() {
    const paymentFlowState = this.paymentFlowService.getCurrentState();
    this.serviceDetails = paymentFlowState.serviceDetails;
    this.userDetails = paymentFlowState.userDetails;
    this.specialistDetails = paymentFlowState.specialistDetails;

    // Extract service amount from service details
    this.extractServiceAmount();
  }

  // ✅ UPDATED: Enhanced recreate method
  recreateStripeElements() {
    console.log('🔄 Recreating Stripe elements...');
    this.cardDetailsEntered = false;
    this.paymentMethodSelected = false;
    this.isElementsMounted = false;
    this.clearErrors();

    this.safeDestroyElements().then(() => {
      if (this.showCardModal) {
        setTimeout(() => {
          this.setupStripeElements();
        }, 200);
      }
    });
  }

  // ✅ NEW: Extract service amount from service details
  private extractServiceAmount() {
    if (!this.serviceDetails) {
      console.warn('⚠️ No service details found');
      this.serviceAmount = 0;
      return;
    }

    // Try different possible price field names
    const priceFields = ['price', 'amount', 'cost', 'servicePrice', 'totalPrice'];
    let foundPrice = false;

    for (const field of priceFields) {
      if (this.serviceDetails[field] !== undefined && this.serviceDetails[field] !== null) {
        this.serviceAmount = Number(this.serviceDetails[field]);
        foundPrice = true;
        console.log(`✅ Found service amount in field '${field}':`, this.serviceAmount);
        break;
      }
    }

    if (!foundPrice) {
      console.warn('⚠️ No price field found in serviceDetails:', this.serviceDetails);
      this.serviceAmount = 0;
    }

    // Validate amount
    if (this.serviceAmount < 0) {
      console.warn('⚠️ Invalid negative price, setting to 0');
      this.serviceAmount = 0;
    }
  }

  // ✅ NEW: Calculate protection fee using backend
  private async calculateProtectionFee() {
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

  // ✅ NEW: Get protection fee explanation
  get protectionFeeExplanation(): string {
    if (!this.protectionFeeDetails) {
      return 'Protection fee helps ensure service quality';
    }

    return this.protectionFeeDetails.description || 'Client protection fee for service quality assurance';
  }

  // ✅ NEW: Get fee percentage for display
  get protectionFeePercentage(): number {
    if (!this.protectionFeeDetails || this.serviceAmount <= 0) {
      return 0;
    }

    return (this.protectionFee / this.serviceAmount) * 100;
  }

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
      console.log('✅ Stripe initialized successfully');
    } catch (error) {
      console.error('Error initializing Stripe:', error);
      this.notificationService.showNotification({
        type: 'error',
        message: 'Failed to initialize payment system'
      });
    }
  }

  // ✅ UPDATED: Enhanced modal opening
  openCardDetailsModal() {
    this.showCardModal = true;

    // ✅ Use Angular's change detection to ensure DOM is ready
    setTimeout(() => {
      this.setupStripeElements();
    }, 150); // Slightly longer delay to ensure DOM is fully rendered
  }

  // ✅ UPDATED: Don't destroy elements when closing modal
  closeCardModal(event?: Event) {
    if (event) {
      event.preventDefault();
    }
    this.showCardModal = false;
    // ✅ Only destroy if card details weren't entered
    this.conditionallyDestroyElements();
    this.clearErrors();
  }

  // ✅ NEW: Conditional element destruction
  private conditionallyDestroyElements() {
    if (!this.cardDetailsEntered) {
      this.safeDestroyElements();
    }
  }

  // ✅ UPDATED: Robust element setup with retry logic
  private async setupStripeElements(): Promise<void> {
    if (!this.stripe) {
      console.error('❌ Stripe not initialized');
      return;
    }

    // ✅ Ensure DOM elements exist
    if (!this.cardNumberElement?.nativeElement ||
      !this.cardExpiryElement?.nativeElement ||
      !this.cardCvcElement?.nativeElement) {
      console.error('❌ Card DOM elements not available');

      if (this.elementMountRetries < this.maxRetries) {
        this.elementMountRetries++;
        console.log(`🔄 Retrying element setup (attempt ${this.elementMountRetries})`);
        setTimeout(() => this.setupStripeElements(), 200);
        return;
      } else {
        this.notificationService.showNotification({
          type: 'error',
          message: 'Failed to load payment form. Please refresh the page.'
        });
        return;
      }
    }

    try {
      // ✅ Clean up existing elements first
      await this.safeDestroyElements();

      // ✅ Create new elements instance
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

      // ✅ Create elements
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

      // ✅ Mount elements with error handling
      await this.mountElementSafely(this.cardNumber, this.cardNumberElement.nativeElement, 'cardNumber');
      await this.mountElementSafely(this.cardExpiry, this.cardExpiryElement.nativeElement, 'cardExpiry');
      await this.mountElementSafely(this.cardCvc, this.cardCvcElement.nativeElement, 'cardCvc');

      // ✅ Setup event listeners
      this.setupElementEventListeners();

      // ✅ Set default cardholder name
      if (this.userDetails?.userFullName && !this.cardholderName) {
        this.cardholderName = this.userDetails.userFullName;
        this.validateCardholderName();
      }

      this.isElementsMounted = true;
      this.elementMountRetries = 0;
      console.log('✅ Stripe elements mounted successfully');

    } catch (error) {
      console.error('❌ Error setting up Stripe elements:', error);
      this.isElementsMounted = false;
      this.notificationService.showNotification({
        type: 'error',
        message: 'Failed to load payment form. Please try again.'
      });
    }
  }

  // ✅ NEW: Safe element mounting with verification
  private async mountElementSafely(element: any, domElement: HTMLElement, elementName: string): Promise<void> {
    try {
      // Check if DOM element is still in the document
      if (!document.contains(domElement)) {
        throw new Error(`DOM element for ${elementName} is not in document`);
      }

      element.mount(domElement);

      // Wait a bit and verify mount was successful
      await new Promise(resolve => setTimeout(resolve, 100));

      console.log(`✅ ${elementName} mounted successfully`);
    } catch (error) {
      console.error(`❌ Failed to mount ${elementName}:`, error);
      throw error;
    }
  }

  // ✅ NEW: Setup event listeners separately
  private setupElementEventListeners(): void {
    if (!this.cardNumber || !this.cardExpiry || !this.cardCvc) return;

    this.cardNumber.on('change', (event) => {
      this.cardNumberComplete = event.complete;
      this.cardNumberError = event.error ? event.error.message : null;
      if (event.brand) {
        this.cardBrand = event.brand.toUpperCase();
      }
    });

    this.cardNumber.on('ready', () => {
      console.log('✅ Card number element ready');
    });

    this.cardExpiry.on('change', (event) => {
      this.cardExpiryComplete = event.complete;
      this.cardExpiryError = event.error ? event.error.message : null;
    });

    this.cardExpiry.on('ready', () => {
      console.log('✅ Card expiry element ready');
    });

    this.cardCvc.on('change', (event) => {
      this.cardCvcComplete = event.complete;
      this.cardCvcError = event.error ? event.error.message : null;
    });

    this.cardCvc.on('ready', () => {
      console.log('✅ Card CVC element ready');
    });
  }

  // ✅ UPDATED: Safe element destruction
  private async safeDestroyElements(): Promise<void> {
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
      this.isElementsMounted = false;
      console.log('✅ Stripe elements destroyed safely');
    } catch (error) {
      console.error('❌ Error destroying elements:', error);
      // Continue anyway
      this.cardNumber = null;
      this.cardExpiry = null;
      this.cardCvc = null;
      this.isElementsMounted = false;
    }
  }

  // ✅ NEW: Verify elements are ready for use
  private verifyElementsReady(): boolean {
    // Check Stripe elements exist
    const elementsExist = !!(this.stripe && this.cardNumber && this.cardExpiry && this.cardCvc);

    // Check ViewChild elements exist
    const viewChildElements = [
      this.cardNumberElement?.nativeElement,
      this.cardExpiryElement?.nativeElement,
      this.cardCvcElement?.nativeElement
    ];

    const domElementsExist = viewChildElements.every(el => !!el);

    // Only check document.contains if all elements exist
    const domElementsInDocument = domElementsExist &&
      viewChildElements.every(el => document.contains(el!));

    console.log('Element verification:', {
      elementsExist,
      domElementsExist,
      domElementsInDocument,
      isElementsMounted: this.isElementsMounted
    });

    return elementsExist && domElementsExist && domElementsInDocument && this.isElementsMounted;
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

  // ✅ UPDATED: Enhanced save card details with element verification
  async saveCardDetails() {
    if (!this.allFieldsComplete) {
      this.validateAllFields();
      return;
    }

    // ✅ Verify elements are still mounted and functional
    if (!this.verifyElementsReady()) {
      console.log('🔄 Elements not ready, attempting to remount...');
      await this.setupStripeElements();

      // Wait a bit for remounting
      await new Promise(resolve => setTimeout(resolve, 300));

      if (!this.verifyElementsReady()) {
        this.notificationService.showNotification({
          type: 'error',
          message: 'Payment form not ready. Please close and reopen the card form.'
        });
        return;
      }
    }

    this.isSavingCard = true;

    try {
      const { error, paymentMethod } = await this.stripe!.createPaymentMethod({
        type: 'card',
        card: this.cardNumber!,
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
        this.cardLast4 = paymentMethod.card.last4 || '';
        this.cardBrand = paymentMethod.card.brand?.toUpperCase() || '';
        this.cardDetailsEntered = true;
        this.paymentMethodSelected = true;
        this.showError = false;

        console.log('✅ Card details saved successfully');
        this.closeCardModal();
      } else {
        throw new Error('Failed to create payment method');
      }

    } catch (error) {
      console.error('❌ Error validating card:', error);

      // Check if it's a mounting issue
      if ((error as Error)?.message?.includes('Element') || (error as Error)?.message?.includes('mounted')) {
        this.notificationService.showNotification({
          type: 'error',
          message: 'Card form disconnected. Please close and reopen the card form.'
        });
        this.recreateStripeElements();
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

  // ✅ UPDATED: Enhanced payment processing with element verification
  async processPayment() {
    if (!this.cardDetailsEntered || !this.serviceDetails || !this.userDetails || !this.feeCalculationLoaded) {
      this.showError = true;
      this.notificationService.showNotification({
        type: 'error',
        message: 'Please complete all required fields before proceeding.'
      });
      return;
    }

    // ✅ Critical verification before payment
    if (!this.verifyElementsReady()) {
      console.error('❌ Stripe elements not ready for payment');
      this.notificationService.showNotification({
        type: 'error',
        message: 'Payment form not ready. Please reopen the card form and try again.'
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

      console.log('💳 Confirming payment with Stripe...');
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

        // Handle element-specific errors
        if (error.message?.includes('Element') || error.message?.includes('mounted')) {
          this.notificationService.showNotification({
            type: 'error',
            message: 'Payment form disconnected. Please refresh the page and try again.'
          });
        } else {
          this.notificationService.showNotification({
            type: 'error',
            message: error.message || 'Payment failed. Please try again.'
          });
        }
        return;
      }

      if (paymentIntent?.status === 'succeeded') {
        console.log('✅ Stripe payment succeeded - money now in escrow');
        await this.confirmPaymentWithBackend(paymentIntentId);
        console.log('✅ Escrow payment process completed successfully');

        // Now safe to destroy elements
        await this.safeDestroyElements();
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

  // ✅ UPDATED: Create payment intent with new escrow structure
  private async createPaymentIntent() {
    const paymentFlowState = this.paymentFlowService.getCurrentState();

    const paymentIntentDto: PaymentIntentCreateDTO = {
      replyId: paymentFlowState.replyId!,
      serviceAmount: this.serviceAmount,     // ✅ NEW: Separate service amount
      protectionFee: this.protectionFee,    // ✅ NEW: Separate protection fee
      totalAmount: this.totalAmount,        // ✅ NEW: Total amount
      currency: 'ron',
      description: `Escrow payment: ${this.serviceDetails?.description || 'Professional service'}`,
      metadata: {
        replyId: paymentFlowState.replyId!,
        conversationId: paymentFlowState.conversationId!,
        userId: this.userDetails?.userId || '',
        specialistId: this.specialistDetails?.userId || '',
        paymentType: 'escrow'                // ✅ NEW: Mark as escrow payment
      }
    };

    console.log('💳 Creating escrow payment intent:', paymentIntentDto);
    return await firstValueFrom(this.paymentService.createPaymentIntent(paymentIntentDto));
  }

  // ✅ UPDATED: Confirm payment with new escrow structure
  private async confirmPaymentWithBackend(paymentIntentId: string): Promise<void> {
    const paymentFlowState = this.paymentFlowService.getCurrentState();

    const confirmationDto: PaymentConfirmationDTO = {
      paymentIntentId: paymentIntentId,
      replyId: paymentFlowState.replyId!,
      serviceAmount: this.serviceAmount,     // ✅ NEW: Service amount
      protectionFee: this.protectionFee,    // ✅ NEW: Protection fee
      totalAmount: this.totalAmount,        // ✅ NEW: Total amount
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

  cancelPayment() {
    console.log('🚫 Payment cancelled by user');
    this.paymentFlowService.cancelPaymentFlow();
    this.notificationService.showNotification({
      type: 'info',
      message: 'Payment cancelled'
    });
  }

  // ✅ NEW: Helper methods for template
  get hasValidServiceAmount(): boolean {
    return this.serviceAmount > 0;
  }

  get isLoadingFeeCalculation(): boolean {
    return !this.feeCalculationLoaded && this.serviceAmount > 0;
  }

  get protectionFeeDisplayText(): string {
    if (!this.feeCalculationLoaded) {
      return 'Calculating...';
    }

    if (this.protectionFee === 0) {
      return 'No protection fee';
    }

    const percentage = this.protectionFeePercentage;
    return `${percentage.toFixed(1)}% protection fee`;
  }
}
