import { Component } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { AuthService } from '../../../services/auth.service';
import { LoginDTO } from '../../../models/api.models';
import { FormField, dtoToFormFields } from '../../../models/form.models'; // adjust path
import { DynamicFormComponent } from '../../../shared/dynamic-form/dynamic-form.component';

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

  goToRegister(): void {
    this.router.navigate(['/register']);
  }
}
