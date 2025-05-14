import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
// import { AdminService } from '../../../services/admin.service';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './admin.users.component.html',
  styleUrls: ['../admin.content.scss']
})
export class AdminUsersComponent {
  users: any[] = [];
  response: any;
  loading: boolean = false;
  error: string | null = null;

  // constructor(private adminService: AdminService) {}

  // ngOnInit(): void {
  //   this.fetchUsers();
  // }

  // fetchUsers(): void {
  //   this.loading = true;
  //   this.adminService.getUsers().subscribe({
  //     next: (res) => {
  //       this.users = res?.response?.data ?? [];
  //       this.loading = false;
  //     },
  //     error: (err) => {
  //       this.error = err.error?.errorMessage?.message || 'A apărut o eroare.';
  //       this.loading = false;
  //     }
  //   });
  // }
}
