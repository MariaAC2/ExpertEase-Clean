import {Component, OnInit} from '@angular/core';
import {DatePipe} from '@angular/common';
import {IconButtonComponent} from '../../shared/icon-button/icon-button.component';
import {ProfileService} from '../../services/profile.service';
import {UserDTO, UserRoleEnum} from '../../models/api.models';
import {Router} from '@angular/router';

@Component({
  selector: 'app-profile',
  imports: [DatePipe, IconButtonComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit {
  user: UserDTO | undefined;
  constructor(private profileService: ProfileService, private route: Router) {}
  ngOnInit() {
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
    this.route.navigate(['profile/account']);
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
}
