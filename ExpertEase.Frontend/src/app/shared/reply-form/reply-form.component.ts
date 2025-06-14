import {Component, EventEmitter, Input, Output} from '@angular/core';
import {SpecialistDTO} from '../../models/api.models';
import {Router} from '@angular/router';
import {FormsModule, NgForm} from '@angular/forms';

@Component({
  selector: 'app-reply-form',
  imports: [
    FormsModule
  ],
  templateUrl: './reply-form.component.html',
  styleUrl: './reply-form.component.scss'
})
export class ReplyFormComponent {
  @Input() replyForm = {
    day: null as number | null,
    month: null as number | null,
    year: null as number | null,
    startHour: null as number | null,
    startMinute: null as number | null,
    endHour: null as number | null,
    endMinute: null as number | null,
    price: null as number | null
  };

  @Output() formSubmit = new EventEmitter<{ [key: string]: any }>();
  @Output() close = new EventEmitter<void>();

  formSubmitted = false;

  constructor(private router: Router) { }

  submitReply(formRef: NgForm) {
    this.formSubmitted = true;

    if (formRef.invalid) {
      // Mark all fields as touched to show validation errors
      Object.keys(formRef.controls).forEach(key => {
        formRef.controls[key].markAsTouched();
      });
      return;
    }

    // Create start and end datetime objects using the same date
    const startDateTime = this.combineDateTime('start');
    const endDateTime = this.combineDateTime('end');

    const replyData = {
      startDate: startDateTime,
      endDate: endDateTime,
      price: this.replyForm.price
    };

    this.formSubmit.emit(replyData);
  }

  private combineDateTime(timeType: 'start' | 'end'): Date {
    const day = this.replyForm.day || 1;
    const month = (this.replyForm.month || 1) - 1; // JavaScript months are 0-indexed
    const year = this.replyForm.year || new Date().getFullYear();

    const hour = timeType === 'start'
      ? (this.replyForm.startHour || 0)
      : (this.replyForm.endHour || 0);
    const minute = timeType === 'start'
      ? (this.replyForm.startMinute || 0)
      : (this.replyForm.endMinute || 0);

    const date = new Date(year, month, day, hour, minute, 0, 0);
    return date;
  }

  closeForm() {
    this.close.emit();
    // this.router.navigate(['/home']);
  }
}
