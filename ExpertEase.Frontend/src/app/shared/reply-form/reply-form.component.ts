import {Component, EventEmitter, Input, Output} from '@angular/core';
import {SpecialistDTO} from '../../models/api.models';
import {Router} from '@angular/router';
import {FormsModule} from '@angular/forms';

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
    startDate: new Date(),
    endDate: new Date(),
    price: 0
  };

  @Output() formSubmit = new EventEmitter<{ [key: string]: any }>();
  @Output() close = new EventEmitter<void>();

  constructor(private router: Router) { }

  submitReply() {
    this.formSubmit.emit(this.replyForm);
  }

  closeForm() {
    this.close.emit();
    // this.router.navigate(['/home']);
  }
}
