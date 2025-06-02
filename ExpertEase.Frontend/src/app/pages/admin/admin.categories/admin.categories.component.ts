import {Component, OnInit} from '@angular/core';
import {AdminDetailsComponent} from '../../../shared/admin-details/admin-details.component';
import {DynamicFormComponent} from '../../../shared/dynamic-form/dynamic-form.component';
import {DynamicTableComponent} from '../../../shared/dynamic-table/dynamic-table.component';
import {NgIf} from '@angular/common';
import {SearchInputComponent} from '../../../shared/search-input/search-input.component';
import {
  CategoryAddDTO,
  CategoryAdminDTO, CategoryUpdateDTO, UserRoleEnum,
} from '../../../models/api.models';
import {dtoToDictionary, dtoToFormFields} from '../../../models/form.models';
import {CategoriesService} from '../../../services/categories.service';
import {TableColumn} from '../../../models/table.models';
import {PaginationComponent} from '../../../shared/pagination/pagination.component';

@Component({
  selector: 'app-admin.categories',
  imports: [
    AdminDetailsComponent,
    DynamicFormComponent,
    SearchInputComponent,
    DynamicTableComponent,
    PaginationComponent,
    NgIf
  ],
  templateUrl: './admin.categories.component.html',
  styleUrls: ['../../../shared/dynamic-table/dynamic-table.component.scss', '../../../shared/admin-details/admin-details.component.scss']
})
export class AdminCategoriesComponent implements OnInit {

  dummy_categories: CategoryAdminDTO[] = [
    {
      id: 'c1',
      name: 'Psihologie',
      description: 'Categoria pentru specialiști în psihologie',
      specialistsCount: 5,
      specialistIds: ['s1', 's2', 's3', 's4', 's5']
    },
    {
      id: 'c2',
      name: 'Nutriție',
      description: 'Categoria pentru specialiști în nutriție',
      specialistsCount: 3,
      specialistIds: ['s6', 's7', 's8']
    }
  ];
  searchTerm: string = '';
  entities: CategoryAdminDTO[] = [];
  selectedEntity: CategoryAdminDTO | null = null;
  entityDetails: Record<string, any> = {};
  entityDetailsId: string | undefined;
  pageSize: number = 10;
  currentPage: number = 1;
  totalItems: number = 0;

  isAddEntityFormVisible = false;
  isUpdateEntityFormVisible = false;
  isEntityDetailsVisible = false;
  selectedEntityId: string | null = null;

  columns: TableColumn[] = [
    { key: 'id', header: 'ID' },
    {
      key: 'name',
      header: 'Denumire',
    },
    { key: 'description', header: 'Descriere' },
    { key: 'specialistsCount', header: 'Număr specialiști' },
    { key: 'specialistIds', header: 'Specialiștii cu această categorie' },
    {
      key: 'actions',
      header: 'Acțiuni',
      type: 'action',
      actions: [
        {
          label: 'Detalii',
          class: 'view',
          callback: (row: CategoryAdminDTO) => this.getEntity(row.id)
        },
        {
          label: 'Modifică',
          class: 'edit',
          callback: (row: CategoryAdminDTO) => this.editEntity(row)
        },
        {
          label: 'Șterge',
          class: 'delete',
          callback: (row) => this.deleteEntity(row)
        }
      ]
    }
  ];

  defaultEntity: CategoryAddDTO = {
    name: '',
    description: ''
  };
  formData: { [key: string]: any } = {};
  error: string | null = null;

  addEntityFormFields = dtoToFormFields(this.defaultEntity, {
    description: { type: 'input', placeholder: 'Descrie categoria' }
  });

  updateEntityFormFields = dtoToFormFields(
    {
      name: this.selectedEntity?.name || '',
      description: this.selectedEntity?.description || '',
    },
  );
  constructor(private adminService: CategoriesService) {}

  ngOnInit(): void {
    const defaultFormValues: CategoryAddDTO = {
      name: '',
      description: ''
    };

    this.formData = { ...defaultFormValues };
    this.getPage();
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.currentPage = 1;
    this.getPage();
  }

  getEntity(categoryId: string): void {
    console.log(categoryId);
    this.adminService.getCategory(categoryId).subscribe({
      next: (res) => {
        // console.log(res.response);
        this.entityDetailsId = res.response?.id;
        this.entityDetails = dtoToDictionary(res.response ?? {});
        this.isEntityDetailsVisible = true;
      },
      error: (err) => {
        console.error('Eroare la preluarea utilizatorului:', err.errorMessage?.status);
        alert('Nu s-au putut încărca detaliile utilizatorului.');
      }
    });
  }

  getPage(): void {
    this.adminService.getCategoriesAdmin(this.searchTerm, this.currentPage, this.pageSize).subscribe({
      next: (res) => {
        console.log(res.response);
        this.entities = res.response?.data ?? [];
        this.totalItems = res.response?.totalCount ?? 0;
      },
      error: (err) => {
        this.error = err.error?.errorMessage?.message || 'A apărut o eroare.';
      }
    });
  }

  addEntity(data: { [key: string]: any }) {
    const categoryToSubmit: CategoryAddDTO = data as CategoryAddDTO;
    this.adminService.addCategory(categoryToSubmit).subscribe({
      next: () => {
        this.closeAddEntityForm();
        this.getPage(); // refresh category list
      },
      error: (err) => {
        console.error('Eroare la adăugarea utilizatorului:', err);
      }
    });
  }

  editEntity(category: CategoryAdminDTO): void {
    this.selectedEntityId = category.id;

    this.formData = {
      name: category.name,
      description: category.description
    };

    this.isUpdateEntityFormVisible = true;
  }

  updateEntity(data: { [key: string]: any }) {
    if (!this.selectedEntityId) {
      console.error('Niciun utilizator nu a fost selectat pentru actualizare.');
      alert('Eroare: Nu a fost selectat niciun utilizator pentru actualizare.'); // or use a proper toast
      return;
    }

    const updatePayload: CategoryUpdateDTO = {
      id: this.selectedEntityId,
      name: data['name'] || this.formData['name'],
      description: data['description'] || this.formData['description']
    };

    this.adminService.updateCategory(this.selectedEntityId, updatePayload).subscribe({
      next: () => {
        this.closeUpdateEntityForm();
        this.getPage();
      },
      error: (err) => {
        console.error('Eroare la actualizare:', err);
      }
    });
  }

  showEntityDetails(category: CategoryAdminDTO) {
    // this.selectedEntity = category;
    this.isEntityDetailsVisible = true;
  }

  closeEntityDetails() {
    this.isEntityDetailsVisible = false;
    this.selectedEntity = null;
  }

  closeUpdateEntityForm() {
    this.selectedEntityId = null;
    this.resetForm(); // clears formData
    this.isUpdateEntityFormVisible = false;
  }
  openAddEntityForm(): void {
    this.selectedEntityId = null;
    this.resetForm(); // clears formData
    this.isAddEntityFormVisible = true;
  }

  viewEntity(category: CategoryAdminDTO): void {
    // Implement the logic to view category details
    this.isEntityDetailsVisible = true;
  }
  deleteEntity(category: CategoryAdminDTO): void {
    if (confirm(`Sigur doriți să ștergeți categoria ${category.name}?`)) {
      this.adminService.deleteCategory(category.id).subscribe({
        next: () => {
          this.getPage();
        },
        error: (err) => {
          console.error('Eroare la ștergerea utilizatorului:', err);
        }
      });
    }
  }
  closeAddEntityForm() {
    this.isAddEntityFormVisible = false;
    this.resetForm();
  }

  resetForm() {
    this.formData = {
      name: '',
      description: ''
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
