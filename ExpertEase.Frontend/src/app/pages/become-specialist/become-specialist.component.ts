import { Component } from '@angular/core';
import {BecomeSpecialistDTO, SpecialistAddDTO,} from '../../models/api.models';
import {dtoToFormFields} from '../../models/form.models';
import {ProfileService} from '../../services/profile.service';
import {Router} from '@angular/router';
import {DynamicFormComponent} from '../../shared/dynamic-form/dynamic-form.component';
import {BinaryOperatorExpr} from '@angular/compiler';
import {NgSwitch, NgSwitchCase} from '@angular/common';
import {CategorySelectorComponent} from '../../shared/category-selector/category-selector.component';

@Component({
  selector: 'app-become-specialist',
  imports: [
    DynamicFormComponent,
    NgSwitch,
    NgSwitchCase,
    CategorySelectorComponent
  ],
  templateUrl: './become-specialist.component.html',
  styleUrl: './become-specialist.component.scss'
})
export class BecomeSpecialistComponent {
  step = 1;
  specialistData: Omit<BecomeSpecialistDTO, 'userId'> = {
    yearsExperience: 0,
    phoneNumber: '',
    address: '',
    description: '',
    categories: []
  };

  formData: { [key: string]: any } = {};

  step1FormFields = dtoToFormFields({
    yearsExperience: 0,
    phoneNumber: '',
    address: '',
    description: '',
  }, {
    address: { type: 'text', class: 'full-width' },
    yearsExperience: { type: 'number', placeholder: 'Ex: 5' },
    description: { type: 'textarea', placeholder: 'Descrie serviciile oferite', class: 'full-width' }
  });

  isAddUserFormVisible = false;

  constructor(private profileService: ProfileService, private router: Router) {
    const defaultFormValues: Omit<BecomeSpecialistDTO, 'userId'> = {
      phoneNumber: '',
      address: '',
      yearsExperience: 0,
      description: '',
    };

    this.formData = {...defaultFormValues};
  }

  handleStep1(data: any) {
    this.specialistData = {
      ...this.specialistData,
      ...data
    };
    this.step = 2;
  }

  updateCategories(categories: string[]) {
    this.specialistData.categories = categories;
  }

  addEntity() {
    const currentUserId = this.profileService.getCurrentUserId();

    const userToSubmit: BecomeSpecialistDTO = {
      userId: currentUserId,
      phoneNumber: this.specialistData.phoneNumber,
      address: this.specialistData.address,
      yearsExperience: this.specialistData.yearsExperience,
      description: this.specialistData.description,
      categories: this.specialistData.categories
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
