import {
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
  isExisting?: boolean;
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

  // Existing portfolio images from the server
  existingPortfolioImages: PortfolioImage[] = [];

  // New files to upload
  newPortfolioFiles: File[] = [];

  // IDs of photos marked for removal
  photosToRemove: string[] = [];

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

      // Load existing portfolio images
      this.existingPortfolioImages = [];
      this.photosToRemove = [];
      this.newPortfolioFiles = [];

      if (this.user.portfolio && this.user.portfolio.length > 0) {
        this.existingPortfolioImages = this.user.portfolio.map((url, index) => ({
          id: `existing_${index}`, // You might want to use actual photo IDs from your API
          url: url,
          isExisting: true
        }));
      }
    }
  }

  // Get all portfolio images for display (existing + new)
  get portfolioImages(): PortfolioImage[] {
    const existingFiltered = this.existingPortfolioImages.filter(
      img => !this.photosToRemove.includes(img.id!)
    );

    const newImages = this.newPortfolioFiles.map((file, index) => ({
      id: `new_${index}`,
      url: URL.createObjectURL(file),
      file: file,
      isNew: true
    }));

    return [...existingFiltered, ...newImages];
  }

  updateCategories(categories: string[]) {
    this.specialistInfo.categories = categories;
  }

  onPortfolioImagesSelected(event: Event) {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;

    // Add new files to the array
    for (let i = 0; i < input.files.length; i++) {
      const file = input.files[i];
      this.newPortfolioFiles.push(file);
    }

    // Reset input
    input.value = '';
  }

  removeImage(index: number) {
    const allImages = this.portfolioImages;
    const imageToRemove = allImages[index];

    if (imageToRemove.isNew) {
      // Remove from new files array
      const newFileIndex = this.newPortfolioFiles.findIndex(
        (file, i) => `new_${i}` === imageToRemove.id
      );
      if (newFileIndex > -1) {
        // Revoke the object URL to prevent memory leaks
        URL.revokeObjectURL(imageToRemove.url);
        this.newPortfolioFiles.splice(newFileIndex, 1);
      }
    } else if (imageToRemove.isExisting) {
      // Mark existing image for removal
      if (imageToRemove.id) {
        this.photosToRemove.push(imageToRemove.id);
      }
    }
  }

  onClose() {
    this.close.emit();
    this.resetForm();
  }

  private resetForm() {
    // Clean up object URLs
    this.newPortfolioFiles.forEach((_, index) => {
      const url = URL.createObjectURL(this.newPortfolioFiles[index]);
      URL.revokeObjectURL(url);
    });

    this.loadSpecialistData();
  }

  async onSubmit() {
    if (this.isLoading) return;

    this.isLoading = true;

    try {
      const formData = new FormData();

      // Add basic profile data
      formData.append('UserId', this.user!.id);

      if (this.specialistInfo.phoneNumber) {
        formData.append('PhoneNumber', this.specialistInfo.phoneNumber);
      }

      if (this.specialistInfo.address) {
        formData.append('Address', this.specialistInfo.address);
      }

      if (this.specialistInfo.yearsExperience !== undefined) {
        formData.append('YearsExperience', this.specialistInfo.yearsExperience.toString());
      }

      if (this.specialistInfo.description) {
        formData.append('Description', this.specialistInfo.description);
      }

      // Add existing photos to keep (those not marked for removal)
      const photosToKeep = this.existingPortfolioImages
        .filter(img => !this.photosToRemove.includes(img.id!))
        .map(img => img.url);

      photosToKeep.forEach((url, index) => {
        formData.append(`ExistingPortfolioPhotoUrls[${index}]`, url);
      });

      // Add photos marked for removal
      this.photosToRemove.forEach((photoId, index) => {
        formData.append(`PhotoIdsToRemove[${index}]`, photoId);
      });

      // Add new photos to upload
      this.newPortfolioFiles.forEach((file) => {
        formData.append('NewPortfolioPhotos', file, file.name);
      });

      // Call the service
      this.specialistService.updateSpecialistProfile(formData).subscribe({
        next: (response) => {
          console.log('Specialist updated successfully', response);
          this.handleSuccess();
        },
        error: (error) => {
          console.error('Error updating specialist:', error);
          this.isLoading = false;
          // You could show an error message here
          alert('Eroare la actualizarea profilului. Te rugăm să încerci din nou.');
        }
      });

    } catch (error) {
      console.error('Error during update:', error);
      this.isLoading = false;
      alert('Eroare neașteptată. Te rugăm să încerci din nou.');
    }
  }

  private handleSuccess() {
    this.isLoading = false;

    // Emit success and close the modal
    this.specialistUpdated.emit(this.user!);
    this.onClose();

    // Show success message
    alert('Profilul a fost actualizat cu succes!');
  }

  // Helper method to get file preview URL
  getFilePreview(file: File): string {
    return URL.createObjectURL(file);
  }

  // Helper method to format file size
  formatFileSize(bytes: number): string {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  // Cleanup on component destroy
  ngOnDestroy() {
    // Clean up any remaining object URLs
    this.newPortfolioFiles.forEach((_, index) => {
      const url = URL.createObjectURL(this.newPortfolioFiles[index]);
      URL.revokeObjectURL(url);
    });
  }
}
