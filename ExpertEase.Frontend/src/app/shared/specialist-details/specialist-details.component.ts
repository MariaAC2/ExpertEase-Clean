import { Component } from '@angular/core';

@Component({
  selector: 'app-specialist-details',
  imports: [],
  templateUrl: './specialist-details.component.html',
  styleUrl: './specialist-details.component.scss'
})
export class SpecialistDetailsComponent {
  dummySpecialist = {
    firstName: 'Ioana',
    lastName: 'Dumitrescu',
    email: 'dumitrescu@gmail.com',
    phoneNumber: '0123456789',
    address: 'Strada Lalelelor nr 5',
    yearsExperience: 7,
    categories: ['Instalații sanitare', 'Reparații chiuvete', 'Montaj'],
    description: 'Specialistă în instalații sanitare cu experiență în intervenții de urgență, reparații și montaj în gospodării și spații comerciale.',
  };
}
