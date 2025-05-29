import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {RequestDTO, StatusEnum} from '../../models/api.models';
import {DatePipe, LowerCasePipe, NgClass, NgIf} from '@angular/common';
import {AuthService} from '../../services/auth.service';
import {SpecialistRequestService} from '../../services/specialist.request.service';

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
    senderUserId: '',
    receiverUserId: '',
    requestedStartDate: new Date(),
    description: '',
    status: StatusEnum.Pending,
    senderContactInfo: {
      phoneNumber: '',
      address: ''
    }
  };

  userRole: string | null = '';

  @Output() requestAccepted = new EventEmitter<string>();
  @Output() requestRejected = new EventEmitter<string>();
  @Output() makeOffer = new EventEmitter<string>();
  constructor(
    private authService: AuthService,
    private specialistRequestService: SpecialistRequestService
  ) {}

  ngOnInit() {
    this.userRole = this.authService.getUserRole(); // 'Client' or 'Specialist'
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
    this.makeOffer.emit(this.request.id);
  }

}
