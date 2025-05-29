import {Component, OnInit} from '@angular/core';
import {CommonModule} from '@angular/common';
import {AdminUsersService} from '../../../services/admin.users.service';
import {AdminUserUpdateDTO, UserAddDTO, UserDTO, UserRoleEnum, UserUpdateDTO} from '../../../models/api.models';
import {FormsModule} from '@angular/forms';
import {DynamicFormComponent} from '../../../shared/dynamic-form/dynamic-form.component';
import {dtoToDictionary, dtoToFormFields} from '../../../models/form.models';
import {TableColumn} from '../../../models/table.models';
import {DynamicTableComponent} from '../../../shared/dynamic-table/dynamic-table.component';
import {AdminDetailsComponent} from '../../../shared/admin-details/admin-details.component';
import {SearchInputComponent} from '../../../shared/search-input/search-input.component';
import {PaginationComponent} from '../../../shared/pagination/pagination.component';

@Component({
  selector: 'app-admin-users',
  standalone: true,
  imports: [CommonModule, FormsModule, DynamicFormComponent, DynamicTableComponent, AdminDetailsComponent, SearchInputComponent, PaginationComponent],
  templateUrl: './admin.users.component.html',
  styleUrls: ['../../../shared/dynamic-table/dynamic-table.component.scss', '../../../shared/admin-details/admin-details.component.scss'],
})
export class AdminUsersComponent implements OnInit {

  dummy_users: UserDTO[] = [
    {
      id: 'm3n4o5p6',
      fullName: 'Ana Georgescu',
      email: 'ana.georgescu@example.com',
      role: UserRoleEnum.Admin,
      createdAt: new Date('2024-09-01T11:30:00Z'),
      updatedAt: new Date('2025-01-12T13:00:00Z')
    },
    {
      id: 'q7r8s9t0',
      fullName: 'Radu Enache',
      email: 'radu.enache@example.com',
      role: UserRoleEnum.Client,
      createdAt: new Date('2024-08-20T15:45:00Z'),
      updatedAt: new Date('2025-01-08T11:15:00Z')
    }
  ];

  searchTerm: string = '';
  users: UserDTO[] = [];
  selectedUser: UserDTO | null = null;
  entityDetails: Record<string, any> = {};
  entityDetailsId: string | undefined;
  pageSizeOptions: number[] = [5, 10, 20, 50];
  pageSize: number = 10;
  currentPage: number = 1;
  totalItems: number = 0;

  isAddUserFormVisible = false;
  isUpdateUserFormVisible = false;
  isUserDetailsVisible = false;
  selectedUserId: string | null = null;

  columns: TableColumn[] = [
    { key: 'id', header: 'ID' },

    {
      key: 'fullName',
      header: 'Nume',
    },

    { key: 'email', header: 'Email' },
    { key: 'role', header: 'Rol' },

    {
      key: 'actions',
      header: 'Acțiuni',
      type: 'action',
      actions: [
        {
          label: 'Detalii',
          class: 'view',
          callback: (row: UserDTO) => this.getEntity(row.id)
        },
        {
          label: 'Modifică',
          class: 'edit',
          callback: (row: UserDTO) => this.editUser(row)
        },
        {
          label: 'Șterge',
          class: 'delete',
          callback: (row) => this.deleteEntity(row)
        }
      ]
    }
  ];

  defaultUser: UserAddDTO = {
    fullName: '',
    email: '',
    password: '',
    role: UserRoleEnum.Client
  };
  formData: { [key: string]: any } = {};

  error: string | null = null;

  addEntityFormFields = dtoToFormFields(this.defaultUser, {
    email: { type: 'email' },
    password: { type: 'password' },
    role: {
      type: 'select',
      options: Object.values(UserRoleEnum) // ['Admin', 'Specialist', 'Client']
    }
  });

  updateEntityFormFields = dtoToFormFields(
    {
      fullName: this.selectedUser?.fullName || '',
      role: this.selectedUser?.role || '',
    },
    {
      role: {
        type: 'select',
        options: Object.values(UserRoleEnum)
      }
    }
  );

  constructor(private adminService: AdminUsersService) {}

