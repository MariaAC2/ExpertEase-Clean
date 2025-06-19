import {
  PortfolioPictureAddDTO,
  SpecialistProfileUpdateDTO,
  UserProfileDTO,
} from '../../../models/api.models';
import {Component, EventEmitter, Input, OnChanges, OnInit, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {FormsModule} from '@angular/forms';
import {SpecialistProfileService} from '../../../services/specialist-profile.service';
import {CategorySelectorComponent} from '../../../shared/category-selector/category-selector.component';

interface SpecialistEditInfo {
  phoneNumber: string;
  address: string;
  categories: string[];
  yearsExperience: number;
  description: string;
}

interface PortfolioImage {
  id?: string;
  url: string;
  file?: File;
  isNew?: boolean;
}

@Component({
  selector: 'app-edit-specialist-info',
  standalone: true,
  imports: [CommonModule, FormsModule, CategorySelectorComponent],
  templateUrl: './edit-specialist-info.component.html',
  styleUrls: ['./edit-specialist-info.component.scss']
})
export class EditSpecialistInfoComponent implements OnInit, OnChanges {
  @Input() isVisible = false;
  @Input() user: UserProfileDTO | undefined | null = null;
  @Input() userRole: string | undefined | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() specialistUpdated = new EventEmitter<UserProfileDTO>();

  specialistInfo: SpecialistEditInfo = {
    phoneNumber: '',
    address: '',
    categories: [],
    yearsExperience: 0,
    description: ''
  };

  portfolioImages: PortfolioImage[] = [];
  newPortfolioFiles: PortfolioPictureAddDTO[] = [];

  isLoading = false;

  constructor(private readonly specialistService: SpecialistProfileService) {}

  ngOnInit() {
    if (this.user) {
      this.loadSpecialistData();
    }
  }

  ngOnChanges() {
    if (this.user && this.isVisible) {
      this.loadSpecialistData();
    }
  }

  private loadSpecialistData() {
    if (this.user) {
      this.specialistInfo = {
        phoneNumber: this.user.phoneNumber || '',
        address: this.user.address || '',
        categories: this.user.categories || [],
        yearsExperience: this.user.yearsExperience || 0,
        description: this.user.description || ''
      };

      // Load existing portfolio images if available
      // Note: You may need to adjust this based on your actual API structure
      this.portfolioImages = [];
      this.newPortfolioFiles = [];
    }
  }

  updateCategories(categories: string[]) {
    this.specialistInfo.categories = categories;
  }

  onPortfolioImagesSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;

    for (const element of input.files) {
      const file = element;

      // Create portfolio DTO for new files
      const portfolioDto: PortfolioPictureAddDTO = {
        fileStream: file,
        contentType: file.type,
        fileName: file.name
      };
      this.newPortfolioFiles.push(portfolioDto);

      // Create preview for display
      const reader = new FileReader();
      reader.onload = () => {
        this.portfolioImages.push({
          url: reader.result as string,
          file: file,
          isNew: true
        });
      };
      reader.readAsDataURL(file);
    }

    // Reset input
    input.value = '';
  }

  removeImage(index: number) {
    const imageToRemove = this.portfolioImages[index];

    if (imageToRemove.isNew) {
      // Remove from new files array
      const fileIndex = this.newPortfolioFiles.findIndex(f => f.fileName === imageToRemove.file?.name);
      if (fileIndex > -1) {
        this.newPortfolioFiles.splice(fileIndex, 1);
      }
    }

    // Remove from display array
    this.portfolioImages.splice(index, 1);
  }

  onClose() {
    this.close.emit();
    this.resetForm();
  }

  private resetForm() {
    this.loadSpecialistData();
    this.newPortfolioFiles = [];
  }

  async onSubmit() {
    if (this.isLoading) return;

    this.isLoading = true;

    try {
      // Create SpecialistProfileUpdateDTO according to your API model
      const updateData: SpecialistProfileUpdateDTO = {
        userId: this.user!.id,
        phoneNumber: this.specialistInfo.phoneNumber,
        address: this.specialistInfo.address,
        categories: this.specialistInfo.categories,
        yearsExperience: this.specialistInfo.yearsExperience,
        description: this.specialistInfo.description
      };

      // Include portfolio if there are new images
      if (this.newPortfolioFiles.length > 0) {
        updateData.portfolio = this.newPortfolioFiles;
      }

      // Update specialist profile
      // this.specialistService.updateSpecialistProfile(updateData).subscribe({
      //   next: (response) => {
      //     console.log('Specialist updated successfully', response);
      //     this.handleSuccess(response.response);
      //   },
      //   error: (error) => {
      //     console.error('Error updating specialist:', error);
      //     this.isLoading = false;
      //     // You could show an error message here
      //   }
      // });

    } catch (error) {
      console.error('Error during update:', error);
      this.isLoading = false;
    }
  }

  private handleSuccess(updatedUser?: UserProfileDTO) {
    this.isLoading = false;
    if (updatedUser) {
      this.specialistUpdated.emit(updatedUser);
    }
    this.onClose();
    // You could show a success message here
  }
}
