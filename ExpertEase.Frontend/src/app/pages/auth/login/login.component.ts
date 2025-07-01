import { Component, OnInit, NgZone } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { LoginDTO, SocialLoginDTO } from '../../../models/api.models';
import { FormField, dtoToFormFields } from '../../../models/form.models';
import { DynamicFormComponent } from '../../../shared/dynamic-form/dynamic-form.component';

declare global {
  interface Window {
    google: any;
    FB: any;
    fbAsyncInit: () => void;
  }
}

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DynamicFormComponent
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  formFields: FormField[] = [];
  formData: { [key: string]: any } = {};
  errorMessage: string | null = null;
  isGoogleLoading = false;
  isFacebookLoading = false;

  constructor(
    private authService: AuthService,
    private router: Router,
    private ngZone: NgZone
  ) {
    const dto: LoginDTO = {
      email: '',
      password: ''
    };

    this.formFields = dtoToFormFields(dto, {
      email: { type: 'email' },
      password: { type: 'password' }
    });
  }

  ngOnInit() {
    this.loadGoogleSDK();
    this.loadFacebookSDK();
  }

  loginUser(data: { [key: string]: any }) {
    const loginData: LoginDTO = data as LoginDTO;

    this.authService.loginUser(loginData).subscribe({
      next: (res) => {
        console.log('Login successful:', res);
        this.router.navigate(['/home']);
      },
      error: (err) => {
        console.error('Login failed:', err);
        this.errorMessage = err.error?.errorMessage?.message || 'Eroare necunoscută.';
      }
    });
  }

  private loadGoogleSDK() {
    if (typeof window === 'undefined') return;

    // Check if Google SDK is already loaded
    if (window.google?.accounts?.id) {
      this.initializeGoogleSignIn();
      return;
    }

    // Load Google SDK
    const script = document.createElement('script');
    script.src = 'https://accounts.google.com/gsi/client';
    script.async = true;
    script.defer = true;
    script.onload = () => {
      console.log('Google SDK loaded');
      this.initializeGoogleSignIn();
    };
    script.onerror = (error) => {
      console.error('Failed to load Google SDK:', error);
    };
    document.head.appendChild(script);
  }

  private loadFacebookSDK() {
    if (typeof window === 'undefined') return;

    // Check if Facebook SDK is already loaded
    if (window.FB) {
      console.log('Facebook SDK already loaded');
      return;
    }

    // Initialize Facebook SDK
    window.fbAsyncInit = () => {
      window.FB.init({
        appId: '734399149543740', // Your Facebook App ID
        cookie: true,
        xfbml: true,
        version: 'v18.0'
      });

      // Check login status
      window.FB.getLoginStatus((response: any) => {
        console.log('Facebook login status:', response);
      });

      console.log('Facebook SDK initialized');
    };

    // Load Facebook SDK script
    const script = document.createElement('script');
    script.async = true;
    script.defer = true;
    script.crossOrigin = 'anonymous';
    script.src = 'https://connect.facebook.net/en_US/sdk.js';
    script.onerror = (error) => {
      console.error('Failed to load Facebook SDK:', error);
    };
    document.head.appendChild(script);
  }

  private initializeGoogleSignIn() {
    if (!window.google?.accounts?.id) {
      console.error('Google SDK not available');
      return;
    }

    try {
      window.google.accounts.id.initialize({
        client_id: '760852864614-0l1qdais39snht0oo3511r0tpbjdj09f.apps.googleusercontent.com',
        callback: (response: any) => {
          this.ngZone.run(() => {
            this.handleGoogleResponse(response);
          });
        },
        auto_select: false,
        cancel_on_tap_outside: true
      });
      console.log('Google Sign-In initialized');
    } catch (error) {
      console.error('Error initializing Google Sign-In:', error);
    }
  }

  signInWithGoogle() {
    if (!window.google?.accounts?.id) {
      this.errorMessage = 'Google SDK nu este disponibil. Vă rugăm să reîncărcați pagina.';
      return;
    }

    if (this.isGoogleLoading) return;

    this.isGoogleLoading = true;
    this.errorMessage = null;

    try {
      window.google.accounts.id.prompt((notification: any) => {
        this.ngZone.run(() => {
          this.isGoogleLoading = false;
          if (notification.isNotDisplayed() || notification.isSkippedMoment()) {
            console.log('Google prompt was not displayed or was skipped');
            this.errorMessage = 'Procesul de autentificare Google a fost anulat.';
          }
        });
      });
    } catch (error) {
      this.ngZone.run(() => {
        this.isGoogleLoading = false;
        console.error('Google sign-in error:', error);
        this.errorMessage = 'Eroare la inițializarea autentificării Google.';
      });
    }
  }

  signInWithFacebook() {
    if (!window.FB) {
      this.errorMessage = 'Facebook SDK nu este disponibil. Vă rugăm să reîncărcați pagina.';
      return;
    }

    if (this.isFacebookLoading) return;

    this.isFacebookLoading = true;
    this.errorMessage = null;

    // First check if user is already logged in
    window.FB.getLoginStatus((statusResponse: any) => {
      this.ngZone.run(() => {
        if (statusResponse.status === 'connected') {
          // User is already logged in, use existing token
          this.handleFacebookResponse(statusResponse.authResponse.accessToken);
        } else {
          // User needs to log in
          window.FB.login((loginResponse: any) => {
            this.ngZone.run(() => {
              this.isFacebookLoading = false;
              if (loginResponse.authResponse) {
                this.handleFacebookResponse(loginResponse.authResponse.accessToken);
              } else {
                console.error('Facebook login failed or cancelled');
                this.errorMessage = 'Autentificarea Facebook a fost anulată sau a eșuat.';
              }
            });
          }, {
            scope: 'email,public_profile',
            return_scopes: true
          });
        }
      });
    });
  }

  private handleGoogleResponse(response: any) {
    this.isGoogleLoading = false;

    if (!response.credential) {
      this.errorMessage = 'Nu s-a primit token-ul de la Google.';
      return;
    }

    const socialLoginData: SocialLoginDTO = {
      token: response.credential,
      provider: 'google'
    };

    console.log('Sending Google social login data:', socialLoginData);

    this.authService.socialLogin(socialLoginData).subscribe({
      next: (result) => {
        console.log('Google social login successful:', result);
        this.router.navigate(['/home']);
      },
      error: (err) => {
        console.error('Google social login failed:', err);
        this.errorMessage = err.error?.errorMessage?.message || 'Eroare la autentificarea cu Google.';
      }
    });
  }

  private handleFacebookResponse(accessToken: string) {
    this.isFacebookLoading = false;

    if (!accessToken) {
      this.errorMessage = 'Nu s-a primit token-ul de la Facebook.';
      return;
    }

    const socialLoginData: SocialLoginDTO = {
      token: accessToken,
      provider: 'facebook'
    };

    console.log('Sending Facebook social login data:', socialLoginData);

    this.authService.socialLogin(socialLoginData).subscribe({
      next: (result) => {
        console.log('Facebook social login successful:', result);
        this.router.navigate(['/home']);
      },
      error: (err) => {
        console.error('Facebook social login failed:', err);
        this.errorMessage = err.error?.errorMessage?.message || 'Eroare la autentificarea cu Facebook.';
      }
    });
  }

  goToRegister(): void {
    this.router.navigate(['/register']);
  }
}
