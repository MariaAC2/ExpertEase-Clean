import {Component, OnInit} from '@angular/core';
import {RequestAddDTO, SpecialistDTO} from '../../models/api.models';
import {CommonModule} from '@angular/common';
import {dtoToDictionary} from '../../models/form.models';
import {HomeService} from '../../services/home.service';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {SearchInputComponent} from '../../shared/search-input/search-input.component';
import {PaginationComponent} from '../../shared/pagination/pagination.component';
import {UserRequestService} from '../../services/user.request.service';
import {SpecialistCardComponent} from '../../shared/specialist-card/specialist-card.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [SpecialistCardComponent, CommonModule, ReactiveFormsModule, FormsModule, SearchInputComponent, PaginationComponent],
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
  selectedSpecialist: SpecialistDTO | null = null;

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

  receiverUserId: string | undefined = '';

  constructor(private homeService: HomeService, private userRequestService: UserRequestService) { }

  closeDetails() {
    this.isUserDetailsVisible = false;
  }

  openRequestForm(specialist: SpecialistDTO) {
    this.isRequestFormVisible = true;
    this.selectedSpecialist = specialist;
    this.requestForm.receiverUserId = specialist.id;
  }

  ngOnInit() {
    this.getPage();
  }

  getEntity(userId: string): void {
    this.homeService.getSpecialist(userId).subscribe({
      next: (res) => {
        this.selectedSpecialist = res.response ?? null;
        this.receiverUserId = this.selectedSpecialist?.id
      },
      error: (err) => {
        console.error('Eroare la preluarea utilizatorului:', err);
        alert('Nu s-au putut încărca detaliile utilizatorului.');
      }
    });
  }

  getPage(): void {
    this.homeService.getSpecialists(this.searchTerm, this.currentPage, this.pageSize).subscribe({
      next: (res) => {
        this.users = res.response?.data ?? [];
        this.totalItems = res.response?.totalCount ?? 0;
      },
      error: (err) => {
        this.error = err.error?.errorMessage?.message || 'A apărut o eroare.';
      }
    });
  }

  addRequest(data: { [key: string]: any }) {
    const requestToSubmit : RequestAddDTO = data as RequestAddDTO;

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
    this.currentPage = 1;
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
