import {AfterViewInit, Component} from '@angular/core';
import {CurrencyPipe, DatePipe} from '@angular/common';
import {JobStatusEnum, ServiceTaskDTO, SpecialistDTO} from '../../models/api.models';
import {FormsModule} from '@angular/forms';
import { loadStripe, StripeCardElement } from '@stripe/stripe-js';

@Component({
  selector: 'app-service-payment',
  imports: [
    FormsModule
  ],
  templateUrl: './service-payment.component.html',
  styleUrl: './service-payment.component.scss'
})
export class ServicePaymentComponent implements AfterViewInit{
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

  ngAfterViewInit(): void {
    this.setupStripe(); // call an async method
  }
  private async setupStripe() {
    const stripe = await loadStripe('pk_test_51RY4TaRP4R8qcMUlWSJwbW6GBjetIiG7jc4fLrsUcl7xMS8uTMcI2mfDHId8YRAku8lllViJiAY0mVPObrvgLYke00QP5RVa7S'); // public key
    if (!stripe) {
      console.error('Stripe failed to initialize.');
      return;
    }
    const elements = stripe.elements();
    const cardElement = elements.create('card');
    cardElement.mount('#card-element');

    const response = await fetch('http://localhost:5241/api/stripe/account/create-payment-intent', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' }
    });
    const { clientSecret } = await response.json();

    console.log('Client secret:', clientSecret);

    const form = document.getElementById('payment-form');
    if (!form) {
      console.error('Payment form not found.');
      return;
    }
    form.addEventListener('submit', async (event) => {
      event.preventDefault();
      const result = await stripe.confirmCardPayment(clientSecret, {
        payment_method: {
          card: cardElement, // of type StripeCardElement
          billing_details: {
            name: 'Test Client'
          }
        }
      });

      if (result.error) {
        console.error(result.error.message);
      } else if (result.paymentIntent.status === 'succeeded') {
        alert('Plată reușită!');
      }
    });
  }

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
