import {Component, EventEmitter, Input, Output} from '@angular/core';
import {CommonModule} from '@angular/common';
import {Router} from '@angular/router';
import {SpecialistDTO} from '../../models/api.models';

@Component({
  selector: 'app-specialist-card',
  imports: [CommonModule],
  templateUrl: './specialist-card.component.html',
  styleUrls: ['./specialist-card.component.scss']
})
export class SpecialistCardComponent {
  // @Input() specialistId!: string;
  // @Input() fullName!: string;
  // @Input() categories: string[] = [];
  // @Input() yearsExperience!: string;
  // @Input() description!: string;
  // @Input() rating: number = 5; // optional: stars (0–5)
  @Input() specialist: SpecialistDTO = {
    id: '',
    fullName: '',
    email: '',
    phoneNumber: '',
    address: '',
    categories: [],
    yearsExperience: 0,
    description: '',
    createdAt: new Date(),
    updatedAt: new Date(),
  }

  constructor(private router: Router) { }

  goToDetails() {
    this.router.navigate(['/specialist-details']);
  }

  goToSendRequest() {
    this.router.navigate(['/request-form']);
  }
}
