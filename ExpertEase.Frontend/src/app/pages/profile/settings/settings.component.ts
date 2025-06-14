import { Component, OnInit, OnDestroy } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { UserDTO, UserRoleEnum } from '../../../models/api.models';
import { UserService } from '../../../services/user.service';
import { AuthService } from '../../../services/auth.service';
import { PhotoService } from '../../../services/photo.service';
import { NgIf, NgForOf } from '@angular/common';

interface UserSettings {
  fullName: string;
  email: string;
  phoneNumber: string;
  address: string;
  language: string;
  darkMode: boolean;
  emailNotifications: boolean;
  smsNotifications: boolean;
  pushNotifications: boolean;
  marketingEmails: boolean;
  serviceReminders: boolean;
  locationSharing: boolean;
  profileVisibility: 'public' | 'private' | 'specialists-only';
  autoAcceptRequests: boolean;
  workingHours: {
    enabled: boolean;
    start: string;
    end: string;
    days: string[];
  };
}

@Component({
  selector: 'app-settings',
  imports: [FormsModule, NgIf, NgForOf],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent implements OnInit, OnDestroy {
  private destroy$ = new Subject<void>();

  user: UserDTO | null = null;
  loading = false;
  saveMessage = '';

  settings: UserSettings = {
    fullName: '',
    email: '',
    phoneNumber: '',
    address: '',
    language: 'ro',
    darkMode: false,
    emailNotifications: true,
    smsNotifications: false,
    pushNotifications: true,
    marketingEmails: false,
    serviceReminders: true,
    locationSharing: false,
    profileVisibility: 'public',
    autoAcceptRequests: false,
    workingHours: {
      enabled: false,
      start: '09:00',
      end: '17:00',
      days: []
    }
  };

  availableDays = [
    { value: 'monday', label: 'Luni' },
    { value: 'tuesday', label: 'Marți' },
    { value: 'wednesday', label: 'Miercuri' },
    { value: 'thursday', label: 'Joi' },
    { value: 'friday', label: 'Vineri' },
    { value: 'saturday', label: 'Sâmbătă' },
    { value: 'sunday', label: 'Duminică' }
  ];

  languages = [
    { value: 'ro', label: 'Română' },
    { value: 'en', label: 'English' },
    { value: 'fr', label: 'Français' },
    { value: 'de', label: 'Deutsch' }
  ];

  profileVisibilityOptions = [
    { value: 'public', label: 'Public - vizibil pentru toți' },
    { value: 'specialists-only', label: 'Doar pentru specialiști' },
    { value: 'private', label: 'Privat - ascuns din căutări' }
  ];

  constructor(
    private readonly userService: UserService,
    private readonly authService: AuthService,
    private readonly photoService: PhotoService,
    private readonly router: Router
  ) {}

  ngOnInit() {
    this.loadUserProfile();
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadUserProfile() {
    this.loading = true;
    // this.userService.getUserProfile()
    //   .pipe(takeUntil(this.destroy$))
    //   .subscribe({
    //     next: (response) => {
    //       this.user = response.response;
    //       this.populateSettingsFromUser();
    //       this.loading = false;
    //     },
    //     error: (error) => {
    //       console.error('Error loading user profile:', error);
    //       this.loading = false;
    //     }
    //   });
  }

  private populateSettingsFromUser() {
    if (!this.user) return;

    this.settings = {
      ...this.settings,
      fullName: this.user.fullName || '',
      email: this.user.email || '',
      phoneNumber: this.user.contactInfo?.phoneNumber || '',
      address: this.user.contactInfo?.address || '',
      // Load other settings from user preferences if available
      // These would typically come from a user preferences endpoint
    };
  }

  onImageUpload(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (!file) return;

    this.loading = true;
    this.photoService.addProfilePicture(file)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          this.showSaveMessage('Poza de profil a fost actualizată cu succes!');
          this.loadUserProfile(); // Reload to get updated profile picture
        },
        error: (error) => {
          console.error('Error uploading image:', error);
          this.showSaveMessage('Eroare la încărcarea pozei de profil.', true);
        },
        complete: () => {
          this.loading = false;
        }
      });
  }

  saveBasicInfo() {
    if (!this.user) return;

    this.loading = true;
    const updateData = {
      fullName: this.settings.fullName,
      email: this.settings.email,
      contactInfo: {
        phoneNumber: this.settings.phoneNumber,
        address: this.settings.address
      }
    };

    // this.userService.updateUserProfile(updateData)
    //   .pipe(takeUntil(this.destroy$))
    //   .subscribe({
    //     next: (response) => {
    //       this.user = response.response;
    //       this.showSaveMessage('Informațiile de bază au fost salvate cu succes!');
    //     },
    //     error: (error) => {
    //       console.error('Error updating profile:', error);
    //       this.showSaveMessage('Eroare la salvarea informațiilor.', true);
    //     },
    //     complete: () => {
    //       this.loading = false;
    //     }
    //   });
  }

  saveNotificationSettings() {
    this.loading = true;

    // TODO: Implement notification settings API call
    // this.userService.updateNotificationSettings(this.settings)
    setTimeout(() => {
      this.showSaveMessage('Setările de notificare au fost salvate!');
      this.loading = false;
    }, 1000);
  }

  savePrivacySettings() {
    this.loading = true;

    // TODO: Implement privacy settings API call
    // this.userService.updatePrivacySettings(this.settings)
    setTimeout(() => {
      this.showSaveMessage('Setările de confidențialitate au fost salvate!');
      this.loading = false;
    }, 1000);
  }

  saveWorkingHours() {
    if (!this.isSpecialist()) return;

    this.loading = true;

    // TODO: Implement working hours API call
    // this.userService.updateWorkingHours(this.settings.workingHours)
    setTimeout(() => {
      this.showSaveMessage('Programul de lucru a fost salvat!');
      this.loading = false;
    }, 1000);
  }

  toggleWorkingDay(day: string) {
    const index = this.settings.workingHours.days.indexOf(day);
    if (index > -1) {
      this.settings.workingHours.days.splice(index, 1);
    } else {
      this.settings.workingHours.days.push(day);
    }
  }

  isWorkingDaySelected(day: string): boolean {
    return this.settings.workingHours.days.includes(day);
  }

  isSpecialist(): boolean {
    return this.user?.role === UserRoleEnum.Specialist;
  }

  changePassword() {
    // Navigate to change password component or open modal
    this.router.navigate(['/profile/change-password']);
  }

  exportData() {
    // TODO: Implement data export functionality
    this.showSaveMessage('Datele tale vor fi trimise pe email în curând.');
  }

  deactivateAccount() {
    const confirmed = confirm(
      'Ești sigur că vrei să dezactivezi contul? ' +
      'Contul tău va fi ascuns temporar, dar poți să îl reactivezi oricând.'
    );

    if (confirmed) {
      this.loading = true;
      // TODO: Implement account deactivation
      // this.userService.deactivateAccount()
      setTimeout(() => {
        this.showSaveMessage('Contul a fost dezactivat.');
        this.authService.logout();
        this.router.navigate(['/home']);
      }, 1500);
    }
  }

  deleteAccount() {
    const firstConfirm = confirm(
      'Ești sigur că vrei să ștergi contul? ' +
      'Această acțiune este ireversibilă și toate datele tale vor fi șterse permanent.'
    );

    if (firstConfirm) {
      const secondConfirm = confirm(
        'Confirmă din nou: Toate datele tale vor fi șterse permanent. ' +
        'Această acțiune NU poate fi anulată.'
      );

      if (secondConfirm) {
        this.loading = true;
        // TODO: Implement account deletion
        // this.userService.deleteAccount()
        setTimeout(() => {
          this.showSaveMessage('Contul a fost șters.');
          this.authService.logout();
          this.router.navigate(['/home']);
        }, 2000);
      }
    }
  }

  goBack() {
    this.router.navigate(['/profile']);
  }

  private showSaveMessage(message: string, isError = false) {
    this.saveMessage = message;
    setTimeout(() => {
      this.saveMessage = '';
    }, 3000);
  }
}
