import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import {UserDTO, UserUpdateDTO} from '../../../models/api.models';
import {UserService} from '../../../services/user.service';

interface UserEditInfo {
  firstName: string,
  lastName: string,
  email: string;
}

interface PasswordChangeData {
  currentPassword: string;
  newPassword: string;
  confirmPassword: string;
}

@Component({
  selector: 'app-edit-user-info',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './edit-user-info.component.html',
  styleUrls: ['./edit-user-info.component.scss']
})
export class EditUserInfoComponent implements OnInit {
  @Input() isVisible = false;
  @Input() user: UserDTO | undefined | null = null;
  @Output() close = new EventEmitter<void>();
  @Output() userUpdated = new EventEmitter<UserDTO>();

  userInfo: UserEditInfo = {
    firstName: '',
    lastName: '',
    email: ''
  };

  passwordData: PasswordChangeData = {
    currentPassword: '',
    newPassword: '',
    confirmPassword: ''
  };

  isLoading = false;

  constructor(private readonly userService: UserService) {}

  ngOnInit() {
    if (this.user) {
      this.loadUserData();
    }
  }

  ngOnChanges() {
    if (this.user && this.isVisible) {
      this.loadUserData();
    }
  }

  private loadUserData() {
    if (this.user) {
      const [firstName = '', lastName = ''] = (this.user.fullName || '').split(' ');

      this.userInfo = {
        firstName,
        lastName,
        email: this.user.email || ''
      };
    }
  }

  onClose() {
    this.close.emit();
    this.resetForm();
  }

  private resetForm() {
    this.loadUserData();
    this.passwordData = {
      currentPassword: '',
      newPassword: '',
      confirmPassword: ''
    };
  }

  async onSubmit() {
    if (this.isLoading) return;

    this.isLoading = true;

    try {
      // Prepare UserUpdateDTO based on your API model
      const updateData: UserUpdateDTO = {
        id: this.user!.id,
        firstName: this.userInfo.firstName,
        lastName: this.userInfo.lastName,
        // Note: Email updates might need special handling depending on your backend
      };

      // Only include password if it's being changed
      if (this.passwordData.currentPassword && this.passwordData.newPassword) {
        if (this.passwordData.newPassword !== this.passwordData.confirmPassword) {
          console.error('Passwords do not match');
          this.isLoading = false;
          return;
        }
        updateData.password = this.passwordData.newPassword;
      }

      // // Call your user service to update user info
      // this.userService.updateUserProfile(updateData).subscribe({
      //   next: (response) => {
      //     console.log('User updated successfully', response);
      //     this.handleSuccess(response.response);
      //   },
      //   error: (error) => {
      //     console.error('Error updating user:', error);
      //     this.isLoading = false;
      //     // You could show an error message here
      //   }
      // });

    } catch (error) {
      console.error('Error during update:', error);
      this.isLoading = false;
    }
  }

  private handleSuccess(updatedUser?: UserDTO) {
    this.isLoading = false;
    if (updatedUser) {
      this.userUpdated.emit(updatedUser);
    }
    this.onClose();
    // You could show a success message here
  }
}
