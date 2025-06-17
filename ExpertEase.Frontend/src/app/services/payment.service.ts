import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {
  PaymentConfirmationDTO,
  PaymentIntentCreateDTO,
  PaymentIntentResponseDTO,
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

  confirmPayment(dto: PaymentConfirmationDTO): Observable<RequestResponse<ServiceTask>> {
    return this.http.post<RequestResponse<ServiceTask>>(
      `${this.baseUrl}/ConfirmPayment`,
      dto
    );
  }

  getPaymentHistory(query: PaginationSearchQueryParams): Observable<RequestResponse<{ data: PaymentHistoryDTO[] }>> {
    return this.http.get<RequestResponse<{ data: PaymentHistoryDTO[] }>>(
      `${this.baseUrl}/GetPaymentHistory`,
      { params: query as any }
    );
  }

  getPaymentDetails(id: string): Observable<RequestResponse<PaymentDetailsDTO>> {
    return this.http.get<RequestResponse<PaymentDetailsDTO>>(
      `${this.baseUrl}/GetPaymentDetails/${id}`
    );
  }

  refundPayment(dto: PaymentRefundDTO): Observable<RequestResponse<Payment>> {
    return this.http.post<RequestResponse<Payment>>(
      `${this.baseUrl}/RefundPayment`,
      dto
    );
  }

  cancelPayment(paymentId: string): Observable<RequestResponse<Payment>> {
    return this.http.post<RequestResponse<Payment>>(
      `${this.baseUrl}/CancelPayment/${paymentId}`,
      {}
    );
  }

  stripeWebhook(payload: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/StripeWebhook`, payload);
  }
}
