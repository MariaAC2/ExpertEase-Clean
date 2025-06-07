import {Component, EventEmitter, Input, Output} from '@angular/core';
import {CommonModule, NgOptimizedImage} from '@angular/common';
import {Router, RouterLink} from '@angular/router';
import {SpecialistDTO} from '../../models/api.models';
import {RequestFormComponent} from '../request-form/request-form.component';

@Component({
  selector: 'app-specialist-card',
  imports: [CommonModule, RequestFormComponent, RouterLink, NgOptimizedImage],
  templateUrl: './specialist-card.component.html',
  styleUrls: ['./specialist-card.component.scss']
})
export class SpecialistCardComponent {
  @Input() specialist: SpecialistDTO = {
    id: '',
    fullName: '',
    email: '',
    profilePictureUrl: '',
    phoneNumber: '',
    address: '',
    categories: [],
    yearsExperience: 0,
    description: '',
    createdAt: new Date(),
    updatedAt: new Date(),
    rating: 0
  }
  isRequestFormVisible = false;

  requestForm = {
    receiverUserId: '',
    requestedStartDate: new Date(),
    phoneNumber: '',
    address: '',
    description: ''
  };

  @Output() specialistId = new EventEmitter<string>();
  @Output() requestFormSubmit = new EventEmitter<{ [key: string]: any }>();

  constructor(private readonly router: Router) { }

  goToSendRequest() {
    this.requestForm.receiverUserId = this.specialist.id;
    this.isRequestFormVisible = true;
  }

  closeRequestForm() {
    this.requestForm = {
      receiverUserId: '',
      requestedStartDate: new Date(),
      phoneNumber: '',
      address: '',
      description: ''
    };
    this.isRequestFormVisible = false;
  }

  submitRequest(data: { [p: string]: any }) {
    this.requestFormSubmit.emit(data);
    this.closeRequestForm();
  }
}
