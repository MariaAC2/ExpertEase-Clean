import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {UserDetailsDTO} from '../../models/api.models';
import {ActivatedRoute, Router} from '@angular/router';
import {UserService} from '../../services/user.service';
import {DatePipe, NgForOf, NgIf} from '@angular/common';

@Component({
  selector: 'app-specialist-details',
  imports: [
    DatePipe,
    NgIf,
    NgForOf
  ],
  templateUrl: './specialist-details.component.html',
  styleUrl: './specialist-details.component.scss'
})
export class SpecialistDetailsComponent implements OnInit {
  userDetails: UserDetailsDTO | null = null;
  userDetailsId: string = '';

  dummyUserDetails: UserDetailsDTO = {
    fullName: 'Maria Popescu',
    profilePictureUrl: 'assets/avatar.svg',
    rating: 4,
    reviews: [
      {
        id: '1',
        content: 'Serviciu excelent!',
        rating: 5,
        senderUserFullName: 'Ion Vasile',
        senderUserProfilePictureUrl: 'assets/avatar.svg',
        createdAt: new Date('2024-06-01T10:00:00'),
        updatedAt: new Date('2024-06-01T10:00:00'),
      },
      {
        id: '2',
        content: 'Foarte punctual și profesionist.',
        rating: 4,
        senderUserFullName: 'Andreea Dumitru',
        senderUserProfilePictureUrl: '',
        createdAt: new Date('2024-05-22T14:00:00'),
        updatedAt: new Date('2024-05-22T14:00:00')
      }
    ],
    // Specialist-only fields
    email: 'maria.popescu@example.com',
    phoneNumber: '0722123456',
    address: 'Str. Libertății, nr. 15',
    yearsExperience: 5,
    description: 'Instalator autorizat cu experiență în proiecte rezidențiale.',
    portfolio: ['Portofoliu1.jpg', 'Portofoliu2.jpg'],
    categories: ['Instalații sanitare', 'Reparații']
  };


  constructor(
    private readonly activatedRoute: ActivatedRoute,
    private readonly homeService: UserService
  ) {}

  ngOnInit() {
    this.userDetailsId = this.activatedRoute.snapshot.paramMap.get('id')!;
    this.userDetails = this.dummyUserDetails;
  //   this.homeService.getUserDetails(this.userDetailsId).subscribe({
  //     next: (res) => {
  //       this.userDetails = res.response ?? null;
  //     },
  //     error: (err) => {
  //       console.error('Eroare la preluarea utilizatorului:', err);
  //       alert('Nu s-au putut încărca detaliile utilizatorului.');
  //     }
  //   });
  }

  onCloseDetails() {
    // this.closeDetails.emit();
    window.history.back();
  }
}
