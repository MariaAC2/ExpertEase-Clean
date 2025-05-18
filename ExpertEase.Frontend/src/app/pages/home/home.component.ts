import { Component } from '@angular/core';
import {SpecialistCardComponent} from '../../shared/specialist-card/specialist-card.component';
import {UserSpecialistDTO} from '../../models/api.models';
import {CommonModule} from '@angular/common';
import {dtoToDictionary} from '../../models/form.models';
import {HomeService} from '../../services/home.service';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {SearchInputComponent} from '../../shared/search-input/search-input.component';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [SpecialistCardComponent, CommonModule, ReactiveFormsModule, FormsModule, SearchInputComponent],
  templateUrl: './home.component.html',
  styleUrls: ['./home.component.scss', '../../shared/search-input/search-input.component.scss']
})
export class HomeComponent {
  searchTerm: string = '';
  dummyUsers: UserSpecialistDTO[] = [
    {
      id: '1',
      firstName: 'Ana',
      lastName: 'Popescu',
      email: 'ana.popescu@example.com',
      password: 'pass123',
      phoneNumber: '0712345678',
      address: 'Strada Florilor 12, București',
      yearsExperience: 5,
      description: 'Specialist în design interior cu experiență în proiecte rezidențiale.',
      categories: [
        { id: 'cat1', name: 'Design interior' },
        { id: 'cat2', name: 'Renovări' }
      ]
    },
    {
      id: '2',
      firstName: 'Mihai',
      lastName: 'Ionescu',
      email: 'mihai.ionescu@example.com',
      password: 'secret456',
      phoneNumber: '0722333444',
      address: 'Aleea Lalelelor 8, Cluj-Napoca',
      yearsExperience: 8,
      description: 'Electrician autorizat ANRE, specializat în instalații electrice industriale.',
      categories: [
        { id: 'cat3', name: 'Electricitate' },
        { id: 'cat4', name: 'Instalații industriale' }
      ]
    },
    {
      id: '3',
      firstName: 'Elena',
      lastName: 'Georgescu',
      email: 'elena.geo@example.com',
      password: 'elenaPass!',
      phoneNumber: '0733445566',
      address: 'Bd. Unirii 45, Iași',
      yearsExperience: 10,
      description: 'Consultant în resurse umane, coaching și formare profesională.',
      categories: [
        { id: 'cat5', name: 'Resurse umane' },
        { id: 'cat6', name: 'Coaching' }
      ]
    }
  ];
  currentPage: number = 1;
  pageSize: number = 10;
  totalItems: number = 0;
  pageSizeOptions: number[] = [5, 10, 20, 50];
  entityDetailsId: string | undefined;
  entityDetails: Record<string, any> = {};
  isUserDetailsVisible = false;
  users: UserSpecialistDTO[] = [];
  error: string | null = null;

  constructor(private homeService: HomeService) { }

  getEntity(userId: string): void {
    this.homeService.getSpecialist(userId).subscribe({
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

  onSearch(term: string): void {
    this.searchTerm = term;
    this.currentPage = 1;
    this.getPage();
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

  getCategoryNames(categories: { id: string; name: string }[]): string[] {
    return categories.map(c => c.name);
  }

  closeUserDetails() {
    this.isUserDetailsVisible = false;
    this.entityDetails = {};
  }
}
