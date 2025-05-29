import {Component, EventEmitter, Input, Output} from '@angular/core';
import {FormsModule} from "@angular/forms";

@Component({
  selector: 'app-review-form',
    imports: [
        FormsModule
    ],
  templateUrl: './review-form.component.html',
  styleUrl: './review-form.component.scss'
})
export class ReviewFormComponent {
  @Input() reviewForm = {
    receiverUserId: '',
    rating: 5,
    content: '',
  };

  @Output() formSubmit = new EventEmitter<{ [key: string]: any }>();
  @Output() close = new EventEmitter<void>();

  submitReview() {
    this.formSubmit.emit(this.reviewForm);
  }

  closeForm() {
    this.close.emit();
  }
}
