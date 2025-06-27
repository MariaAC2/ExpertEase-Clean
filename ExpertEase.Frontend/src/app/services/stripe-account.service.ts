import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {Observable} from 'rxjs';
import {RequestResponse, StripeAccountStatusDTO} from '../models/api.models';
import {AuthService} from './auth.service';

interface StripeAccountLinkResponse {
  url: string;
}

@Injectable({
  providedIn: 'root'
})
export class StripeAccountService {
  private readonly baseUrl = `http://localhost:5241/api/stripe/account`;

  constructor(private readonly http: HttpClient, private readonly authService: AuthService) {}

  // Generate onboarding link for existing account (activation)
  generateOnboardingLink(accountId: string): Observable<RequestResponse<StripeAccountLinkResponse>> {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.post<RequestResponse<StripeAccountLinkResponse>>(
      `${this.baseUrl}/onboarding-link/${accountId}`,
      {},
      { headers }
    );
  }

  // Generate dashboard link for complete account
  generateDashboardLink(accountId: string): Observable<RequestResponse<StripeAccountLinkResponse>> {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.post<RequestResponse<StripeAccountLinkResponse>>(
      `${this.baseUrl}/dashboard-link/${accountId}`,
      { headers }
    );
  }

  // Get account status from Stripe (optional - for future use)
  getAccountStatus(accountId: string): Observable<RequestResponse<StripeAccountStatusDTO>> {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.get<RequestResponse<StripeAccountStatusDTO>>(
      `${this.baseUrl}/status/${accountId}`,
      { headers }
    );
  }
}
