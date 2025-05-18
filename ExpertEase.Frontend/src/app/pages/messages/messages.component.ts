import {Component} from '@angular/core';
import {RequestAddDTO, RequestDTO, StatusEnum} from '../../models/api.models';
import {CommonModule} from '@angular/common';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {RequestMessageComponent} from '../../shared/request-message/request-message.component';

@Component({
  selector: 'app-messages',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, ReactiveFormsModule, RequestMessageComponent],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.scss'
})
export class MessagesComponent {
  request = {
    requestedStartDate: '',
    phoneNumber: '',
    address: '',
    description: ''
  };

  dummyRequest: RequestAddDTO = {
    receiverUserId: 'a3c2b8b1-38fd-4c7f-a258-9e31bb215999',
    requestedStartDate: new Date(),
    phoneNumber: "0723123456",
    address: "Str. Lalelelor nr. 42, București",
    description: "Am o țeavă spartă sub chiuvetă care trebuie reparată urgent. Apa curge constant."
  };
  dummy_request_details: RequestDTO = {
    id: 'e2d9f210-aed4-4b8b-b19f-23cf5c7f1b99',
    requestedStartDate: new Date('2025-05-22T10:30:00'),
    description: 'Solicit instalarea unei chiuvete noi în bucătărie. Este deja cumpărată, trebuie doar montată.',
    status: StatusEnum.Pending, // or StatusEnum.Pending if using enum reference
    senderUser: {
      firstName: 'Andrei',
      lastName: 'Popescu',
      email: 'andrei.popescu@example.com',
      phoneNumber: '0722123456',
      address: 'Str. Teiului nr. 14, București'
    },
    receiverUser: {
      firstName: 'Ioana',
      lastName: 'Dumitrescu',
      email: 'ioana.dumitrescu@expertease.ro',
      phoneNumber: '0733344556',
      address: 'Str. Independenței nr. 20, București'
    },
    rejectedAt: undefined
  };

  submitRequest() {
    console.log('S-a trimis');
  }
}
