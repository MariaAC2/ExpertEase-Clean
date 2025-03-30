import { Component } from '@angular/core';
import {FormsModule} from '@angular/forms';
import {CommonModule} from '@angular/common';
import { AuthService } from './auth.service';

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
    // confirm_password: ''
  };

  errors: { [key: string]: string } = {};

  constructor(private authService: AuthService) {}

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

    // if (this.formData.password !== this.formData.confirm_password) {
    //   this.errors['confirm_password'] = 'Parolele nu coincid.';
    // }

    if (Object.keys(this.errors).length === 0) {
      console.log('Form is valid:', this.formData);
      // proceed with submission
    } else {
      console.warn('Form errors:', this.errors);
    }

    this.authService.registerUser(this.formData).subscribe({
      next: (res) => {
        console.log('User registered!', res);
        // maybe redirect or show success message
      },
      error: (err) => {
        console.error('Registration failed:', err);
        // show error to user
      }
    });
  }

  validateEmail(email: string): boolean {
    // Basic email validation regex
    const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    return re.test(email);
  }
}

