import {Component, EventEmitter, Input, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {Router} from '@angular/router';
import {SpecialistDTO} from '../../models/api.models';
import {SpecialistDetailsComponent} from '../specialist-details/specialist-details.component';
import {RequestFormComponent} from '../request-form/request-form.component';

@Component({
  selector: 'app-specialist-card',
  imports: [CommonModule, SpecialistDetailsComponent, RequestFormComponent],
  templateUrl: './specialist-card.component.html',
  styleUrls: ['./specialist-card.component.scss']
})
export class SpecialistCardComponent {
  @Input() specialist: SpecialistDTO = {
    id: '',
    fullName: '',
    email: '',
    phoneNumber: '',
    address: '',
    categories: [],
    yearsExperience: 0,
    description: '',
    createdAt: new Date(),
    updatedAt: new Date(),
  }

  isUserDetailsVisible = false;
  isRequestFormVisible = false;

  requestForm = {
    receiverUserId: '',
    requestedStartDate: new Date(),
    phoneNumber: '',
    address: '',
    description: ''
  };

  @Output() viewDetails = new EventEmitter<SpecialistDTO>();
  @Output() requestFormSubmit = new EventEmitter<{ [key: string]: any }>();

  constructor(private router: Router) { }

  goToDetails() {
    this.viewDetails.emit(this.specialist);
    this.isUserDetailsVisible = true;
  }

  closeDetails() {
    this.isUserDetailsVisible = false;
  }

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
