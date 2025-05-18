import { Component, Input } from '@angular/core';
import {CommonModule} from '@angular/common';
import {Router} from '@angular/router';

@Component({
  selector: 'app-specialist-card',
  imports: [CommonModule],
  templateUrl: './specialist-card.component.html',
  styleUrls: ['./specialist-card.component.scss']
})
export class SpecialistCardComponent {
  @Input() specialistId!: string;
  @Input() firstName!: string;
  @Input() lastName!: string;
  @Input() categories: string[] = [];
  @Input() yearsExperience!: string;
  @Input() description!: string;
  @Input() rating: number = 5; // optional: stars (0–5)

  constructor(private router: Router) { }

  goToDetails() {
    this.router.navigate(['/specialist-details']);
  }

  goToSendRequest() {
    this.router.navigate(['/request-form']);
  }

}
