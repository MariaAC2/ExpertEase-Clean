import { Component } from '@angular/core';
import {FormsModule} from "@angular/forms";
import {CommonModule} from "@angular/common";
import {AuthService} from '../auth.service';
import {Router} from '@angular/router';

@Component({
  selector: 'app-login',
  imports: [
      CommonModule,
      FormsModule,
  ],
  templateUrl: './login.component.html',
  styleUrl: './login.component.scss'
})
export class LoginComponent {
  formData = {
    email: '',
    password: ''
  };

  errors: { [key: string]: string } = {};

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit() {
    this.errors = {};

    if (!this.formData.email.trim()) {
      this.errors['email'] = 'Adresa de email este obligatorie.';
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

    this.authService.loginUser(this.formData).subscribe({
      next: (res) => {
        console.log(res);
        console.log('Login submitted with:', this.formData);
        this.router.navigate(['/home']);
        // maybe redirect or show success message
      },
      error: (err) => {
        console.error('Login failed:', err);
        // show error to user
      }
    });
  }
}
