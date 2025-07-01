import {Component, EventEmitter, Input, Output} from '@angular/core';
import {CommonModule, NgOptimizedImage} from '@angular/common';
import {Router, RouterLink} from '@angular/router';
import {RequestAddDTO, SpecialistDTO} from '../../models/api.models';
import {RequestFormComponent} from '../request-form/request-form.component';

@Component({
  selector: 'app-specialist-card',
  imports: [CommonModule, RequestFormComponent, RouterLink],
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
  };

  isRequestFormVisible = false;

  requestForm = {
    receiverUserId: '',
    day: null as number | null,
    month: null as number | null,
    year: null as number | null,
    startHour: null as number | null,
    startMinute: null as number | null,
    phoneNumber: '',
    address: '',
    description: ''
  };

  @Output() specialistId = new EventEmitter<string>();
  @Output() requestFormSubmit = new EventEmitter<RequestAddDTO>();

  constructor(private readonly router: Router) { }

  // Navigate to specialist details with overlay parameter
  navigateToDetails() {
    this.router.navigate(['/specialist', this.specialist.id], {
      queryParams: { overlay: 'true' }
    });
  }

  goToSendRequest() {
    // Set the receiverUserId when opening the request form
    this.requestForm.receiverUserId = this.specialist.id;
    this.isRequestFormVisible = true;
  }

  closeRequestForm() {
    // Reset the form data when closing
    this.requestForm = {
      receiverUserId: '',
      day: null,
      month: null,
      year: null,
      startHour: null,
      startMinute: null,
      phoneNumber: '',
      address: '',
      description: ''
    };
    this.isRequestFormVisible = false;
  }

  private transformFormDataToRequestDTO(formData: { [key: string]: any }): RequestAddDTO {
    // Combine date and time into a single Date object
    const requestedStartDate = this.combineDateTime(formData);

    return {
      receiverUserId: formData['receiverUserId'],
      requestedStartDate: requestedStartDate,
      phoneNumber: formData['phoneNumber'],
      address: formData['address'],
      description: formData['description']
    };
  }

  /**
   * Combine date and time fields into a Date object
   */
  private combineDateTime(formData: { [key: string]: any }): Date {
    const day = formData['day'] || 1;
    const month = (formData['month'] || 1) - 1; // JavaScript months are 0-indexed
    const year = formData['year'] || new Date().getFullYear();
    const hour = formData['startHour'] || 0;
    const minute = formData['startMinute'] || 0;

    return new Date(year, month, day, hour, minute, 0, 0);
  }

  submitRequest(data: { [key: string]: any }) {
    // Emit the RequestAddDTO to parent component
    const requestData = this.transformFormDataToRequestDTO(data);
    this.requestFormSubmit.emit(requestData);
    this.closeRequestForm();
  }
}
