import {Component, Input} from '@angular/core';
import {SpecialistDTO} from '../../models/api.models';

@Component({
  selector: 'app-specialist-details',
  imports: [],
  templateUrl: './specialist-details.component.html',
  styleUrl: './specialist-details.component.scss'
})
export class SpecialistDetailsComponent {
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
  };
}
