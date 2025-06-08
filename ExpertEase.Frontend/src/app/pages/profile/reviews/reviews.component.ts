import {Component, OnInit} from '@angular/core';
import {DatePipe, NgForOf, NgIf} from '@angular/common';
import {ReviewDTO} from '../../../models/api.models';
import {ReviewService} from '../../../services/review.service';

@Component({
  selector: 'app-reviews',
  imports: [
    NgForOf,
    NgIf,
    DatePipe
  ],
  templateUrl: './reviews.component.html',
  styleUrl: './reviews.component.scss'
})
export class ReviewsComponent implements OnInit {
  reviews: ReviewDTO[] = [];
  page = 1;
  pageSize = 5;

  dummyReviews: ReviewDTO[] = [
    {
      id: '1',
      receiverUserId: 'user-123',
      senderUserFullName: 'Ana Popescu',
      senderUserProfilePictureUrl: 'https://randomuser.me/api/portraits/women/21.jpg',
      rating: 5,
      content: 'Excelent! A fost foarte profesionistă și m-a ajutat rapid.',
      createdAt: new Date('2024-12-05T14:35:00'),
      updatedAt: new Date('2024-12-05T14:35:00')
    },
    {
      id: '2',
      receiverUserId: 'user-456',
      senderUserFullName: 'Andrei Ionescu',
      senderUserProfilePictureUrl: 'https://randomuser.me/api/portraits/men/32.jpg',
      rating: 4,
      content: 'Serviciu bun, dar am avut o mică întârziere la început.',
      createdAt: new Date('2025-01-10T09:20:00'),
      updatedAt: new Date('2025-01-10T09:20:00')
    },
    {
      id: '3',
      receiverUserId: 'user-789',
      senderUserFullName: 'Mihai Georgescu',
      senderUserProfilePictureUrl: 'https://randomuser.me/api/portraits/men/45.jpg',
      rating: 3,
      content: 'Experiență ok, dar se poate îmbunătăți comunicarea.',
      createdAt: new Date('2025-03-18T17:45:00'),
      updatedAt: new Date('2025-03-18T17:45:00')
    },
    {
      id: '4',
      receiverUserId: 'user-123',
      senderUserFullName: 'Elena Tudor',
      senderUserProfilePictureUrl: 'https://randomuser.me/api/portraits/women/55.jpg',
      rating: 5,
      content: 'Foarte mulțumită! Recomand cu încredere!',
      createdAt: new Date('2025-04-02T11:10:00'),
      updatedAt: new Date('2025-04-02T11:10:00')
    },
    {
      id: '5',
      receiverUserId: 'user-999',
      senderUserFullName: 'Radu Vasile',
      senderUserProfilePictureUrl: 'https://randomuser.me/api/portraits/men/61.jpg',
      rating: 2,
      content: 'Din păcate, nu a fost ce am așteptat. Probleme de punctualitate.',
      createdAt: new Date('2025-02-14T08:30:00'),
      updatedAt: new Date('2025-02-14T08:30:00')
    },
    {
      id: '6',
      receiverUserId: 'user-888',
      senderUserFullName: 'Ioana Marinescu',
      senderUserProfilePictureUrl: 'https://randomuser.me/api/portraits/women/88.jpg',
      rating: 4,
      content: 'Totul a decurs bine, dar aș fi vrut mai multe explicații.',
      createdAt: new Date('2025-05-22T15:50:00'),
      updatedAt: new Date('2025-05-22T15:50:00')
    }
  ];


  constructor(private reviewService: ReviewService) {}

  ngOnInit(): void {
    this.reviews = this.dummyReviews; // For testing purposes
    this.loadReviews();
  }

  loadReviews(): void {
    this.reviewService.getReviews(this.page, this.pageSize).subscribe({
      next: (res) => {
        this.reviews = res.response?.data || [];
      },
      error: (err) => {
        console.error('Error loading reviews', err);
      }
    });
  }
}
