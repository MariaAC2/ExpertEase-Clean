import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {
  CalculateProtectionFeeRequestDTO, CalculateProtectionFeeResponseDTO,
  PaymentConfirmationDTO, PaymentDetailsDTO,
  PaymentIntentCreateDTO,
  PaymentIntentResponseDTO, PaymentRefundDTO,
  RequestResponse
} from '../models/api.models';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';

@Injectable({
  providedIn: 'root'
})
export class PaymentService {
  private readonly baseUrl = 'http://localhost:5241/api/Payment';

  constructor(private readonly http: HttpClient, private readonly authService: AuthService) {}

  createPaymentIntent(dto: PaymentIntentCreateDTO): Observable<RequestResponse<PaymentIntentResponseDTO>> {
    console.log('Creating payment intent with data:', dto);

    const token = this.authService.getToken();
    const headers = {
      Authorization: `Bearer ${token}`
    };
    return this.http.post<RequestResponse<PaymentIntentResponseDTO>>(
      `${this.baseUrl}/CreatePaymentIntent`,
      dto,
      { headers }
    );
  }

  calculateProtectionFee(request: CalculateProtectionFeeRequestDTO): Observable<RequestResponse<CalculateProtectionFeeResponseDTO>> {
    console.log('Calculating protection fee for:', request);
    const token = this.authService.getToken();
    const headers = {
      Authorization: `Bearer ${token}`
    };
    return this.http.post<RequestResponse<CalculateProtectionFeeResponseDTO>>(
      `${this.baseUrl}/CalculateProtectionFee`,
      request,
      { headers }
    );
  }

  // ✅ UPDATED: Now returns just success/failure
  confirmPayment(dto: PaymentConfirmationDTO): Observable<RequestResponse<void>> {
    const token = this.authService.getToken();
    const headers = {
      Authorization: `Bearer ${token}`
    };
    return this.http.post<RequestResponse<void>>(
      `${this.baseUrl}/ConfirmPayment`,
      dto,
      { headers }
    );
  }

  getPaymentHistory(query: any): Observable<RequestResponse<{ data: any[] }>> {
    const token = this.authService.getToken();
    const headers = {
      Authorization: `Bearer ${token}`
    };
    return this.http.get<RequestResponse<{ data: any[] }>>(
      `${this.baseUrl}/GetPaymentHistory`,
      { params: query, headers }
    );
  }

  getPaymentDetails(id: string): Observable<RequestResponse<PaymentDetailsDTO>> {
    const token = this.authService.getToken();
    const headers = {
      Authorization: `Bearer ${token}`
    };
    return this.http.get<RequestResponse<PaymentDetailsDTO>>(
      `${this.baseUrl}/GetPaymentDetails/${id}`,
      { headers }
    );
  }

  // ✅ UPDATED: Now returns just success/failure
  refundPayment(dto: PaymentRefundDTO): Observable<RequestResponse<void>> {
    const token = this.authService.getToken();
    const headers = {
      Authorization: `Bearer ${token}`
    };
    return this.http.post<RequestResponse<void>>(
      `${this.baseUrl}/RefundPayment`,
      dto,
      { headers }
    );
  }

  // ✅ UPDATED: Now returns just success/failure
  cancelPayment(paymentId: string): Observable<RequestResponse<void>> {
    const token = this.authService.getToken();
    const headers = {
      Authorization: `Bearer ${token}`
    };
    return this.http.post<RequestResponse<void>>(
      `${this.baseUrl}/CancelPayment/${paymentId}`,
      {},
      { headers }
    );
  }

  stripeWebhook(payload: any): Observable<{ received: boolean; error?: string }> {
    return this.http.post<{ received: boolean; error?: string }>(
      `${this.baseUrl}/StripeWebhook`,
      payload
    );
  }
}
