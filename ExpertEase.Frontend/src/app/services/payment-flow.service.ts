// payment-flow.service.ts
import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import {
  ServicePaymentDetailsDTO,
  UserPaymentDetailsDTO,
  PaymentDetailsDTO,
  SpecialistDTO,
  FirestoreConversationItemDTO, ReplyDTO, RequestDTO
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
  private paymentFlowState = new BehaviorSubject<PaymentFlowState>({
    isActive: false,
    serviceDetails: null,
    userDetails: null,
    specialistDetails: null,
    replyId: null,
    conversationId: null
  });

  public paymentFlow$ = this.paymentFlowState.asObservable();

  // Subject for payment completion events
  private paymentCompleted = new BehaviorSubject<PaymentDetailsDTO | null>(null);
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

  completePayment(paymentDetails: PaymentDetailsDTO): void {
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
}
