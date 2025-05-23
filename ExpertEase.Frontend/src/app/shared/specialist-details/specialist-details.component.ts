import {Component, EventEmitter, Input, Output} from '@angular/core';
import {SpecialistDTO} from '../../models/api.models';
import {ActivatedRoute} from '@angular/router';
import {HomeService} from '../../services/home.service';

@Component({
  selector: 'app-specialist-details',
  imports: [],
  templateUrl: './specialist-details.component.html',
  styleUrl: './specialist-details.component.scss'
})
export class SpecialistDetailsComponent {
  @Input() specialist: SpecialistDTO | null = null;
  @Output() closeDetails = new EventEmitter<void>();

  constructor(
    private route: ActivatedRoute,
    private homeService: HomeService
  ) {}

  onCloseDetails() {
    this.closeDetails.emit();
  }
}
