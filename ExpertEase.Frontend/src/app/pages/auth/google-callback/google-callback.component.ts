import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, Params } from '@angular/router';
import { CommonModule } from '@angular/common';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-google-callback',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './google-callback.component.html',
  styleUrls: ['./google-callback.component.scss']
})
export class GoogleCallbackComponent implements OnInit {
  errorMessage: string | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router
  ) {}

  ngOnInit() {
    console.log('Google callback component initialized');
    this.handleGoogleCallback();
  }

// Update your google-callback.component.ts to add more debugging:

  private handleGoogleCallback() {
    // Get ALL query parameters to see what Google sent
    const params = this.route.snapshot.queryParams;
    const fullUrl = window.location.href;

    console.log('=== GOOGLE CALLBACK DEBUG ===');
    console.log('Full URL:', fullUrl);
    console.log('All query params:', params);
    console.log('Available param keys:', Object.keys(params));

    // Check each parameter individually
    console.log('code:', params['code']);
    console.log('state:', params['state']);
    console.log('error:', params['error']);
    console.log('error_description:', params['error_description']);
    console.log('scope:', params['scope']);
    console.log('authuser:', params['authuser']);
    console.log('prompt:', params['prompt']);
    console.log('=============================');

    const code = params['code'];
    const state = params['state'];
    const error = params['error'];
    const errorDescription = params['error_description'];

    // Show what we received in the UI temporarily
    if (Object.keys(params).length === 0) {
      this.errorMessage = `No parameters received. Full URL: ${fullUrl}`;
      return;
    }

    if (error) {
      console.error('Google OAuth error:', error, errorDescription);
      this.handleError(this.getErrorMessage(error, errorDescription));
      return;
    }

    if (!code || !state) {
      console.error('Missing code or state parameter');
      this.errorMessage = `Missing parameters. Received: code=${!!code}, state=${!!state}. All params: ${JSON.stringify(params)}`;
      return;
    }

    // Rest of your existing logic...
    const storedState = sessionStorage.getItem('google_oauth_state');
    if (state !== storedState) {
      console.error('State mismatch:', { received: state, stored: storedState });
      this.handleError('Eroare de securitate în autentificarea Google.');
      return;
    }

    console.log('Valid Google OAuth response received');

    if (window.opener) {
      console.log('Handling popup callback');
      this.handlePopupCallback(code, state);
    } else {
      console.log('Handling redirect callback');
      this.handleRedirectCallback(code);
    }
  }

  private handlePopupCallback(code: string, state: string) {
    try {
      // Store code and state for parent window
      localStorage.setItem('google_auth_code', code);
      localStorage.setItem('google_auth_state', state);

      console.log('Stored auth data for parent window, closing popup');

      // Close popup
      window.close();
    } catch (error) {
      console.error('Error in popup callback:', error);
      localStorage.setItem('google_auth_error', 'Eroare la procesarea răspunsului Google.');
      window.close();
    }
  }

  private handleRedirectCallback(code: string) {
    // For redirect flow, we need to exchange the code here
    // and then redirect to the appropriate page

    const oauthType = sessionStorage.getItem('google_oauth_type') || 'login';
    const redirectPath = sessionStorage.getItem('google_oauth_redirect') || '/home';

    // Clean up session storage
    sessionStorage.removeItem('google_oauth_state');
    sessionStorage.removeItem('google_oauth_type');
    sessionStorage.removeItem('google_oauth_redirect');

    // Store the code and redirect to login/register page to handle the exchange
    localStorage.setItem('google_auth_code_redirect', code);
    localStorage.setItem('google_auth_type', oauthType);

    // Redirect back to the appropriate page
    if (oauthType === 'register') {
      this.router.navigate(['/register'], {
        queryParams: { google_auth: 'processing' }
      });
    } else {
      this.router.navigate(['/login'], {
        queryParams: { google_auth: 'processing' }
      });
    }
  }

  private handleError(message: string) {
    this.errorMessage = message;

    // If this is a popup, store error for parent window
    if (window.opener) {
      localStorage.setItem('google_auth_error', message);
      setTimeout(() => {
        window.close();
      }, 3000);
    }
  }

  private getErrorMessage(error: string, description?: string): string {
    switch (error) {
      case 'access_denied':
        return 'Accesul a fost refuzat. Vă rugăm să acceptați permisiunile Google.';
      case 'invalid_request':
        return 'Cerere invalidă către Google. Vă rugăm să încercați din nou.';
      case 'unauthorized_client':
        return 'Client neautorizat. Contactați administratorul.';
      case 'unsupported_response_type':
        return 'Tip de răspuns nesuportat. Eroare de configurare.';
      case 'invalid_scope':
        return 'Permisiuni invalide solicitate.';
      case 'server_error':
        return 'Eroare de server Google. Încercați din nou mai târziu.';
      case 'temporarily_unavailable':
        return 'Serviciul Google este temporar indisponibil.';
      default:
        return description || `Eroare Google OAuth: ${error}`;
    }
  }

  goBack() {
    const oauthType = sessionStorage.getItem('google_oauth_type') || 'login';

    // Clean up any stored data
    sessionStorage.clear();
    localStorage.removeItem('google_auth_code');
    localStorage.removeItem('google_auth_state');
    localStorage.removeItem('google_auth_error');
    localStorage.removeItem('google_auth_code_redirect');
    localStorage.removeItem('google_auth_type');

    // Navigate back to appropriate page
    if (oauthType === 'register') {
      this.router.navigate(['/register']);
    } else {
      this.router.navigate(['/login']);
    }
  }
}
