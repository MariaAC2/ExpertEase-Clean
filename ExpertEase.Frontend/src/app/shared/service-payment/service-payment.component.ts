import {AfterViewInit, Component, EventEmitter, Input, OnDestroy, OnInit, Output} from '@angular/core';
import {CurrencyPipe, DatePipe} from '@angular/common';
import {
  JobStatusEnum, PaymentDetailsDTO,
  ServicePaymentDetailsDTO,
  ServiceTaskDTO,
  SpecialistDTO,
  UserPaymentDetailsDTO
} from '../../models/api.models';
import {FormsModule} from '@angular/forms';
import { loadStripe, StripeCardElement } from '@stripe/stripe-js';
import {Subject, takeUntil} from 'rxjs';
import {PaymentFlowService, PaymentFlowState} from '../../services/payment-flow.service';

@Component({
  selector: 'app-service-payment',
  imports: [
    FormsModule
  ],
  templateUrl: './service-payment.component.html',
  styleUrl: './service-payment.component.scss'
})
export class ServicePaymentComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();
  paymentFlowState: PaymentFlowState = {
    isActive: false,
    serviceDetails: null,
    userDetails: null,
    specialistDetails: null,
    replyId: null,
    conversationId: null
  };

  constructor(
    private paymentFlowService: PaymentFlowService,
    // private paymentService: PaymentService
  ) {}

  ngOnInit() {
    this.paymentFlowService.paymentFlow$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        this.paymentFlowState = state;
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  // onPaymentConfirmed(paymentDetails: PaymentDetailsDTO): void {
  //   console.log('Processing payment:', paymentDetails);
  //
  //   // Call backend to confirm payment and create service task
  //   this.paymentService.confirmPayment(paymentDetails).subscribe({
  //     next: (res) => {
  //       console.log('Payment confirmed on backend:', res);
  //
  //       // Notify other components that payment is complete
  //       this.paymentFlowService.completePayment(paymentDetails);
  //
  //       // Show success message
  //       this.showPaymentSuccess();
  //     },
  //     error: (err) => {
  //       console.error('Error confirming payment:', err);
  //       alert('Eroare la confirmarea plății. Te rugăm să contactezi suportul.');
  //     }
  //   });
  // }

  onPaymentCancelled(): void {
    console.log('Payment cancelled');
    this.paymentFlowService.cancelPaymentFlow();
  }

  private showPaymentSuccess(): void {
    // You could show a toast notification here
    console.log('Payment successful!');
  }
}
