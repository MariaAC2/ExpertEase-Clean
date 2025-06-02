import {Component, OnInit} from '@angular/core';
import {DatePipe, NgForOf, NgIf} from '@angular/common';
import {IconButtonComponent} from '../../shared/icon-button/icon-button.component';
import {ProfileService} from '../../services/profile.service';
import {UserDTO, UserRoleEnum} from '../../models/api.models';
import {Router} from '@angular/router';

@Component({
  selector: 'app-profile',
  imports: [DatePipe, IconButtonComponent, NgForOf, NgIf],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  user: UserDTO | undefined;
  dummyUser: UserDTO = {
    id: 'user-001',
    fullName: 'Elena Georgescu',
    email: 'elena.georgescu@example.com',
    role: UserRoleEnum.Specialist,
    createdAt: new Date('2023-02-10T10:15:00Z'),
    updatedAt: new Date(),
    rating: 4.8,
    contactInfo: {
      phoneNumber: '0745123456',
      address: 'Strada Florilor 12, Timișoara'
    },
    specialist: {
      yearsExperience: 6,
      description: 'Specialist în traduceri juridice și consultanță contractuală.',
      categories: [
        { id: 'cat-007', name: 'Traduceri juridice' },
        { id: 'cat-008', name: 'Consultanță legală' }
      ]
    }
  };
  constructor(private profileService: ProfileService, private route: Router) {}
  ngOnInit() {
    // this.user = this.dummyUser;
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

  onEdit() {
    // open modal or navigate to profile edit
  }

  goToReviews() {
    // navigate to reviews page
  }

  goToHistory() {
    // navigate to service history
  }

  goToExpertEase() {
    // navigate to ExpertEase account management
    // this.route.navigate(['profile/account']);
  }

  goToSettings() {
    // navigate to settings
  }

  logout() {
    const confirmed = window.confirm('Sigur vrei să te deloghezi?');

    if (confirmed) {
      this.profileService.logout();
    }
    this.route.navigate(['/home']);
  }

  protected readonly UserRoleEnum = UserRoleEnum;
}
