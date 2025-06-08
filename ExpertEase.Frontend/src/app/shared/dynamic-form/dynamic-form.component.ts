import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import {FormsModule, NgModel, NgForm} from '@angular/forms';
import {CommonModule} from '@angular/common';
import {FormField} from '../../models/form.models';

@Component({
  selector: 'app-dynamic-form',
  standalone: true,
  templateUrl: './dynamic-form.component.html',
  styleUrls: ['./dynamic-form.component.scss'],
  imports: [CommonModule, FormsModule]
})
export class DynamicFormComponent implements OnChanges {
  @Input() fields: FormField[] = [];
  @Input() initialData: { [key: string]: any } = {};
  @Input() submitText = 'Submit';
  @Output() formSubmit = new EventEmitter<{ [key: string]: any }>();

  formData: { [key: string]: any } = {};
  formSubmitted = false;

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['initialData'] && changes['initialData'].currentValue) {
      this.formData = { ...changes['initialData'].currentValue };

      // Ensure all expected keys exist
      this.fields.forEach(field => {
        if (!(field.key in this.formData)) {
          this.formData[field.key] = '';
        }
      });
    }
  }

  onSubmit(formRef: NgForm) {
    this.formSubmitted = true;
    console.log(this.formSubmitted);

    if (formRef.invalid) return;

    this.formSubmit.emit(this.formData);
  }

  liveFormatPhoneNumber(key: string) {
    let raw = this.formData[key] || '';
    const hasPlus = raw.startsWith('+');

    // Strip all non-digit characters
    const digits = raw.replace(/\D/g, '');

    let formatted = digits;

    // Priority: +40 7XX XXX XXX
    if (digits.length === 11 && digits.startsWith('40')) {
      formatted = digits.replace(/(\d{2})(\d{3})(\d{3})(\d{3})/, '$1 $2 $3 $4');
    }
    // Fallback: 07X XXX XXXX
    else if (digits.length === 10 && digits.startsWith('0')) {
      formatted = digits.replace(/(\d{3})(\d{3})(\d{4})/, '$1 $2 $3');
    }
    // Shorter fallback: 7XX XXX XXX
    else if (digits.length === 9) {
      formatted = digits.replace(/(\d{3})(\d{3})(\d{3})/, '$1 $2 $3');
    }

    this.formData[key] = hasPlus ? `+${formatted}` : formatted;
  }
}
