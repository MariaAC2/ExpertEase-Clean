import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {RequestDTO, StatusEnum} from '../../models/api.models';
import {DatePipe, LowerCasePipe, NgClass, NgIf} from '@angular/common';
import {AuthService} from '../../services/auth.service';
import {RequestService} from '../../services/request.service';
import { Timestamp } from 'firebase/firestore';

@Component({
  selector: 'app-request-message',
  imports: [
    DatePipe,
    NgIf,
    NgClass
  ],
  templateUrl: './request-message.component.html',
  styleUrl: './request-message.component.scss'
})
export class RequestMessageComponent implements OnInit {
  @Input() request: RequestDTO = {
    id: '',
    senderId: '',
    requestedStartDate: new Date(),
    description: '',
    status: StatusEnum.Pending,
    senderPhoneNumber: '',
    senderAddress: ''
  };

  @Input() currentUserId: string | null | undefined; // Add this input

  userRole: string | null = '';

  @Output() requestAccepted = new EventEmitter<string>();
  @Output() requestRejected = new EventEmitter<string>();
  @Output() makeOffer = new EventEmitter<string>();
  constructor(
    private readonly authService: AuthService,
  ) {}

  ngOnInit(): void {
    this.userRole = this.authService.getUserRole();
    console.log(this.request);
  }

  acceptRequest() {
    this.requestAccepted.emit(this.request.id);
  }

  rejectRequest() {
    this.requestRejected.emit(this.request.id);
  }

  showOfferButton(): boolean {
    return this.request.status === StatusEnum.Accepted && this.userRole === 'Specialist';
  }

  triggerOffer() {
    console.log('Fac o oferta pentru cererea:', this.request.id);
    this.makeOffer.emit(this.request.id);
  }

  get isOwnMessage(): boolean {
    return this.request.senderId === this.currentUserId;
  }
}
