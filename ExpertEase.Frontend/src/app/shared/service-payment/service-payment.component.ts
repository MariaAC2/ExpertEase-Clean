import { Component } from '@angular/core';
import {CurrencyPipe, DatePipe} from '@angular/common';
import {JobStatusEnum, ServiceTaskDTO, SpecialistDTO} from '../../models/api.models';
import {FormsModule} from '@angular/forms';

@Component({
  selector: 'app-service-payment',
  imports: [
    CurrencyPipe,
    DatePipe,
    FormsModule
  ],
  templateUrl: './service-payment.component.html',
  styleUrl: './service-payment.component.scss'
})
export class ServicePaymentComponent {
  serviceTask: ServiceTaskDTO = {
    id: '12345',
    replyId: '67890',
    userId: 'user123',
    specialistId: 'spec456',
    description: 'Montare mobilier',
    address: 'Str. Exemplu 123',
    startDate: new Date(),
    endDate: new Date(),
    specialistFullName: 'Ion Popescu',
    price: 350,
    status: JobStatusEnum.Confirmed,
    completedAt: new Date(),
    cancelledAt: new Date(),
  };


  cardPaymentSelected = false;
  showError = false;

  selectCardPayment() {
    this.cardPaymentSelected = true;
    this.showError = false;
  }

  payForService() {
    if (!this.cardPaymentSelected) {
      this.showError = true;
      return;
    }
    console.log('Plată inițiată cu cardul bancar.');
    window.open('https://www.plata-cu-cardul.ro', '_blank');
  }
}
