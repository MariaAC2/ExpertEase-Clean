import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {SpecialistDTO} from '../../models/api.models';
import {ActivatedRoute, Router} from '@angular/router';
import {HomeService} from '../../services/home.service';

@Component({
  selector: 'app-specialist-details',
  imports: [],
  templateUrl: './specialist-details.component.html',
  styleUrl: './specialist-details.component.scss'
})
export class SpecialistDetailsComponent implements OnInit {
  // @Output() closeDetails = new EventEmitter<void>();

  specialist: SpecialistDTO | null = null;
  specialistId: string = '';

  dummySpecialist: SpecialistDTO = {
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
  };

  constructor(
    private route: Router,
    private activatedRoute: ActivatedRoute,
    private homeService: HomeService
  ) {}

  ngOnInit() {
    this.specialistId = this.activatedRoute.snapshot.paramMap.get('id')!;
    // this.specialist = this.dummySpecialist;
    this.homeService.getSpecialist(this.specialistId).subscribe({
      next: (res) => {
        this.specialist = res.response ?? null;
      },
      error: (err) => {
        console.error('Eroare la preluarea utilizatorului:', err);
        alert('Nu s-au putut încărca detaliile utilizatorului.');
      }
    });
  }

  onCloseDetails() {
    // this.closeDetails.emit();
    window.history.back();
  }
}
