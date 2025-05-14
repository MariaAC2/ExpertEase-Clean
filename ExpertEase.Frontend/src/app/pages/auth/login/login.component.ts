import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../../services/auth.service';
import { Router } from '@angular/router';
import { LoginDTO } from '../../../models/api.models'; // Adjust path as needed

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule
  ],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent {
  formData: LoginDTO = {
    email: '',
    password: ''
  };

  errors: { [key: string]: string } = {};
  errorMessage: string | null = null;

  constructor(private authService: AuthService, private router: Router) {}

  onSubmit(): void {
    this.errors = {};

    if (!this.formData.email.trim()) {
      this.errors['email'] = 'Adresa de email este obligatorie.';
    }

    if (!this.formData.password) {
      this.errors['password'] = 'Parola este obligatorie.';
    } else if (this.formData.password.length < 6) {
      this.errors['password'] = 'Parola trebuie să aibă minim 6 caractere.';
    }

    if (Object.keys(this.errors).length > 0) {
      console.warn('Form errors:', this.errors);
      return;
    }

    this.authService.loginUser(this.formData).subscribe({
      next: (res) => {
        console.log(res);
        console.log('Login submitted with:', this.formData);
        this.router.navigate(['/home']);
      },
      error: (err) => {
        console.error('Login failed:', err);
        this.errorMessage = err.error?.errorMessage?.message || 'Eroare necunoscută.';
        console.error('Error message:', this.errorMessage);
      }
    });
  }
}
