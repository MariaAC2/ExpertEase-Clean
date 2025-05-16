import { Component } from '@angular/core';
import {DatePipe} from '@angular/common';
import {IconButtonComponent} from '../../shared/icon-button/icon-button.component';

@Component({
  selector: 'app-profile',
  imports: [DatePipe, IconButtonComponent],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent {
  user = {
    firstName: 'Prenume',
    lastName: 'Nume',
    email: 'nume.prenume@email.com',
    createdAt: new Date('2010-10-10')
  };

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
  }

  goToSettings() {
    // navigate to settings
  }

  logout() {
    // perform logout logic
  }
}
