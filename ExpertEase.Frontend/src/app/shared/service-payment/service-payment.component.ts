import {AfterViewInit, Component, EventEmitter, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {CurrencyPipe, DatePipe, NgIf} from '@angular/common';
import {
  JobStatusEnum, PaymentDetailsDTO, PaymentStatusEnum,
  ServicePaymentDetailsDTO,
  ServiceTaskDTO,
  SpecialistDTO, UserDetailsDTO,
  UserPaymentDetailsDTO
} from '../../models/api.models';
import {FormsModule} from '@angular/forms';
import { loadStripe, StripeCardElement } from '@stripe/stripe-js';
import {Subject, takeUntil} from 'rxjs';
import {PaymentFlowService, PaymentFlowState} from '../../services/payment-flow.service';

@Component({
  selector: 'app-service-payment',
  imports: [
    FormsModule,
    DatePipe,
    CurrencyPipe,
    NgIf
  ],
  templateUrl: './service-payment.component.html',
  styleUrl: './service-payment.component.scss'
})
export class ServicePaymentComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  // State from payment flow service
  serviceDetails: ServicePaymentDetailsDTO | null = null;
  userDetails: UserPaymentDetailsDTO | null = null;
  specialistDetails: UserPaymentDetailsDTO | null = null;

  // Payment UI state
  paymentMethodSelected = false;
  isProcessing = false;
  showError = false;

  // Price calculations
  protectionFee = 5.00; // Fixed protection fee for now

  get totalAmount(): number {
    if (!this.serviceDetails) return 0;
    return this.serviceDetails.price + this.protectionFee;
  }

  constructor(
    private paymentFlowService: PaymentFlowService
    // private paymentService: PaymentService // Uncomment when implementing actual payment
  ) {}

  ngOnInit() {
    this.paymentFlowService.paymentFlow$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        if (state.isActive) {
          this.serviceDetails = state.serviceDetails;
          this.userDetails = state.userDetails;
          this.specialistDetails = state.specialistDetails;
        }
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  selectCardPayment(): void {
    this.paymentMethodSelected = true;
    this.showError = false;
  }

  processPayment(): void {
    if (!this.paymentMethodSelected) {
      this.showError = true;
      return;
    }

    if (!this.serviceDetails || !this.userDetails || !this.specialistDetails) {
      console.error('Missing payment details');
      return;
    }

    this.isProcessing = true;

    // For now, simulate successful payment after 2 seconds
    // Replace this with actual Stripe integration
    setTimeout(() => {
      this.handlePaymentSuccess();
    }, 2000);

    /* TODO: Replace simulation with actual payment processing
    const paymentData = {
      amount: this.totalAmount,
      currency: 'RON',
      serviceDetails: this.serviceDetails,
      userDetails: this.userDetails,
      specialistDetails: this.specialistDetails
    };

    this.paymentService.processPayment(paymentData).subscribe({
      next: (result) => {
        this.handlePaymentSuccess();
      },
      error: (error) => {
        console.error('Payment failed:', error);
        this.isProcessing = false;
        // Show error message to user
      }
    });
    */
  }

  private handlePaymentSuccess(): void {
    const paymentDetails: PaymentDetailsDTO = {
      id: `payment_${Date.now()}`,
      amount: this.totalAmount,
      currency: 'RON',
      status: PaymentStatusEnum.Completed,
      serviceTaskId: this.serviceDetails!.serviceTaskId,
      paidAt: new Date().toISOString(),
      createdAt: new Date().toISOString(),
      stripePaymentIntentId: `pi_${Date.now()}`,
      serviceDescription: this.serviceDetails!.description,
      serviceAddress: this.serviceDetails!.address,
      serviceStartDate: this.serviceDetails!.startDate,
      serviceEndDate: this.serviceDetails!.endDate,
      specialistName: this.specialistDetails!.userFullName,
      clientName: this.userDetails!.userFullName,
    };

    this.paymentFlowService.completePayment(paymentDetails);
    this.isProcessing = false;
  }

  cancelPayment(): void {
    this.paymentFlowService.cancelPaymentFlow();
  }
}
