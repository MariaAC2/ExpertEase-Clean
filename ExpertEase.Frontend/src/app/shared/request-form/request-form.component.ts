import { Component } from '@angular/core';
import {FormsModule, ReactiveFormsModule} from "@angular/forms";
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
  request= {
    requestedStartDate: new Date(),
    phoneNumber: '0723123456',
    address: 'Str. Teiului nr. 14, București',
    description: 'Solicit instalarea unei chiuvete noi în bucătărie. Este deja cumpărată, trebuie doar montată.'
  };

  constructor(private router: Router) { }

  submitRequest() {
    this.router.navigate(['/messages']);
  }
}
