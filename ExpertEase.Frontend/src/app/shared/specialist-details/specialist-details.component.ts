import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {UserDetailsDTO} from '../../models/api.models';
import {ActivatedRoute, Router} from '@angular/router';
import {UserService} from '../../services/user.service';
import {NgForOf, NgIf, NgTemplateOutlet} from '@angular/common';

@Component({
  selector: 'app-specialist-details',
  imports: [
    NgIf,
    NgForOf,
    NgTemplateOutlet
  ],
  templateUrl: './specialist-details.component.html',
  styleUrl: './specialist-details.component.scss'
})
export class SpecialistDetailsComponent implements OnInit {
  userDetails: UserDetailsDTO | null = null;
  userDetailsId: string = '';
  showOverlay: boolean = false;

  dummyUserDetails: UserDetailsDTO = {
    fullName: 'Maria Popescu',
    profilePictureUrl: 'assets/avatar.svg',
    rating: 4,
    reviews: [
      {
        id: '1',
        content: 'Serviciu excelent! Maria a venit punctual și a rezolvat problema cu instalația sanitară foarte profesionist. Recomand cu încredere!',
        rating: 5,
        senderUserFullName: 'Ion Vasile',
        senderUserProfilePictureUrl: 'assets/avatar.svg',
        createdAt: new Date('2024-06-01T10:00:00'),
        updatedAt: new Date('2024-06-01T10:00:00'),
      },
      {
        id: '2',
        content: 'Foarte punctual și profesionist. A explicat tot ce a făcut și a lăsat totul foarte curat.',
        rating: 4,
        senderUserFullName: 'Andreea Dumitru',
        senderUserProfilePictureUrl: '',
        createdAt: new Date('2024-05-22T14:00:00'),
        updatedAt: new Date('2024-05-22T14:00:00')
      },
      {
        id: '3',
        content: 'Foarte mulțumit de serviciile oferite. Prețuri corecte și muncă de calitate.',
        rating: 5,
        senderUserFullName: 'Mihai Georgescu',
        senderUserProfilePictureUrl: 'assets/avatar.svg',
        createdAt: new Date('2024-05-15T16:30:00'),
        updatedAt: new Date('2024-05-15T16:30:00')
      }
    ],
    // Specialist-only fields
    email: 'maria.popescu@example.com',
    phoneNumber: '0722123456',
    address: 'Str. Libertății, nr. 15, Sector 2, București',
    yearsExperience: 5,
    description: 'Instalator autorizat cu experiență în proiecte rezidențiale și comerciale. Specializată în instalații sanitare moderne și reparații de urgență.',
    portfolio: ['Portofoliu1.jpg', 'Portofoliu2.jpg'],
    categories: ['Instalații sanitare', 'Reparații', 'Urgențe']
  };

  constructor(
    private readonly activatedRoute: ActivatedRoute,
    private readonly userService: UserService,
    private readonly router: Router
  ) {}

  ngOnInit() {
    this.userDetailsId = this.activatedRoute.snapshot.paramMap.get('id')!;

    // Check if we're in overlay mode (coming from specialist card)
    this.showOverlay = this.router.url.includes('overlay') ||
      this.activatedRoute.snapshot.queryParams['overlay'] === 'true';

    // this.userDetails = this.dummyUserDetails;
    this.userService.getUserDetails(this.userDetailsId).subscribe({
      next: (res) => {
        this.userDetails = res.response ?? null;
      },
      error: (err) => {
        console.error('Eroare la preluarea utilizatorului:', err);
        alert('Nu s-au putut încărca detaliile utilizatorului.');
      }
    });
  }

  onCloseDetails() {
    if (this.showOverlay) {
      // If in overlay mode, go back to previous page
      window.history.back();
    } else {
      // If in standalone mode, navigate to home or specialists list
      this.router.navigate(['/specialists']);
    }
  }

  getStarArray(rating: number): boolean[] {
    return Array.from({length: 5}, (_, i) => i < rating);
  }

  formatPhoneNumber(phone: string): string {
    // Format Romanian phone number for display
    const cleaned = phone.replace(/\D/g, '');
    if (cleaned.length === 10 && cleaned.startsWith('07')) {
      return `${cleaned.slice(0, 4)} ${cleaned.slice(4, 7)} ${cleaned.slice(7)}`;
    }
    return phone;
  }

  getTimeSince(date: Date): string {
    const now = new Date();
    const diffInMs = now.getTime() - date.getTime();
    const diffInDays = Math.floor(diffInMs / (1000 * 60 * 60 * 24));

    if (diffInDays === 0) return 'Astăzi';
    if (diffInDays === 1) return 'Ieri';
    if (diffInDays < 7) return `Acum ${diffInDays} zile`;
    if (diffInDays < 30) return `Acum ${Math.floor(diffInDays / 7)} săptămâni`;
    return `Acum ${Math.floor(diffInDays / 30)} luni`;
  }
}