  ngOnInit(): void {
    const defaultFormValues: UserAddDTO = {
      fullName: '',
      email: '',
      password: '',
      role: UserRoleEnum.Client
    };

    this.formData = { ...defaultFormValues };
    this.getPage();
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.currentPage = 1;
    this.getPage();
  }

  getEntity(userId: string): void {
    console.log(userId);
    this.adminService.getUser(userId).subscribe({
      next: (res) => {
        // console.log(res.response);
        this.entityDetailsId = res.response?.id;
        this.entityDetails = dtoToDictionary(res.response ?? {});
        this.isUserDetailsVisible = true;
      },
      error: (err) => {
        console.error('Eroare la preluarea utilizatorului:', err);
        alert('Nu s-au putut încărca detaliile utilizatorului.');
      }
    });
  }

  getPage(): void {
    this.adminService.getUsers(this.searchTerm, this.currentPage, this.pageSize).subscribe({
      next: (res) => {
        console.log(res.response);
        this.users = res.response?.data ?? [];
        this.totalItems = res.response?.totalCount ?? 0;
      },
      error: (err) => {
        this.error = err.error?.errorMessage?.message || 'A apărut o eroare.';
      }
    });
  }

  addEntity(data: { [key: string]: any }) {
    const userToSubmit: UserAddDTO = data as UserAddDTO;
    this.adminService.addUser(userToSubmit).subscribe({
      next: () => {
        this.closeAddUserForm();
        this.getPage(); // refresh user list
      },
      error: (err) => {
        console.error('Eroare la adăugarea utilizatorului:', err);
      }
    });
  }

  editUser(user: UserDTO): void {
    this.selectedUserId = user.id;

    this.formData = {
      fullName: user.fullName,
      role: user.role
    };

    this.isUpdateUserFormVisible = true;
  }

  updateEntity(data: { [key: string]: any }) {
    if (!this.selectedUserId) {
      console.error('Niciun utilizator nu a fost selectat pentru actualizare.');
      alert('Eroare: Nu a fost selectat niciun utilizator pentru actualizare.'); // or use a proper toast
      return;
    }

    const fullName =
      data['fullName'] ??
      [data['firstName'], data['lastName']]
        .filter(Boolean)
        .join(' ')
        .trim();

    const updatePayload: AdminUserUpdateDTO = {
      id: this.selectedUserId,
      fullName: fullName,
      role: data['role'],
    };

    this.adminService.updateUser(this.selectedUserId, updatePayload).subscribe({
      next: () => {
        this.closeUpdateUserForm();
        this.getPage();
      },
      error: (err) => {
        console.error('Eroare la actualizare:', err);
      }
    });
  }

  showUserDetails(user: UserDTO) {
    // this.selectedUser = user;
    this.isUserDetailsVisible = true;
  }

  closeUserDetails() {
    this.isUserDetailsVisible = false;
    this.selectedUser = null;
  }

  closeUpdateUserForm() {
    this.selectedUserId = null;
    this.resetForm(); // clears formData
    this.isUpdateUserFormVisible = false;
  }
  openAddUserForm(): void {
    this.selectedUserId = null;
    this.resetForm(); // clears formData
    this.isAddUserFormVisible = true;
  }

  viewUser(user: UserDTO): void {
    // Implement the logic to view user details
    this.isUserDetailsVisible = true;
  }
  deleteEntity(user: UserDTO): void {
    if (confirm(`Sigur doriți să ștergeți utilizatorul ${user.fullName}?`)) {
      this.adminService.deleteUser(user.id).subscribe({
        next: () => {
          this.getPage();
        },
        error: (err) => {
          console.error('Eroare la ștergerea utilizatorului:', err);
        }
      });
    }
  }
  closeAddUserForm() {
    this.isAddUserFormVisible = false;
    this.resetForm();
  }

  resetForm() {
    this.formData = {
      firstName: '',
      lastName: '',
      email: '',
      password: '',
      role: UserRoleEnum.Client,
    };
  }

  onPageChange(newPage: number): void {
    this.currentPage = newPage;
    this.getPage();
  }

  onPageSizeChange(newSize: number): void {
    this.pageSize = newSize;
    this.currentPage = 1;
    this.getPage();
  }
}
