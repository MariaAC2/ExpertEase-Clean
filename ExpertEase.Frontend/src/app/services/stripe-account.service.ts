import { Injectable } from '@angular/core';
import {HttpClient} from '@angular/common/http';
import {Observable} from 'rxjs';
import {RequestResponse} from '../models/api.models';

interface StripeAccountLinkResponse {
  url: string;
}
@Injectable({
  providedIn: 'root'
})
export class StripeAccountService {
  private readonly baseUrl = `http://localhost:5241/api/stripe/account`;

  constructor(private readonly http: HttpClient) {}

  generateOnboardingLink(accountId: string): Observable<RequestResponse<StripeAccountLinkResponse>> {
    return this.http.post<RequestResponse<StripeAccountLinkResponse>>(
      `${this.baseUrl}/onboarding-link/${accountId}`,
      {}
    );
  }
}
