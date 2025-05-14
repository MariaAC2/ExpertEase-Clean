import { Component } from '@angular/core';
import {FormsModule} from '@angular/forms';
import {CommonModule} from '@angular/common';
import { AuthService } from '../../../services/auth.service';
import {Router} from '@angular/router';

@Component({
  selector: 'app-register',
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './register.component.html',
  styleUrl: './register.component.scss'
})
export class RegisterComponent {
  formData = {
    firstName: '',
    lastName: '',
    email: '',
    password: ''
  };

  errors: { [key: string]: string } = {};
  errorMessage: string | null = null;

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit() {
    this.errors = {}; // clear previous errors

    if (!this.formData.lastName.trim()) {
      this.errors['lastName'] = 'Numele este obligatoriu.';
    }

    if (!this.formData.firstName.trim()) {
      this.errors['firstName'] = 'Prenumele este obligatoriu.';
    }

    if (!this.formData.email.trim()) {
      this.errors['email'] = 'Adresa de email este obligatorie.';
    } else if (!this.validateEmail(this.formData.email)) {
      this.errors['email'] = 'Email invalid.';
    }

    if (!this.formData.password) {
      this.errors['password'] = 'Parola este obligatorie.';
    } else if (this.formData.password.length < 6) {
      this.errors['password'] = 'Parola trebuie să aibă minim 6 caractere.';
    }

    if (Object.keys(this.errors).length === 0) {
      console.log('Form is valid:', this.formData);
      // proceed with submission
    } else {
      console.warn('Form errors:', this.errors);
    }

    this.authService.registerUser(this.formData).subscribe({
      next: (res) => {
        console.log('User registered!', res);
        this.router.navigate(['/home']);
        // maybe redirect or show success message
      },
      error: (err) => {
        console.error('Registration failed:', err);
        this.errorMessage = err.error?.errorMessage?.message;
        console.error('Error message:', this.errorMessage);
        // show error to user
      }
    });
    // const simulatedError = {
    //   error: {
    //     errorMessage: {
    //       message: 'The user already exists!',
    //       code: 'UserAlreadyExists',
    //       status: 'Conflict'
    //     }
    //   }
    // };
    //
    // // simulate error handling logic as if it came from HttpClient
    // this.errorMessage = simulatedError.error?.errorMessage?.message;
    // console.error('Simulated error message:', this.errorMessage);
  }

  validateEmail(email: string): boolean {
    // Basic email validation regex
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
  }

  goToLogin() {
    this.router.navigate(['/login']);
  }
}

