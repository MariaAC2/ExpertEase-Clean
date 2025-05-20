import { Component } from '@angular/core';
import {RequestDTO, StatusEnum} from '../../models/api.models';
import {DatePipe, LowerCasePipe, NgClass} from '@angular/common';

@Component({
  selector: 'app-request-message',
  imports: [
    DatePipe,
    LowerCasePipe,
    NgClass
  ],
  templateUrl: './request-message.component.html',
  styleUrl: './request-message.component.scss'
})
export class RequestMessageComponent {
  dummy_request_details: RequestDTO = {
    id: 'e2d9f210-aed4-4b8b-b19f-23cf5c7f1b99',
    requestedStartDate: new Date('2025-05-22T10:30:00'),
    description: 'Solicit instalarea unei chiuvete noi în bucătărie. Este deja cumpărată, trebuie doar montată.',
    status: StatusEnum.Pending, // or StatusEnum.Pending if using enum reference
    senderContactInfo: {
      phoneNumber: '0722123456',
      address: 'Str. Teiului nr. 14, București'
    },
  };
  currentUserEmail: string | undefined;
}
