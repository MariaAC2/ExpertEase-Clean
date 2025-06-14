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

  constructor(private readonly router: Router) { }

  submitRequest(formRef: NgForm) {
    this.formSubmitted = true;

    if (formRef.invalid) {
      // Mark all fields as touched to show validation errors
      Object.keys(formRef.controls).forEach(key => {
        formRef.controls[key].markAsTouched();
      });
      return;
    }

    // Combine date and time into a single datetime before emitting
    const combinedDateTime = this.combineDateTime();
    const requestData = {
      ...this.requestForm,
      requestedStartDate: combinedDateTime
    };

    this.formSubmit.emit(requestData);
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
    this.close.emit();
  }
}
