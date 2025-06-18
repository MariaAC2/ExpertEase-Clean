import { Component } from '@angular/core';
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
export class LoginComponent {
  formFields: FormField[] = [];
  formData: { [key: string]: any } = {};
  errorMessage: string | null = null;

  constructor(private authService: AuthService, private router: Router) {
    const dto: LoginDTO = {
      email: '',
      password: ''
    };

    this.formFields = dtoToFormFields(dto, {
      email: { type: 'email' },
      password: { type: 'password' }
    });

    this.initializeGoogleSignIn();
    this.initializeFacebookSDK();
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

  initializeGoogleSignIn() {
    if (typeof window !== 'undefined' && window.google) {
      window.google.accounts.id.initialize({
        client_id: '760852864614-0l1qdais39snht0oo3511r0tpbjdj09f.apps.googleusercontent.com', // Replace with your actual Google Client ID
        callback: (response: any) => this.handleGoogleResponse(response)
      });
    }
  }

  initializeFacebookSDK() {
    if (typeof window !== 'undefined') {
      (window as any).fbAsyncInit = () => {
        window.FB.init({
          appId: '734399149543740', // Replace with your actual Facebook App ID
          cookie: true,
          xfbml: true,
          version: 'v18.0'
        });
      };

      // Load Facebook SDK
      const script = document.createElement('script');
      script.async = true;
      script.defer = true;
      script.crossOrigin = 'anonymous';
      script.src = 'https://connect.facebook.net/ro_RO/sdk.js';
      document.head.appendChild(script);
    }
  }

  signInWithGoogle() {
    if (window.google) {
      window.google.accounts.id.prompt();
    }
  }

  signInWithFacebook() {
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

  goToRegister(): void {
    this.router.navigate(['/register']);
  }
}
