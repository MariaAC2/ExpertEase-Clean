import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import {FormsModule} from '@angular/forms';
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

  onSubmit(): void {
    this.formSubmit.emit(this.formData);
  }
}
