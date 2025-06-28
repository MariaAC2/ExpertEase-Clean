// customer-payment-method.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AuthService } from './auth.service';
import { 
  SaveCustomerPaymentMethodDto, 
  CustomerPaymentMethodDto, 
  RequestResponse 
} from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class CustomerPaymentMethodService {
  private readonly baseUrl = 'http://localhost:5241/api/CustomerPaymentMethod';

  constructor(
    private readonly http: HttpClient, 
    private readonly authService: AuthService
  ) {}

  /**
   * Save a new customer payment method
   */
  savePaymentMethod(dto: SaveCustomerPaymentMethodDto): Observable<RequestResponse<CustomerPaymentMethodDto>> {
    const token = this.authService.getToken();
    const headers = {
      Authorization: `Bearer ${token}`
    };
    
    return this.http.post<RequestResponse<CustomerPaymentMethodDto>>(
      `${this.baseUrl}/Add`,
      dto,
      { headers }
    );
  }

  /**
   * Get all saved payment methods for current user
   */
  getPaymentMethods(): Observable<RequestResponse<CustomerPaymentMethodDto[]>> {
    const token = this.authService.getToken();
    const headers = {
      Authorization: `Bearer ${token}`
    };
    
    return this.http.get<RequestResponse<CustomerPaymentMethodDto[]>>(
      `${this.baseUrl}/GetAll`,
      { headers }
    );
  }

  /**
   * Delete a saved payment method
   */
  deletePaymentMethod(paymentMethodId: string): Observable<RequestResponse<void>> {
    const token = this.authService.getToken();
    const headers = {
      Authorization: `Bearer ${token}`
    };
    
    return this.http.delete<RequestResponse<void>>(
      `${this.baseUrl}/Delete/${paymentMethodId}`,
      { headers }
    );
  }

  /**
   * Set a payment method as default
   */
  setAsDefault(paymentMethodId: string): Observable<RequestResponse<void>> {
    const token = this.authService.getToken();
    const headers = {
      Authorization: `Bearer ${token}`
    };
    
    return this.http.put<RequestResponse<void>>(
      `${this.baseUrl}/SetDefault/${paymentMethodId}`,
      {},
      { headers }
    );
  }
}