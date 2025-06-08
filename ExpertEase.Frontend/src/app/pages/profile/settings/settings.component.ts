import { Component } from '@angular/core';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'app-settings',
  imports: [
    FormsModule
  ],
  templateUrl: './settings.component.html',
  styleUrl: './settings.component.scss'
})
export class SettingsComponent {
  settings = {
    fullName: '',
    email: '',
    phoneNumber: '',
    language: 'ro',
    darkMode: false,
    emailNotifications: true,
    smsNotifications: false,
  };

  onImageUpload(event: Event) {
    const file = (event.target as HTMLInputElement).files?.[0];
    if (file) {
      console.log('Uploaded image:', file.name);
    }
  }

  changePassword() {
    alert('Password change flow goes here.');
  }

  deactivateAccount() {
    if (confirm('Ești sigur că vrei să dezactivezi contul?')) {
      console.log('Account deactivated');
    }
  }

  deleteAccount() {
    if (confirm('Ești sigur că vrei să ștergi contul? Această acțiune este ireversibilă.')) {
      console.log('Account deleted');
    }
  }
}
