import { Component, OnInit, NgZone } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import {Router, RouterLink, RouterLinkActive} from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import {LoginDTO, UserRegisterDTO, SocialLoginDTO} from '../../../models/api.models';
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
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DynamicFormComponent,
    RouterLink,
    RouterLinkActive
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent implements OnInit {
  formFields: FormField[] = [];
  formData: { [key: string]: any } = {};
  errorMessage: string | null = null;
  acceptedPolicies: boolean = false;
  showPolicyError: boolean = false;
  isGoogleLoading = false;
  isFacebookLoading = false;
  sdkLoadAttempts = 0;
  maxSdkLoadAttempts = 3;

  constructor(
    private authService: AuthService,
    private router: Router,
    private ngZone: NgZone
  ) {
    const dto: UserRegisterDTO = {
      firstName: '',
      lastName: '',
      email: '',
      password: ''
    };

    this.formFields = dtoToFormFields(dto, {
      email: {type: 'email'},
      password: {type: 'password'}
    });

    console.log('Registration form fields:', this.formFields);
  }

  ngOnInit() {
    this.debugEnvironment();
    this.loadGoogleSDK();
    this.loadFacebookSDK();
  }

  private debugEnvironment() {
    console.log('=== Registration Debug Info ===');
    console.log('Current URL:', window.location.href);
    console.log('Protocol:', window.location.protocol);
    console.log('Host:', window.location.host);
    console.log('User Agent:', navigator.userAgent);
    console.log('===============================');
  }

  registerUser(data: { [key: string]: any }) {
    if (!this.acceptedPolicies) {
      this.showPolicyError = true;
      return;
    }

    this.showPolicyError = false;
    const userDto: UserRegisterDTO = data as UserRegisterDTO;

    this.authService.registerUser(userDto).subscribe({
      next: () => {
        const loginDto: LoginDTO = {
          email: userDto.email,
          password: userDto.password
        };

        this.authService.loginUser(loginDto).subscribe({
          next: () => {
            this.router.navigate(['/home']);
          },
          error: (err) => {
            console.error('Auto-login after registration failed:', err);
            this.errorMessage = 'Înregistrare reușită, dar autentificarea automată a eșuat. Vă rugăm să vă autentificați manual.';
          }
        });
      },
      error: (err) => {
        console.error('Registration failed:', err);
        this.errorMessage = err.error?.errorMessage?.message || 'Eroare la înregistrare.';
      }
    });
  }

  private async loadGoogleSDK() {
    if (typeof window === 'undefined') return;

    try {
      // Check if already loaded
      if (window.google?.accounts?.id) {
        console.log('Google SDK already available');
        await this.initializeGoogleSignIn();
        return;
      }

      console.log('Loading Google SDK...');
      const script = document.createElement('script');
      script.src = 'https://accounts.google.com/gsi/client';
      script.async = true;
      script.defer = true;

      script.onload = async () => {
        console.log('Google SDK loaded successfully');
        // Wait a bit for SDK to fully initialize
        setTimeout(async () => {
          await this.initializeGoogleSignIn();
        }, 100);
      };

      script.onerror = (error) => {
        console.error('Failed to load Google SDK:', error);
        this.errorMessage = 'Nu s-a putut încărca SDK-ul Google. Verificați conexiunea la internet.';
      };

      document.head.appendChild(script);
    } catch (error) {
      console.error('Error loading Google SDK:', error);
    }
  }

  private async loadFacebookSDK() {
    if (typeof window === 'undefined') return;

    try {
      if (window.FB) {
        console.log('Facebook SDK already available');
        return;
      }

      console.log('Loading Facebook SDK...');

      window.fbAsyncInit = () => {
        window.FB.init({
          appId: '734399149543740',
          cookie: true,
          xfbml: true,
          version: 'v18.0'
        });
        console.log('Facebook SDK initialized successfully');
      };

      const script = document.createElement('script');
      script.async = true;
      script.defer = true;
      script.crossOrigin = 'anonymous';
      script.src = 'https://connect.facebook.net/en_US/sdk.js';

      script.onerror = (error) => {
        console.error('Failed to load Facebook SDK:', error);
      };

      document.head.appendChild(script);
    } catch (error) {
      console.error('Error loading Facebook SDK:', error);
    }
  }

  private async initializeGoogleSignIn(): Promise<void> {
    return new Promise((resolve, reject) => {
      const maxAttempts = 3;
      let attempt = 0;

      const tryInitialize = () => {
        attempt++;
        console.log(`Google SDK initialization attempt ${attempt}/${maxAttempts}`);

        if (!window.google?.accounts?.id) {
          if (attempt < maxAttempts) {
            setTimeout(tryInitialize, 1000);
            return;
          }
          reject(new Error('Google SDK not available after all attempts'));
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
            cancel_on_tap_outside: true,
            use_fedcm_for_prompt: false,
            context: 'signup'
          });

          console.log('Google Sign-In initialized successfully for registration');
          resolve();
        } catch (error) {
          console.error('Error initializing Google Sign-In:', error);
          if (attempt < maxAttempts) {
            setTimeout(tryInitialize, 1000);
          } else {
            reject(error);
          }
        }
      };

      tryInitialize();
    });
  }

  signUpWithGoogle() {
    console.log('Google sign-up clicked');

    // Check policies
    if (!this.acceptedPolicies) {
      this.showPolicyError = true;
      this.errorMessage = 'Trebuie să acceptați termenii și condițiile pentru a continua.';
      return;
    }

    this.showPolicyError = false;

    // Check SDK availability
    if (!window.google?.accounts?.id) {
      this.errorMessage = 'Google SDK nu este disponibil. Încercați să reîncărcați pagina.';
      return;
    }

    if (this.isGoogleLoading) {
      console.log('Google sign-up already in progress');
      return;
    }

    this.isGoogleLoading = true;
    this.errorMessage = null;

    console.log('Starting Google sign-up process...');

    try {
      // Create a promise to handle the popup
      const signUpPromise = new Promise<void>((resolve, reject) => {
        const timeout = setTimeout(() => {
          reject(new Error('Google sign-up timeout'));
        }, 30000); // 30 second timeout

        window.google.accounts.id.prompt((notification: any) => {
          clearTimeout(timeout);

          if (notification.isNotDisplayed()) {
            const reason = notification.getNotDisplayedReason();
            console.log('Google prompt not displayed:', reason);
            reject(new Error(`Google prompt not displayed: ${reason}`));
          } else if (notification.isSkippedMoment()) {
            const reason = notification.getSkippedReason();
            console.log('Google prompt skipped:', reason);
            reject(new Error(`Google prompt skipped: ${reason}`));
          } else {
            resolve();
          }
        });
      });

      signUpPromise.catch((error) => {
        this.ngZone.run(() => {
          this.isGoogleLoading = false;
          console.error('Google sign-up promise rejected:', error);

          let errorMsg = 'Eroare la înregistrarea cu Google.';

          if (error.message.includes('timeout')) {
            errorMsg = 'Timp expirat pentru Google Sign-In. Încercați din nou.';
          } else if (error.message.includes('not displayed')) {
            errorMsg = 'Pop-up-ul Google nu poate fi afișat. Verificați setările browser-ului.';
          } else if (error.message.includes('skipped')) {
            errorMsg = 'Procesul de înregistrare Google a fost anulat.';
          }

          this.errorMessage = errorMsg;
        });
      });

    } catch (error) {
      this.ngZone.run(() => {
        this.isGoogleLoading = false;
        console.error('Google sign-up error:', error);
        this.errorMessage = 'Eroare la inițializarea înregistrării Google: ' + (error as Error).message;
      });
    }
  }

  signUpWithFacebook() {
    console.log('Facebook sign-up clicked');

    // Check policies
    if (!this.acceptedPolicies) {
      this.showPolicyError = true;
      this.errorMessage = 'Trebuie să acceptați termenii și condițiile pentru a continua.';
      return;
    }

    this.showPolicyError = false;

    if (!window.FB) {
      this.errorMessage = 'Facebook SDK nu este disponibil. Încercați să reîncărcați pagina.';
      return;
    }

    if (this.isFacebookLoading) return;

    this.isFacebookLoading = true;
    this.errorMessage = null;

    window.FB.getLoginStatus((statusResponse: any) => {
      this.ngZone.run(() => {
        console.log('Facebook status:', statusResponse);

        if (statusResponse.status === 'connected') {
          this.handleFacebookResponse(statusResponse.authResponse.accessToken);
        } else {
          window.FB.login((loginResponse: any) => {
            this.ngZone.run(() => {
              this.isFacebookLoading = false;
              console.log('Facebook login response:', loginResponse);

              if (loginResponse.authResponse) {
                this.handleFacebookResponse(loginResponse.authResponse.accessToken);
              } else {
                console.error('Facebook login failed or cancelled:', loginResponse);
                this.errorMessage = 'Înregistrarea Facebook a fost anulată sau a eșuat.';
              }
            });
          }, {
            scope: 'email,public_profile',
            return_scopes: true,
            auth_type: 'rerequest'
          });
        }
      });
    });
  }

  private handleGoogleResponse(response: any) {
    this.isGoogleLoading = false;

    console.log('Google response received:', response);

    if (!response.credential) {
      this.errorMessage = 'Nu s-a primit token-ul de la Google. Încercați din nou.';
      return;
    }

    const socialLoginData: SocialLoginDTO = {
      token: response.credential,
      provider: 'google'
    };

    console.log('Sending Google social registration data:', socialLoginData);

    this.authService.socialLogin(socialLoginData).subscribe({
      next: (result) => {
        console.log('Google social registration successful:', result);
        this.router.navigate(['/home']);
      },
      error: (err) => {
        console.error('Google social registration failed:', err);
        console.error('Full error object:', JSON.stringify(err, null, 2));

        let errorMsg = 'Eroare la înregistrarea cu Google.';

        // Enhanced error handling
        if (err.status === 400) {
          if (err.error?.errorMessage?.message) {
            const backendMsg = err.error.errorMessage.message;
            if (backendMsg.includes('Invalid') && backendMsg.includes('token')) {
              errorMsg = 'Token Google invalid. Încercați să vă deconectați din Google și să încercați din nou.';
            } else if (backendMsg.includes('already exists')) {
              errorMsg = 'Un cont cu acest email există deja. Încercați să vă autentificați în loc să vă înregistrați.';
            } else if (backendMsg.includes('email')) {
              errorMsg = 'Eroare la procesarea email-ului. Asigurați-vă că ați acordat permisiunea pentru email în Google.';
            } else {
              errorMsg = backendMsg;
            }
          }
        } else if (err.status === 500) {
          errorMsg = 'Eroare de server. Încercați din nou mai târziu.';
        } else if (err.status === 0) {
          errorMsg = 'Eroare de conexiune. Verificați conexiunea la internet.';
        }

        this.errorMessage = errorMsg;
      }
    });
  }

  private handleFacebookResponse(accessToken: string) {
    this.isFacebookLoading = false;

    console.log('Facebook token received:', accessToken ? 'Token present' : 'No token');

    if (!accessToken) {
      this.errorMessage = 'Nu s-a primit token-ul de la Facebook. Încercați din nou.';
      return;
    }

    const socialLoginData: SocialLoginDTO = {
      token: accessToken,
      provider: 'facebook'
    };

    console.log('Sending Facebook social registration data:', socialLoginData);

    this.authService.socialLogin(socialLoginData).subscribe({
      next: (result) => {
        console.log('Facebook social registration successful:', result);
        this.router.navigate(['/home']);
      },
      error: (err) => {
        console.error('Facebook social registration failed:', err);
        console.error('Full error object:', JSON.stringify(err, null, 2));

        let errorMsg = 'Eroare la înregistrarea cu Facebook.';

        // Enhanced error handling for Facebook
        if (err.status === 400) {
          if (err.error?.errorMessage?.message) {
            const backendMsg = err.error.errorMessage.message;
            if (backendMsg.includes('Invalid') && backendMsg.includes('token')) {
              errorMsg = 'Token Facebook invalid. Încercați să vă deconectați din Facebook și să încercați din nou.';
            } else if (backendMsg.includes('already exists')) {
              errorMsg = 'Un cont cu acest email există deja. Încercați să vă autentificați în loc să vă înregistrați.';
            } else if (backendMsg.includes('email')) {
              errorMsg = 'Email-ul nu este disponibil din Facebook. Asigurați-vă că ați acordat permisiunea pentru email.';
            } else {
              errorMsg = backendMsg;
            }
          }
        } else if (err.status === 500) {
          errorMsg = 'Eroare de server. Încercați din nou mai târziu.';
        } else if (err.status === 0) {
          errorMsg = 'Eroare de conexiune. Verificați conexiunea la internet.';
        }

        this.errorMessage = errorMsg;
      }
    });
  }

  // Additional helper methods for debugging
  private debugGoogleError(error: any) {
    console.log('=== Google Error Debug ===');
    console.log('Error type:', typeof error);
    console.log('Error message:', error.message || 'No message');
    console.log('Error stack:', error.stack || 'No stack');
    console.log('Full error:', error);

    // Check Google SDK state
    console.log('Google SDK available:', !!window.google?.accounts?.id);
    if (window.google?.accounts?.id) {
      console.log('Google SDK methods:', Object.keys(window.google.accounts.id));
    }
    console.log('========================');
  }

  // Manual retry method for Google sign-up
  retryGoogleSignUp() {
    console.log('Retrying Google sign-up...');
    this.isGoogleLoading = false;
    this.errorMessage = null;

    // Wait a moment then try again
    setTimeout(() => {
      this.signUpWithGoogle();
    }, 1000);
  }

  // Manual retry method for Facebook sign-up
  retryFacebookSignUp() {
    console.log('Retrying Facebook sign-up...');
    this.isFacebookLoading = false;
    this.errorMessage = null;

    // First try to logout from Facebook to reset state
    if (window.FB) {
      window.FB.logout(() => {
        setTimeout(() => {
          this.signUpWithFacebook();
        }, 1000);
      });
    } else {
      setTimeout(() => {
        this.signUpWithFacebook();
      }, 1000);
    }
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
