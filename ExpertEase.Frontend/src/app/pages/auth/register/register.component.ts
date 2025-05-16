// import { Component } from '@angular/core';
// import { FormsModule } from '@angular/forms';
// import { CommonModule } from '@angular/common';
// import { Router } from '@angular/router';
// import { AuthService } from '../../../services/auth.service';
// import { UserRegisterDTO } from '../../../models/api.models'; // adjust path if needed
//
// @Component({
//   selector: 'app-register',
//   standalone: true,
//   imports: [
//     CommonModule,
//     FormsModule
//   ],
//   templateUrl: './register.component.html',
//   styleUrls: ['./register.component.scss']
// })
// export class RegisterComponent {
//   formData: UserRegisterDTO = {
//     firstName: '',
//     lastName: '',
//     email: '',
//     password: ''
//   };
//
//   errors: { [key: string]: string } = {};
//   errorMessage: string | null = null;
//
//   constructor(private authService: AuthService, private router: Router) {}
//
//   onSubmit(): void {
//     this.errors = {}; // clear previous errors
//
//     // Validate input
//     if (!this.formData.lastName.trim()) {
//       this.errors['lastName'] = 'Numele este obligatoriu.';
//     }
//
//     if (!this.formData.firstName.trim()) {
//       this.errors['firstName'] = 'Prenumele este obligatoriu.';
//     }
//
//     if (!this.formData.email.trim()) {
//       this.errors['email'] = 'Adresa de email este obligatorie.';
//     } else if (!this.validateEmail(this.formData.email)) {
//       this.errors['email'] = 'Email invalid.';
//     }
//
//     if (!this.formData.password) {
//       this.errors['password'] = 'Parola este obligatorie.';
//     } else if (this.formData.password.length < 6) {
//       this.errors['password'] = 'Parola trebuie să aibă minim 6 caractere.';
//     }
//
//     if (Object.keys(this.errors).length > 0) {
//       console.warn('Form errors:', this.errors);
//       return;
//     }
//
//     // Send request
//     this.authService.registerUser(this.formData).subscribe({
//       next: (res) => {
//         console.log('User registered!', res);
//         this.router.navigate(['/home']);
//       },
//       error: (err) => {
//         console.error('Registration failed:', err);
//         this.errorMessage = err.error?.errorMessage?.message || 'Eroare necunoscută.';
//       }
//     });
//   }
//
//   validateEmail(email: string): boolean {
//     const re = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
//     return re.test(email);
//   }
//
//   goToLogin(): void {
//     this.router.navigate(['/login']);
//   }
// }


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
        this.authService.loginUser(userDto);
        this.router.navigate(['/home']);
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
