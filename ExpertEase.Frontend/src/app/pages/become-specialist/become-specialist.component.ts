import {Component, OnInit} from '@angular/core';
import {BecomeSpecialistDTO, PortfolioPictureAddDTO} from '../../models/api.models';
import {dtoToFormFields} from '../../models/form.models';
import {Router} from '@angular/router';
import {DynamicFormComponent} from '../../shared/dynamic-form/dynamic-form.component';
import {NgIf, NgSwitch, NgSwitchCase} from '@angular/common';
import {CategorySelectorComponent} from '../../shared/category-selector/category-selector.component';
import {AuthService} from '../../services/auth.service';
import {SpecialistProfileService} from '../../services/specialist-profile.service';
import {PortfolioUploadComponent} from '../../shared/portfolio-upload/portfolio-upload.component';
import {AlertComponent} from '../../shared/alert/alert.component';

@Component({
  selector: 'app-become-specialist',
  imports: [
    DynamicFormComponent,
    NgSwitch,
    NgSwitchCase,
    CategorySelectorComponent,
    PortfolioUploadComponent,
    AlertComponent,
    NgIf
  ],
  templateUrl: './become-specialist.component.html',
  styleUrl: './become-specialist.component.scss'
})
export class BecomeSpecialistComponent implements OnInit {
  step = 1;
  portfolioImages: File[] = [];
  imagePreviews: string[] = [];

  // Alert states
  showStep2BackAlert = false;
  showStep3BackAlert = false;

  specialistData: Omit<BecomeSpecialistDTO, 'userId'> = {
    yearsExperience: 0,
    phoneNumber: '',
    address: '',
    description: '',
    categories: [],
    portfolio: []
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

  constructor(private readonly authService: AuthService,
              private readonly specialistProfileService: SpecialistProfileService,
              private readonly router: Router) {
  }

  ngOnInit() {
    const defaultFormValues: Omit<BecomeSpecialistDTO, 'userId'> = {
      phoneNumber: '',
      address: '',
      yearsExperience: 0,
      description: '',
    };

    this.formData = {...defaultFormValues};
  }

  // Step 1 handlers
  handleStep1(data: any) {
    this.specialistData = {
      ...this.specialistData,
      ...data
    };
    this.step = 2;
  }

  // Step 2 handlers
  updateCategories(categories: string[]) {
    this.specialistData.categories = categories;
  }

  handleStep2() {
    if (!this.specialistData.categories || this.specialistData.categories.length === 0) {
      // You could show an alert here if no categories are selected
      console.warn('No categories selected');
      return;
    }
    this.step = 3;
  }

  // Step 3 handlers
  updatePortfolio(portfolio: PortfolioPictureAddDTO[]) {
    this.portfolioImages = portfolio.map(p => p.fileStream);
    this.imagePreviews = portfolio.map(p => URL.createObjectURL(p.fileStream));
    this.specialistData.portfolio = portfolio;
  }

  // Unified back button logic
  showBackConfirmation() {
    if (this.step === 2) {
      this.showStep2BackAlert = true;
    } else if (this.step === 3) {
      this.showStep3BackAlert = true;
    }
  }

  getAlertMessage(): string {
    if (this.showStep2BackAlert) {
      return 'Sigur vrei să te întorci la pasul anterior? Categoriile selectate vor fi pierdute.';
    } else if (this.showStep3BackAlert) {
      return 'Sigur vrei să te întorci la pasul anterior? Toate imaginile adăugate vor fi pierdute.';
    }
    return '';
  }

  handleAlertConfirm() {
    if (this.showStep2BackAlert) {
      this.goBackToStep1();
    } else if (this.showStep3BackAlert) {
      this.goBackToStep2();
    }
  }

  handleAlertCancel() {
    this.showStep2BackAlert = false;
    this.showStep3BackAlert = false;
  }

  goBackToStep1() {
    // Clear categories when going back to step 1
    this.specialistData.categories = [];
    this.showStep2BackAlert = false;
    this.step = 1;
  }

  goBackToStep2() {
    // Clear portfolio data when going back to step 2
    this.specialistData.portfolio = [];
    this.portfolioImages = [];
    this.imagePreviews = [];
    this.showStep3BackAlert = false;
    this.step = 2;
  }

  // Final submission
  addEntity() {
    const currentUserId = this.authService.getUserId();

    if (!currentUserId) {
      console.error('User ID not found');
      return;
    }

    const userToSubmit: BecomeSpecialistDTO = {
      userId: currentUserId,
      phoneNumber: this.specialistData.phoneNumber,
      address: this.specialistData.address,
      yearsExperience: this.specialistData.yearsExperience,
      description: this.specialistData.description,
      categories: this.specialistData.categories
    };

    this.specialistProfileService.becomeSpecialist(userToSubmit).subscribe({
      next: () => {
        this.closeAddUserForm();
        this.router.navigate(['/home']);
      },
      error: (err) => {
        console.error('Eroare la adăugarea utilizatorului:', err);
      }
    });
  }

  closeAddUserForm() {
    this.isAddUserFormVisible = false;
  }
}
