import {Component, OnInit} from '@angular/core';
import {DatePipe, NgForOf, NgIf} from '@angular/common';
import {IconButtonComponent} from '../../shared/icon-button/icon-button.component';
import {UserProfileDTO, UserRoleEnum} from '../../models/api.models';
import {Router} from '@angular/router';
import {UserService} from '../../services/user.service';
import {AuthService} from '../../services/auth.service';
import {PhotoService} from '../../services/photo.service';
import {EditSpecialistInfoComponent} from './edit-specialist-info/edit-specialist-info.component';
import {EditUserInfoComponent} from './edit-user-info/edit-user-info.component';

@Component({
  selector: 'app-profile',
  imports: [DatePipe, IconButtonComponent, NgForOf, NgIf, EditSpecialistInfoComponent, EditUserInfoComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  user: UserProfileDTO | undefined | null;
  userRole: string | undefined | null;

  // Modal visibility states
  showEditUserModal = false;
  showEditSpecialistModal = false;

  // dummyUser: UserDTO = {
  //   id: 'user-001',
  //   fullName: 'Elena Georgescu',
  //   email: 'elena.georgescu@example.com',
  //   role: UserRoleEnum.Specialist,
  //   createdAt: new Date('2023-02-10T10:15:00Z'),
  //   updatedAt: new Date(),
  //   rating: 4.8,
  //   contactInfo: {
  //     phoneNumber: '0745123456',
  //     address: 'Strada Florilor 12, Timișoara'
  //   },
  //   specialist: {
  //     yearsExperience: 6,
  //     description: 'Specialist în traduceri juridice și consultanță contractuală.',
  //     categories: [
  //       { id: 'cat-007', name: 'Traduceri juridice' },
  //       { id: 'cat-008', name: 'Consultanță legală' }
  //     ]
  //   }
  // };

  constructor(private readonly profileService: UserService,
              private readonly authService: AuthService,
              private readonly photoService: PhotoService,
              private readonly route: Router) {}

  ngOnInit() {
    this.userRole = this.authService.getUserRole()
    this.loadUserProfile();
  }
  loadUserProfile() {
    // this.user = this.dummyUser; // Use this for testing
    this.profileService.getUserProfile().subscribe({
      next: (res) => {
        this.user = res.response;
        console.log(this.user);
      },
      error: (err) => {
        console.error(err);
      }
    });
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (!file) return;

    this.photoService.addProfilePicture(file).subscribe({
      next: (res) => {
        console.log('Upload success:', res);
        // Reload user profile to get updated picture URL
        this.loadUserProfile();
      },
      error: (err) => {
        console.error('Upload error:', err);
      }
    });
  }

  // User Edit Modal Methods
  openEditUserModal() {
    this.showEditUserModal = true;
  }

  onCloseEditUserModal() {
    this.showEditUserModal = false;
  }

  onUserUpdated(updatedUser: UserProfileDTO) {
    this.user = updatedUser;
    console.log('User profile updated:', updatedUser);
    // Optionally show a success message
    // this.showSuccessMessage('Profilul a fost actualizat cu succes!');
  }

  // Specialist Edit Modal Methods
  openEditSpecialistModal() {
    this.showEditSpecialistModal = true;
  }

  onCloseEditSpecialistModal() {
    this.showEditSpecialistModal = false;
  }

  onSpecialistUpdated(updatedUser: UserProfileDTO) {
    this.user = updatedUser;
    console.log('Specialist profile updated:', updatedUser);
    // Reload the entire profile to ensure all data is fresh
    this.loadUserProfile();
    // Optionally show a success message
    // this.showSuccessMessage('Anunțul de specialist a fost actualizat cu succes!');
  }

  // Keep your existing methods
  goToReviews() {
    this.route.navigate(['profile/user-reviews']);
  }

  goToBankAccount() {
    this.route.navigate(['profile/stripe-account']);
  }

  goToHistory() {
    // navigate to service history
  }

  goToSettings() {
    this.route.navigate(['profile/user-settings']);
  }

  logout() {
    const confirmed = window.confirm('Sigur vrei să te deloghezi?');

    if (confirmed) {
      this.authService.logout();
      this.route.navigate(['/home']);
    }
  }
}
