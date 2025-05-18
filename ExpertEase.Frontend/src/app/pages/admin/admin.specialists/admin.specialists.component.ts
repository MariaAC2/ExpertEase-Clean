import { Component } from '@angular/core';
import {FormsModule} from '@angular/forms';
import {NgForOf, NgIf} from '@angular/common';
import {
  UserAddDTO,
  UserDTO,
  UserRoleEnum,
  UserSpecialistAddDTO,
  UserSpecialistDTO, UserSpecialistUpdateDTO,
  UserUpdateDTO
} from '../../../models/api.models';
import {TableColumn} from '../../../models/table.models';
import {dtoToDictionary, dtoToFormFields} from '../../../models/form.models';
import {AdminUsersService} from '../../../services/admin.users.service';
import {AdminSpecialistsService} from '../../../services/admin.specialists.service';
import {AdminDetailsComponent} from '../../../shared/admin-details/admin-details.component';
import {DynamicFormComponent} from '../../../shared/dynamic-form/dynamic-form.component';
import {DynamicTableComponent} from '../../../shared/dynamic-table/dynamic-table.component';
import {SearchInputComponent} from '../../../shared/search-input/search-input.component';

@Component({
  selector: 'app-admin.users',
  imports: [
    FormsModule,
    AdminDetailsComponent,
    DynamicFormComponent,
    DynamicTableComponent,
    NgForOf,
    NgIf,
    SearchInputComponent
  ],
  templateUrl: './admin.specialists.component.html',
  styleUrls: ['../../../shared/dynamic-table/dynamic-table.component.scss', '../../../shared/admin-details/admin-details.component.scss'],
})
export class AdminSpecialistsComponent {
  dummySpecialists: UserSpecialistDTO[] = [
    {
      id: '1a2b3c4d',
      firstName: 'Elena',
      lastName: 'Popa',
      email: 'elena.popa@example.com',
      password: 'dummyPass123!',
      phoneNumber: '+40741111222',
      address: 'Str. Mihai Eminescu, nr. 10, București',
      yearsExperience: 5,
      description: 'Specialist în educație timpurie cu experiență în dezvoltarea copilului.',
      categories: []
    },
    {
      id: '2b3c4d5e',
      firstName: 'Alexandru',
      lastName: 'Ionescu',
      email: 'alex.ionescu@example.com',
      password: 'dummyPass123!',
      phoneNumber: '+40741222333',
      address: 'Bd. Unirii, nr. 45, Cluj-Napoca',
      yearsExperience: 8,
      description: 'Consilier vocațional pasionat de ghidarea carierei adolescenților.',
      categories: []
    },
    {
      id: '3c4d5e6f',
      firstName: 'Mara',
      lastName: 'Dumitrescu',
      email: 'mara.dumitrescu@example.com',
      password: 'dummyPass123!',
      phoneNumber: '+40741333444',
      address: 'Str. Libertății, nr. 12, Timișoara',
      yearsExperience: 3,
      description: 'Psiholog specializat în coaching parental.',
      categories: []
    },
    {
      id: '4d5e6f7g',
      firstName: 'Radu',
      lastName: 'Georgescu',
      email: 'radu.georgescu@example.com',
      password: 'dummyPass123!',
      phoneNumber: '+40741444555',
      address: 'Calea Victoriei, nr. 77, Brașov',
      yearsExperience: 10,
      description: 'Mentor în educația non-formală cu experiență în proiecte Erasmus+.',
      categories: []
    },
    {
      id: '5e6f7g8h',
      firstName: 'Ioana',
      lastName: 'Stan',
      email: 'ioana.stan@example.com',
      password: 'dummyPass123!',
      phoneNumber: '+40741555666',
      address: 'Str. Lalelelor, nr. 23, Iași',
      yearsExperience: 2,
      description: 'Trainer junior dedicat activităților creative pentru copii.',
      categories: []
    }
  ];

dummy_user = {
  id: '1a2b3c4d',
  firstName: 'Elena',
  lastName: 'Popa',
  email: 'elena.popa@example.com',
  password: 'dummyPass123!',
  phoneNumber: '+40741111222',
  address: 'Str. Mihai Eminescu, nr. 10, București',
  yearsExperience: 5,
  description: 'Specialist în educație timpurie cu experiență în dezvoltarea copilului.',
  categories: []
}
  searchTerm: string = '';
  users: UserSpecialistDTO[] = [];
  selectedUser: UserSpecialistDTO | null = null;
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
      compute: (row: UserDTO) => `${row.lastName} ${row.firstName}`
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
          callback: (row: UserSpecialistDTO) => this.getEntity(row.id)
        },
        {
          label: 'Modifică',
          class: 'edit',
          callback: (row: UserSpecialistDTO) => this.editUser(row)
        },
        {
          label: 'Șterge',
          class: 'delete',
          callback: (row) => this.deleteUser(row)
        }
      ]
    }
  ];

  defaultUser: UserSpecialistAddDTO = {
    firstName: '',
    lastName: '',
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
    description: { type: 'textarea', placeholder: 'Descrie serviciile oferite' }
  });

  updateEntityFormFields = dtoToFormFields(
    {
      firstName: this.selectedUser?.firstName || '',
      lastName: this.selectedUser?.lastName || '',
      phoneNumber: this.selectedUser?.phoneNumber || '',
      address: this.selectedUser?.address || '',
      yearsExperience: this.selectedUser?.yearsExperience || '',
      description: this.selectedUser?.description || ''
    }
  );

  constructor(private adminService: AdminSpecialistsService) {}

  ngOnInit(): void {
    const defaultFormValues: UserSpecialistAddDTO = {
      firstName: '',
      lastName: '',
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
    this.adminService.getSpecialists(this.searchTerm, this.currentPage, this.pageSize).subscribe({
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
    const userToSubmit: UserSpecialistAddDTO = data as UserSpecialistAddDTO;
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

  editUser(user: UserSpecialistDTO): void {
    this.selectedUserId = user.id;
    this.formData = {
      firstName: user.firstName,
      lastName: user.lastName,
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

    const updatePayload: UserSpecialistUpdateDTO = {
      id: this.selectedUserId,
      firstName: data['firstName'],
      lastName: data['lastName'],
      password: data['password'],
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
    if (confirm(`Sigur doriți să ștergeți utilizatorul ${user.firstName} ${user.lastName}?`)) {
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

  goToPreviousPage(): void {
    if (this.currentPage > 1) {
      this.currentPage--;
      this.getPage();
    }
  }

  goToNextPage(): void {
    const totalPages = this.getTotalPages();
    if (this.currentPage < totalPages) {
      this.currentPage++;
      this.getPage();
    }
  }

  getTotalPages(): number {
    return Math.ceil(this.totalItems / this.pageSize);
  }

  onPageSizeChange(newSize: number): void {
    this.pageSize = newSize;
    this.currentPage = 1; // Reset to first page
    this.getPage();
  }

}
