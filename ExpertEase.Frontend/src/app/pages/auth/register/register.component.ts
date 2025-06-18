import { Component } from '@angular/core';
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
export class RegisterComponent {
  formFields: FormField[] = [];
  formData: { [key: string]: any } = {};
  errorMessage: string | null = null;
  acceptedPolicies: boolean = false;
  showPolicyError: boolean = false;

  constructor(private authService: AuthService, private router: Router) {
    const dto: UserRegisterDTO = {
      firstName: '',
      lastName: '',
      email: '',
      password: ''
    };

    this.formFields = dtoToFormFields(dto, {
      email: { type: 'email' },
      password: { type: 'password' }
    });

    console.log(this.formFields);
    this.initializeGoogleSignIn();
    this.initializeFacebookSDK();
  }

  registerUser(data: { [key: string]: any }) {
    // Check if policies are accepted
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
            console.error('Login failed:', err);
            this.errorMessage = err.error?.errorMessage?.message || 'Eroare necunoscută.';
          }
        });
      },
      error: (err) => {
        console.error('Registration failed:', err);
        this.errorMessage = err.error?.errorMessage?.message || 'Eroare necunoscută.';
      }
    });
  }

  initializeGoogleSignIn() {
    if (typeof window !== 'undefined' && window.google) {
      window.google.accounts.id.initialize({
        client_id: 'YOUR_GOOGLE_CLIENT_ID', // Replace with your actual Google Client ID
        callback: (response: any) => this.handleGoogleResponse(response)
      });
    }
  }

  initializeFacebookSDK() {
    // Facebook SDK is loaded globally in index.html
    if (typeof window !== 'undefined' && window.FB) {
      console.log('Facebook SDK already loaded');
    } else {
      // Wait for SDK to load
      window.addEventListener('load', () => {
        if (window.FB) {
          console.log('Facebook SDK loaded');
        }
      });
    }
  }

  signUpWithGoogle() {
    // Check if policies are accepted
    if (!this.acceptedPolicies) {
      this.showPolicyError = true;
      return;
    }

    this.showPolicyError = false;

    if (window.google) {
      window.google.accounts.id.prompt();
    }
  }

  signUpWithFacebook() {
    // Check if policies are accepted
    if (!this.acceptedPolicies) {
      this.showPolicyError = true;
      return;
    }

    this.showPolicyError = false;

    if (window.FB) {
      window.FB.login((response: any) => {
        if (response.authResponse) {
          this.handleFacebookResponse(response.authResponse.accessToken);
        } else {
          console.error('Facebook login failed');
        }
      }, { scope: 'email,public_profile' });
    }
  }

  handleGoogleResponse(response: any) {
    const socialLoginData: SocialLoginDTO = {
      token: response.credential,
      provider: 'google'
    };

    this.authService.socialLogin(socialLoginData).subscribe({
      next: () => {
        this.router.navigate(['/home']);
      },
      error: (err) => {
        console.error('Google social login failed:', err);
        this.errorMessage = err.error?.errorMessage?.message || 'Eroare la autentificarea cu Google.';
      }
    });
  }

  handleFacebookResponse(accessToken: string) {
    const socialLoginData: SocialLoginDTO = {
      token: accessToken,
      provider: 'facebook'
    };

    this.authService.socialLogin(socialLoginData).subscribe({
      next: () => {
        this.router.navigate(['/home']);
      },
      error: (err) => {
        console.error('Facebook social login failed:', err);
        this.errorMessage = err.error?.errorMessage?.message || 'Eroare la autentificarea cu Facebook.';
      }
    });
  }

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
