import {Component, OnInit} from '@angular/core';
import {RequestAddDTO, SpecialistDTO, PaginationSearchQueryParams, SpecialistFilterParams} from '../../models/api.models';
import {CommonModule} from '@angular/common';
import {dtoToDictionary} from '../../models/form.models';
import {SpecialistService} from '../../services/specialist.service';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {SearchInputComponent} from '../../shared/search-input/search-input.component';
import {PaginationComponent} from '../../shared/pagination/pagination.component';
import {RequestService} from '../../services/request.service';
import {SpecialistCardComponent} from '../../shared/specialist-card/specialist-card.component';
import {RouterLink, RouterLinkActive} from '@angular/router';
import {AuthService} from '../../services/auth.service';
import {SearchFiltersComponent} from '../../shared/search-filters/search-filters.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [
    SpecialistCardComponent,
    CommonModule,
    ReactiveFormsModule,
    FormsModule,
    SearchInputComponent,
    PaginationComponent,
    RouterLink,
    RouterLinkActive,
    SearchFiltersComponent
  ],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss', '../../shared/search-input/search-input.component.scss']
})
export class HomeComponent implements OnInit{
  searchTerm: string = '';
  currentPage: number = 1;
  pageSize: number = 10;
  totalItems: number = 0;
  entityDetails: Record<string, any> = {};
  isUserDetailsVisible = false;
  users: SpecialistDTO[] = [];
  error: string | null = null;

  // Separate search and filter parameters
  searchParams: PaginationSearchQueryParams = {
    page: 1,
    pageSize: 10
  };

  filterParams: SpecialistFilterParams = {};

  isRequestFormVisible = false;

  requestForm: {
    receiverUserId: string;
    requestedStartDate: Date;
    phoneNumber: string;
    address: string;
    description: string;
  } = {
    receiverUserId: '',
    requestedStartDate: new Date(),
    phoneNumber: '',
    address: '',
    description: ''
  }

  dummySpecialists: SpecialistDTO[] = [
    {
      id: 'spec-001',
      fullName: 'Andrei Popescu',
      email: 'andrei.popescu@example.com',
      phoneNumber: '0721123456',
      address: 'Strada Mihai Eminescu 45, București',
      yearsExperience: 5,
      description: 'Traducător medical cu experiență în terminologie clinică și farmaceutică.',
      createdAt: new Date('2022-03-10T10:00:00Z'),
      updatedAt: new Date(),
      rating: 5,
      categories: [
        { id: 'cat-001', name: 'Traduceri medicale' },
        { id: 'cat-002', name: 'Farmacie' }
      ]
    },
    {
      id: 'spec-002',
      fullName: 'Ioana Marinescu',
      email: 'ioana.marinescu@example.com',
      phoneNumber: '0733456789',
      address: 'Bd. Revoluției 17, Cluj-Napoca',
      yearsExperience: 8,
      description: 'Specialist în psihologie cu experiență în redactarea și corectarea articolelor științifice.',
      createdAt: new Date('2020-11-20T08:30:00Z'),
      updatedAt: new Date(),
      rating: 4,
      // categories: [
      //   { id: 'cat-003', name: 'Psihologie' },
      //   { id: 'cat-004', name: 'Redactare academică' }
      // ]
    },
    {
      id: 'spec-003',
      fullName: 'Victor Ionescu',
      email: 'victor.ionescu@example.com',
      phoneNumber: '0744123123',
      address: 'Str. Libertății 88, Iași',
      yearsExperience: 3,
      description: 'Tânăr specialist în tehnologia informației, ofer servicii de consultanță și mentenanță software.',
      createdAt: new Date('2023-01-15T14:45:00Z'),
      updatedAt: new Date(),
      rating: 4.5,
      // categories: [
      //   { id: 'cat-005', name: 'IT & Software' },
      //   { id: 'cat-006', name: 'Consultanță tehnică' }
      // ]
    }
  ];

  constructor(private readonly homeService: SpecialistService,
              private readonly userRequestService: RequestService,
              private readonly authService: AuthService) { }

  ngOnInit() {
    this.getPage();
  }

  getPage(): void {
    // Update search params with pagination info
    this.searchParams.page = this.currentPage;
    this.searchParams.pageSize = this.pageSize;

    console.log('Searching with params:', this.searchParams, 'Filters:', this.filterParams);

    // this.users = this.dummySpecialists; // For testing purposes, using dummy data
    this.homeService.getSpecialists(this.searchParams, this.filterParams).subscribe({
      next: (res) => {
        this.users = res.response?.data ?? [];
        this.totalItems = res.response?.totalCount ?? 0;
        this.error = null;
      },
      error: (err) => {
        this.error = err.error?.errorMessage?.message || 'A apărut o eroare.';
        console.error('Error loading specialists:', err);
      }
    });
  }

  addRequest(data: { [key: string]: any }) {
    const requestToSubmit : RequestAddDTO = data as RequestAddDTO;
    console.log('Request to submit:', requestToSubmit);

    this.userRequestService.addRequest(requestToSubmit).subscribe({
      next: (res) => {
        alert('Cererea a fost trimisă cu succes!');
        this.closeRequestForm();
      },
      error: (err) => {
        console.error('Eroare la trimiterea cererii:', err);
        alert('Nu s-a putut trimite cererea.');
      }
    });
  }

  onSearch(term: string): void {
    this.searchTerm = term;
    this.searchParams.search = term || undefined;
    this.currentPage = 1;
    this.getPage();
  }

  onFiltersChange(filters: SpecialistFilterParams): void {
    console.log('Filters changed:', filters);

    // Update filter parameters
    this.filterParams = { ...filters };
    this.currentPage = 1; // Reset to first page when filters change
    this.getPage();
  }

  onPageChange(newPage: number): void {
    this.currentPage = newPage;
    this.getPage();
  }

  onPageSizeChange(newSize: number): void {
    this.pageSize = newSize;
    this.currentPage = 1; // Reset to first page
    this.getPage();
  }

  clearAllFilters(): void {
    this.filterParams = {};
    this.searchParams.search = undefined;
    this.searchTerm = '';
    this.currentPage = 1;
    this.getPage();
  }

  // Quick filter methods
  getTopRatedSpecialists(): void {
    this.filterParams = {
      ...this.filterParams,
      sortByRating: 'desc',
      minRating: 4.5
    };
    this.currentPage = 1;
    this.getPage();
  }

  getExperiencedSpecialists(): void {
    this.filterParams = {
      ...this.filterParams,
      experienceRange: '7-10'
    };
    this.currentPage = 1;
    this.getPage();
  }

  getHighRatedSpecialists(): void {
    this.filterParams = {
      ...this.filterParams,
      minRating: 4.5,
      sortByRating: 'desc'
    };
    this.currentPage = 1;
    this.getPage();
  }

  getCategoryNames(categories: { id: string; name: string }[]): string[] {
    return categories.map(c => c.name);
  }

  closeUserDetails() {
    this.isUserDetailsVisible = false;
    this.entityDetails = {};
  }

  closeRequestForm() {
    this.requestForm = {
      receiverUserId: '',
      requestedStartDate: new Date(),
      phoneNumber: '',
      address: '',
      description: ''
    }
    this.isRequestFormVisible = false;
  }

  protected readonly SpecialistCardComponent = SpecialistCardComponent;
}
