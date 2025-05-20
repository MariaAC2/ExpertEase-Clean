import {Component, EventEmitter, Input, Output} from '@angular/core';
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
import {CommonModule} from '@angular/common';
import {Router} from '@angular/router';
import {RequestAddDTO} from '../../models/api.models';

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
  // request= {
  //   requestedStartDate: new Date(),
  //   phoneNumber: '0723123456',
  //   address: 'Str. Teiului nr. 14, București',
  //   description: 'Solicit instalarea unei chiuvete noi în bucătărie. Este deja cumpărată, trebuie doar montată.'
  // };
  @Input() request: RequestAddDTO = {
    receiverUserId: '',
    requestedStartDate: new Date(),
    phoneNumber: '',
    address: '',
    description: ''
  };

  @Output() formSubmit = new EventEmitter<RequestAddDTO>();

  constructor(private router: Router) { }

  submitRequest() {
    this.formSubmit.emit(this.request);
    this.router.navigate(['/messages']);
  }
}
