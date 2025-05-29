import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import {LoginDTO, UserRegisterDTO} from '../../../models/api.models';
import { FormField, dtoToFormFields } from '../../../models/form.models'; // adjust path
import { DynamicFormComponent } from '../../../shared/dynamic-form/dynamic-form.component';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    DynamicFormComponent
  ],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss']
})
export class RegisterComponent {
  formFields: FormField[] = [];
  formData: { [key: string]: any } = {};
  errorMessage: string | null = null;

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
  }

  registerUser(data: { [key: string]: any }) {
    const userDto: UserRegisterDTO = data as UserRegisterDTO;

    this.authService.registerUser(userDto).subscribe({
      next: () => {
        const loginDto: LoginDTO = {
          email: userDto.email,
          password: userDto.password
        };

        // Now correctly call and subscribe to the login method
        this.authService.loginUser(loginDto).subscribe({
          next: () => {
            this.router.navigate(['/home']); // Move here to ensure it's after successful login
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

  goToLogin(): void {
    this.router.navigate(['/login']);
  }
}
