import {Component, EventEmitter, Input, Output} from '@angular/core';
import {FormsModule, NgForm, ReactiveFormsModule} from "@angular/forms";
import {CommonModule} from '@angular/common';
import {Router} from '@angular/router';

@Component({
  selector: 'app-request-form',
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule
  ],
  templateUrl: './request-form.component.html',
  styleUrl: './request-form.component.scss'
})
export class RequestFormComponent {
  @Input() requestForm = {
    receiverUserId: '',
    day: null as number | null,
    month: null as number | null,
    year: null as number | null,
    startHour: null as number | null,
    startMinute: null as number | null,
    phoneNumber: '',
    address: '',
    description: ''
  };

  @Output() formSubmit = new EventEmitter<{ [key: string]: any }>();
  @Output() close = new EventEmitter<void>();

  formSubmitted = false;
  isLoading = false;
  validationErrors: { [key: string]: string } = {};

  constructor(private readonly router: Router) { }

  submitRequest(formRef: NgForm) {
    this.formSubmitted = true;
    this.validationErrors = {};

    // Perform custom validations
    this.validateForm();

    if (formRef.invalid || Object.keys(this.validationErrors).length > 0) {
      // Mark all fields as touched to show validation errors
      Object.keys(formRef.controls).forEach(key => {
        formRef.controls[key].markAsTouched();
      });
      return;
    }

    // Start loading
    this.isLoading = true;

    // Combine date and time into a single datetime before emitting
    const combinedDateTime = this.combineDateTime();
    const requestData = {
      ...this.requestForm,
      requestedStartDate: combinedDateTime
    };

    // Simulate API call with timeout
    setTimeout(() => {
      this.formSubmit.emit(requestData);
      this.isLoading = false;

      // Navigate to messages after successful submission
      this.router.navigate(['/messages']).then(() => {
        this.closeForm();
      });
    }, 2000); // 2 second loading simulation
  }

  private validateForm(): void {
    const errors: { [key: string]: string } = {};

    // Validate date is in the future
    if (this.requestForm.day && this.requestForm.month && this.requestForm.year &&
      this.requestForm.startHour !== null && this.requestForm.startMinute !== null) {

      const selectedDate = this.combineDateTime();
      const now = new Date();

      if (selectedDate <= now) {
        errors['datetime'] = 'Data și ora trebuie să fie în viitor.';
      }

      // Check if date is too far in the future (more than 1 year)
      const oneYearFromNow = new Date();
      oneYearFromNow.setFullYear(oneYearFromNow.getFullYear() + 1);

      if (selectedDate > oneYearFromNow) {
        errors['datetime'] = 'Data nu poate fi mai mult de un an în viitor.';
      }

      // Validate working hours (8 AM to 8 PM)
      const hour = this.requestForm.startHour!;
      if (hour < 8 || hour > 20) {
        errors['time'] = 'Ora trebuie să fie între 08:00 și 20:00.';
      }
    }

    // Validate phone number format (Romanian format)
    if (this.requestForm.phoneNumber) {
      const phoneRegex = /^(\+40|0)(7[0-9]{8}|[23][0-9]{7})$/;
      if (!phoneRegex.test(this.requestForm.phoneNumber.replace(/\s/g, ''))) {
        errors['phoneNumber'] = 'Numărul de telefon nu este valid. Folosiți formatul românesc (ex: 0723123456).';
      }
    }

    // Validate address minimum length
    if (this.requestForm.address && this.requestForm.address.trim().length < 10) {
      errors['address'] = 'Adresa trebuie să conțină cel puțin 10 caractere.';
    }

    // Validate description minimum length
    if (this.requestForm.description && this.requestForm.description.trim().length < 20) {
      errors['description'] = 'Descrierea trebuie să conțină cel puțin 20 de caractere.';
    }

    // Validate date fields individually
    if (this.requestForm.day !== null) {
      const month = this.requestForm.month || 1;
      const year = this.requestForm.year || new Date().getFullYear();
      const daysInMonth = new Date(year, month, 0).getDate();

      if (this.requestForm.day < 1 || this.requestForm.day > daysInMonth) {
        errors['day'] = `Ziua trebuie să fie între 1 și ${daysInMonth} pentru luna selectată.`;
      }
    }

    this.validationErrors = errors;
  }

  private combineDateTime(): Date {
    const day = this.requestForm.day || 1;
    const month = (this.requestForm.month || 1) - 1; // JavaScript months are 0-indexed
    const year = this.requestForm.year || new Date().getFullYear();
    const hour = this.requestForm.startHour || 0;
    const minute = this.requestForm.startMinute || 0;

    const date = new Date(year, month, day, hour, minute, 0, 0);
    return date;
  }

  closeForm() {
    if (!this.isLoading) {
      this.close.emit();
    }
  }

  // Helper method to get current year for default min value
  getCurrentYear(): number {
    return new Date().getFullYear();
  }

  // Helper method to format phone number as user types
  formatPhoneNumber(): void {
    if (this.requestForm.phoneNumber) {
      // Remove all non-digit characters except +
      let cleaned = this.requestForm.phoneNumber.replace(/[^\d+]/g, '');

      // Format Romanian phone number
      if (cleaned.startsWith('+40')) {
        cleaned = cleaned.slice(3);
        this.requestForm.phoneNumber = '+40 ' + this.formatDigits(cleaned);
      } else if (cleaned.startsWith('40')) {
        cleaned = cleaned.slice(2);
        this.requestForm.phoneNumber = '+40 ' + this.formatDigits(cleaned);
      } else if (cleaned.startsWith('0')) {
        this.requestForm.phoneNumber = this.formatDigits(cleaned);
      } else if (cleaned.length > 0) {
        this.requestForm.phoneNumber = '0' + this.formatDigits(cleaned);
      }
    }
  }

  private formatDigits(digits: string): string {
    // Format as 0XXX XXX XXX
    if (digits.length <= 4) return digits;
    if (digits.length <= 7) return digits.slice(0, 4) + ' ' + digits.slice(4);
    return digits.slice(0, 4) + ' ' + digits.slice(4, 7) + ' ' + digits.slice(7, 10);
  }
}
