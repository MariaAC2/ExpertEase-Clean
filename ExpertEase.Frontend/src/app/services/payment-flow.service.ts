// payment-flow.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import {
  ServicePaymentDetailsDTO,
  UserPaymentDetailsDTO,
  PaymentDetailsDTO,
} from '../models/api.models';

export interface PaymentFlowState {
  isActive: boolean;
  serviceDetails: ServicePaymentDetailsDTO | null;
  userDetails: UserPaymentDetailsDTO | null;
  specialistDetails: UserPaymentDetailsDTO | null;
  replyId: string | null;
  conversationId: string | null;
}

@Injectable({
  providedIn: 'root'
})
export class PaymentFlowService {
  private readonly paymentFlowState = new BehaviorSubject<PaymentFlowState>({
    isActive: false,
    serviceDetails: null,
    userDetails: null,
    specialistDetails: null,
    replyId: null,
    conversationId: null
  });

  public paymentFlow$ = this.paymentFlowState.asObservable();

  // Subject for payment completion events
  private readonly paymentCompleted = new BehaviorSubject<PaymentDetailsDTO | null>(null);
  public paymentCompleted$ = this.paymentCompleted.asObservable();

  initiatePaymentFlow(
    replyId: string,
    serviceDetails: ServicePaymentDetailsDTO,
    userDetails: UserPaymentDetailsDTO,
    specialistDetails: UserPaymentDetailsDTO,
    conversationId: string
  ): void {

    this.paymentFlowState.next({
      isActive: true,
      serviceDetails,
      userDetails,
      specialistDetails,
      replyId: replyId,
      conversationId
    });
  }

  // ✅ FIXED: Added optional parameter and proper handling
  completePayment(paymentDetails: PaymentDetailsDTO | null = null): void {
    this.paymentCompleted.next(paymentDetails);
    this.cancelPaymentFlow();
  }

  cancelPaymentFlow(): void {
    this.paymentFlowState.next({
      isActive: false,
      serviceDetails: null,
      userDetails: null,
      specialistDetails: null,
      replyId: null,
      conversationId: null
    });
  }

  getCurrentState(): PaymentFlowState {
    return this.paymentFlowState.value;
  }

  // ✅ ADDED: Helper method to get current reply ID
  getCurrentReplyId(): string | null {
    return this.paymentFlowState.value.replyId;
  }

  // ✅ ADDED: Helper method to check if payment flow is active
  isPaymentFlowActive(): boolean {
    return this.paymentFlowState.value.isActive;
  }

  // ✅ ADDED: Helper method to get payment amount
  getPaymentAmount(): number {
    const serviceDetails = this.paymentFlowState.value.serviceDetails;
    return serviceDetails?.price || 0;
  }
}
