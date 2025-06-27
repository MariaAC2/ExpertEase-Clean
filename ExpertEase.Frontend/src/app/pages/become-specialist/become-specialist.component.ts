import {Component, OnInit} from '@angular/core';
import {BecomeSpecialistDTO} from '../../models/api.models';
import {dtoToFormFields} from '../../models/form.models';
import {Router} from '@angular/router';
import {DynamicFormComponent} from '../../shared/dynamic-form/dynamic-form.component';
import {NgIf, NgSwitch, NgSwitchCase} from '@angular/common';
import {CategorySelectorComponent} from '../../shared/category-selector/category-selector.component';
import {AuthService} from '../../services/auth.service';
import {SpecialistProfileService} from '../../services/specialist-profile.service';
import {PortfolioUploadComponent} from '../../shared/portfolio-upload/portfolio-upload.component';
import {AlertComponent} from '../../shared/alert/alert.component';
import {StripeAccountService} from '../../services/stripe-account.service';

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

  // New properties for Stripe activation
  stripeAccountId = '';
  isActivatingStripe = false;
  stripeActivationError = '';

  // Alert states
  showStep2BackAlert = false;
  showStep3BackAlert = false;
  showStep4BackAlert = false;

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

  constructor(
    private readonly authService: AuthService,
    private readonly specialistProfileService: SpecialistProfileService,
    private readonly stripeAccountService: StripeAccountService,
    private readonly router: Router
  ) {}

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
    this.step = 3;
  }

  skipCategories() {
    // Clear any selected categories and move to next step
    this.specialistData.categories = [];
    this.step = 3;
  }

  // Step 3 handlers - Updated to handle File objects instead of DTOs
  updatePortfolio(files: File[]) {
    this.portfolioImages = files;
    // Clean up old previews
    this.imagePreviews.forEach(url => URL.revokeObjectURL(url));
    // Create new previews
    this.imagePreviews = files.map(file => URL.createObjectURL(file));
  }

  handleStep3() {
    // Move to Stripe activation step
    this.step = 4;
  }

  skipPortfolio() {
    // Clear any uploaded portfolio and move to next step
    this.portfolioImages = [];
    // Clean up previews
    this.imagePreviews.forEach(url => URL.revokeObjectURL(url));
    this.imagePreviews = [];
    this.step = 4;
  }

  // Step 4 - Stripe Account Activation - Updated to use FormData
  async activateStripeAccount() {
    this.isActivatingStripe = true;
    this.stripeActivationError = '';

    try {
      const currentUserId = this.authService.getUserId();
      if (!currentUserId) {
        throw new Error('User ID not found');
      }

      // Create the specialist profile using the new FormData approach
      const response = await this.createSpecialistProfileWithFormData(currentUserId);

      if (!response?.response) {
        throw new Error(response?.errorMessage?.message || 'Eroare la crearea profilului');
      }

      // Get the Stripe account ID from the response
      this.stripeAccountId = response.response?.stripeAccountId || '';

      console.log('Retrieved stripeAccountId:', this.stripeAccountId);

      if (!this.stripeAccountId) {
        throw new Error('Stripe account ID not found in response');
      }

      // Generate onboarding link
      this.stripeAccountService.generateOnboardingLink(this.stripeAccountId).subscribe({
        next: (linkResponse) => {
          console.log('Onboarding link response:', linkResponse);

          if (linkResponse?.response && linkResponse.response?.url) {
            // Redirect to Stripe onboarding
            console.log('Redirecting to Stripe onboarding:', linkResponse.response.url);
            window.location.href = linkResponse.response.url;
          } else {
            throw new Error(linkResponse?.errorMessage?.message || 'Eroare la generarea link-ului de activare');
          }
        },
        error: (linkError) => {
          console.error('Error generating onboarding link:', linkError);
          this.stripeActivationError = linkError.message || linkError.error?.message || 'Eroare la generarea link-ului de activare';
          this.isActivatingStripe = false;
        }
      });

    } catch (error: any) {
      console.error('Error in activateStripeAccount:', error);
      this.stripeActivationError = error.message || 'Eroare la activarea contului Stripe';
      this.isActivatingStripe = false;
    }
  }

  // Helper method to create specialist profile with FormData
  private createSpecialistProfileWithFormData(userId: string): Promise<any> {
    return new Promise((resolve, reject) => {
      // Use the updated service method that expects separated data and files
      this.specialistProfileService.becomeSpecialist(
        {
          userId: userId,
          phoneNumber: this.specialistData.phoneNumber,
          address: this.specialistData.address,
          yearsExperience: this.specialistData.yearsExperience,
          description: this.specialistData.description,
          categories: this.specialistData.categories
        },
        this.portfolioImages // Pass files separately
      ).subscribe({
        next: (response) => {
          console.log('Specialist profile created:', response);
          resolve(response);
        },
        error: (error) => {
          console.error('Error creating specialist profile:', error);
          reject(error);
        }
      });
    });
  }

  // Option to skip Stripe activation
  skipStripeActivation() {
    this.createSpecialistProfileOnly();
  }

  private createSpecialistProfileOnly() {
    const currentUserId = this.authService.getUserId();
    if (!currentUserId) {
      console.error('User ID not found');
      return;
    }

    this.specialistProfileService.becomeSpecialist(
      {
        userId: currentUserId,
        phoneNumber: this.specialistData.phoneNumber,
        address: this.specialistData.address,
        yearsExperience: this.specialistData.yearsExperience,
        description: this.specialistData.description,
        categories: this.specialistData.categories
      },
      this.portfolioImages
    ).subscribe({
      next: (response) => {
        console.log('Specialist profile created successfully:', response);
        this.closeAddUserForm();
        this.router.navigate(['/home']);
      },
      error: (err) => {
        console.error('Eroare la adăugarea utilizatorului:', err);
        this.stripeActivationError = err.message || err.error?.message || 'Eroare la crearea profilului';
      }
    });
  }

  // Back button logic
  showBackConfirmation() {
    if (this.step === 2) {
      this.showStep2BackAlert = true;
    } else if (this.step === 3) {
      this.showStep3BackAlert = true;
    } else if (this.step === 4) {
      this.showStep4BackAlert = true;
    }
  }

  getAlertMessage(): string {
    if (this.showStep2BackAlert) {
      return 'Sigur vrei să te întorci la pasul anterior? Categoriile selectate vor fi pierdute.';
    } else if (this.showStep3BackAlert) {
      return 'Sigur vrei să te întorci la pasul anterior? Toate imaginile adăugate vor fi pierdute.';
    } else if (this.showStep4BackAlert) {
      return 'Sigur vrei să te întorci la pasul anterior?';
    }
    return '';
  }

  handleAlertConfirm() {
    if (this.showStep2BackAlert) {
      this.goBackToStep1();
    } else if (this.showStep3BackAlert) {
      this.goBackToStep2();
    } else if (this.showStep4BackAlert) {
      this.goBackToStep3();
    }
  }

  handleAlertCancel() {
    this.showStep2BackAlert = false;
    this.showStep3BackAlert = false;
    this.showStep4BackAlert = false;
  }

  goBackToStep1() {
    this.specialistData.categories = [];
    this.showStep2BackAlert = false;
    this.step = 1;
  }

  goBackToStep2() {
    // Clean up portfolio files and previews
    this.portfolioImages = [];
    this.imagePreviews.forEach(url => URL.revokeObjectURL(url));
    this.imagePreviews = [];
    this.showStep3BackAlert = false;
    this.step = 2;
  }

  goBackToStep3() {
    this.showStep4BackAlert = false;
    this.step = 3;
  }

  closeAddUserForm() {
    this.isAddUserFormVisible = false;
  }

  // Cleanup on component destroy
  ngOnDestroy() {
    // Clean up any object URLs to prevent memory leaks
    this.imagePreviews.forEach(url => URL.revokeObjectURL(url));
  }
}
