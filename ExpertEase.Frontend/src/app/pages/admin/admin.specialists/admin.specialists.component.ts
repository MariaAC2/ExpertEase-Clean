import { Component } from '@angular/core';
import {FormsModule} from '@angular/forms';
import {NgIf} from '@angular/common';
import {
  SpecialistAddDTO,
  SpecialistDTO, SpecialistUpdateDTO,
  UserDTO,
  UserRoleEnum,
} from '../../../models/api.models';
import {TableColumn} from '../../../models/table.models';
import {dtoToDictionary, dtoToFormFields} from '../../../models/form.models';
import {SpecialistService} from '../../../services/specialist.service';
import {AdminDetailsComponent} from '../../../shared/admin-details/admin-details.component';
import {DynamicFormComponent} from '../../../shared/dynamic-form/dynamic-form.component';
import {DynamicTableComponent} from '../../../shared/dynamic-table/dynamic-table.component';
import {SearchInputComponent} from '../../../shared/search-input/search-input.component';
import {PaginationComponent} from '../../../shared/pagination/pagination.component';

@Component({
  selector: 'app-admin.users',
  imports: [
    FormsModule,
    AdminDetailsComponent,
    DynamicFormComponent,
    DynamicTableComponent,
    NgIf,
    SearchInputComponent,
    PaginationComponent
  ],
  templateUrl: './admin.specialists.component.html',
  styleUrls: ['../../../shared/dynamic-table/dynamic-table.component.scss', '../../../shared/admin-details/admin-details.component.scss'],
})
export class AdminSpecialistsComponent {
  searchTerm: string = '';
  users: SpecialistDTO[] = [];
  selectedUser: SpecialistDTO | null = null;
  entityDetails: Record<string, any> = {};
  entityDetailsId: string | undefined;
  pageSize: number = 10;
  currentPage: number = 1;
  totalItems: number = 0;

  isAddUserFormVisible = false;
  isUpdateUserFormVisible = false;
  isUserDetailsVisible = false;
  selectedUserId: string | null = null;

  placeholder: string = 'Caută un specialist...';

  columns: TableColumn[] = [
    { key: 'id', header: 'ID' },

    {
      key: 'fullName',
      header: 'Nume',
    },

    { key: 'email', header: 'Email' },
    { key: 'phoneNumber', header: 'Număr de telefon' },
    { key: 'address', header: 'Adresă' },

    {
      key: 'actions',
      header: 'Acțiuni',
      type: 'action',
      actions: [
        {
          label: 'Detalii',
          class: 'view',
          callback: (row: SpecialistDTO) => this.getEntity(row.id)
        },
        {
          label: 'Modifică',
          class: 'edit',
          callback: (row: SpecialistDTO) => this.editUser(row)
        },
        {
          label: 'Șterge',
          class: 'delete',
          callback: (row) => this.deleteUser(row)
        }
      ]
    }
  ];

  defaultUser: SpecialistAddDTO = {
    fullName: '',
    email: '',
    password: '',
    phoneNumber: '',
    address: '',
    yearsExperience: 0,
    description: '',
  };
  formData: { [key: string]: any } = {};

  error: string | null = null;

  addEntityFormFields = dtoToFormFields(this.defaultUser, {
    email: { type: 'email' },
    password: { type: 'password' },
    yearsExperience: { type: 'number', placeholder: 'Ex: 5' },
    description: { type: 'textarea', placeholder: 'Descrie serviciile oferite', class: 'full-width' }
  });

  updateEntityFormFields = dtoToFormFields(
    {
      fullName: this.selectedUser?.fullName || '',
      phoneNumber: this.selectedUser?.phoneNumber || '',
      address: this.selectedUser?.address || '',
      yearsExperience: this.selectedUser?.yearsExperience || '',
      description: this.selectedUser?.description || ''
    }
  );

  constructor(private readonly adminService: SpecialistService) {
    const defaultFormValues: SpecialistAddDTO = {
      fullName: '',
      email: '',
      password: '',
      phoneNumber: '',
      address: '',
      yearsExperience: 0,
      description: '',
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
    this.adminService.getSpecialist(userId).subscribe({
      next: (res) => {
        this.entityDetailsId = res.response?.id;
        console.log(res.response);
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
    this.adminService.getSpecialistsLegacy(this.searchTerm, this.currentPage, this.pageSize).subscribe({
      next: (res) => {
        this.users = res.response?.data ?? [];
        this.totalItems = res.response?.totalCount ?? 0;
      },
      error: (err) => {
        this.error = err.error?.errorMessage?.message || 'A apărut o eroare.';
      }
    });
  }

  addEntity(data: { [key: string]: any }) {
    const userToSubmit: SpecialistAddDTO = data as SpecialistAddDTO;
    this.adminService.addSpecialist(userToSubmit).subscribe({
      next: () => {
        this.closeAddUserForm();
        this.getPage(); // refresh user list
      },
      error: (err) => {
        console.error('Eroare la adăugarea utilizatorului:', err);
      }
    });
  }

  editUser(user: SpecialistDTO): void {
    this.selectedUserId = user.id;

    this.formData = {
      fullName: user.fullName,
      phoneNumber: user.phoneNumber,
      address: user.address,
      yearsExperience: user.yearsExperience,
      description: user.description,
    };

    this.isUpdateUserFormVisible = true;
  }

  updateEntity(data: { [key: string]: any }) {
    console.log('Form data submitted:', data);
    if (!this.selectedUserId) {
      console.error('Niciun utilizator nu a fost selectat pentru actualizare.');
      alert('Eroare: Nu a fost selectat niciun utilizator pentru actualizare.'); // or use a proper toast
      return;
    }

    const updatePayload: SpecialistUpdateDTO = {
      id: this.selectedUserId,
      fullName: data['fullName'],
      phoneNumber: data['phoneNumber'],
      address: data['address'],
      yearsExperience: data['yearsExperience'],
      description: data['description']
    };

    console.log('Update payload:', updatePayload);

    this.adminService.updateSpecialist(this.selectedUserId, updatePayload).subscribe({
      next: () => {
        console.log()
        this.closeUpdateUserForm();
        this.getPage();
      },
      error: (err) => {
        console.error('Eroare la actualizare:', err);
      }
    });
  }

  closeUserDetails() {
    this.isUserDetailsVisible = false;
    this.entityDetails = {};
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
  deleteUser(user: UserDTO): void {
    if (confirm(`Sigur doriți să ștergeți utilizatorul ${user.fullName}?`)) {
      this.adminService.deleteSpecialist(user.id).subscribe({
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

  // goToPreviousPage(): void {
  //   if (this.currentPage > 1) {
  //     this.currentPage--;
  //     this.getPage();
  //   }
  // }
  //
  // goToNextPage(): void {
  //   const totalPages = this.getTotalPages();
  //   if (this.currentPage < totalPages) {
  //     this.currentPage++;
  //     this.getPage();
  //   }
  // }

  getTotalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize);
  }

  onPageSizeChange(newSize: number): void {
    this.pageSize = newSize;
    this.currentPage = 1; // Reset to first page
    this.getPage();
  }

  onPageChange(newPage: number): void {
    this.currentPage = newPage;
    this.getPage();
  }
}
