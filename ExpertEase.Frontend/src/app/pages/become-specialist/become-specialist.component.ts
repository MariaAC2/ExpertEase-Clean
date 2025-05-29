import { Component } from '@angular/core';
import {BecomeSpecialistDTO, SpecialistAddDTO,} from '../../models/api.models';
import {dtoToFormFields} from '../../models/form.models';
import {ProfileService} from '../../services/profile.service';
import {Router} from '@angular/router';
import {DynamicFormComponent} from '../../shared/dynamic-form/dynamic-form.component';
import {BinaryOperatorExpr} from '@angular/compiler';

@Component({
  selector: 'app-become-specialist',
  imports: [
    DynamicFormComponent
  ],
  templateUrl: './become-specialist.component.html',
  styleUrl: './become-specialist.component.scss'
})
export class BecomeSpecialistComponent {
  defaultUser: Omit<BecomeSpecialistDTO, 'userId'> = {
    phoneNumber: '',
    address: '',
    yearsExperience: 0,
    description: '',
  };

  formData: { [key: string]: any } = {};

  addEntityFormFields = dtoToFormFields(this.defaultUser, {
    yearsExperience: { type: 'number', placeholder: 'Ex: 5' },
    description: { type: 'textarea', placeholder: 'Descrie serviciile oferite' }
  });

  isAddUserFormVisible = false;

  constructor(private profileService: ProfileService, private router: Router) {}

  ngOnInit() {
    const defaultFormValues: Omit<BecomeSpecialistDTO, 'userId'> = {
      phoneNumber: '',
      address: '',
      yearsExperience: 0,
      description: '',
    };

    this.formData = {...defaultFormValues};
  }

  addEntity(data: { [key: string]: any }) {
    const currentUserId = this.profileService.getCurrentUserId();

    const userToSubmit: BecomeSpecialistDTO = {
      userId: currentUserId,
      phoneNumber: data['phoneNumber'],
      address: data['address'],
      yearsExperience: data['yearsExperience'],
      description: data['description'],
    };

    this.profileService.becomeSpecialist(userToSubmit).subscribe({
      next: () => {
        this.closeAddUserForm();
        this.router.navigate(['/home']);
        // this.getPage(); // refresh user list
      },
      error: (err) => {
        console.error('Eroare la adăugarea utilizatorului:', err);
      }
    });
  }

  closeAddUserForm() {
    this.isAddUserFormVisible = false;
    // this.resetForm();
  }
}
