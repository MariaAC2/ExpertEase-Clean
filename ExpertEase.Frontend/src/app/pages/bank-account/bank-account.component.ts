import { Component } from '@angular/core';
import {AccountDTO, UserDTO} from '../../models/api.models';
import {ProfileService} from '../../services/profile.service';
import {CurrencyPipe} from '@angular/common';

@Component({
  selector: 'app-bank-account',
  imports: [
    CurrencyPipe
  ],
  templateUrl: './bank-account.component.html',
  styleUrl: './bank-account.component.scss'
})
export class BankAccountComponent {
  user: UserDTO | null = null;
  account = {
    id: 'acc-1234567890',
    currency: 'RON',
    balance: 1250.75
  };
  constructor(private userService: ProfileService) {}

  ngOnInit(): void {
    this.loadUser();
  }

  loadUser(): void {
    // this.userService.getCurrentUser().subscribe({
    //   next: (data) => this.user = data,
    //   error: (err) => console.error('Failed to load user', err)
    // });
  }

  onDeposit(): void {
    // const amount = prompt('Introduce suma de depus:');
    // const value = Number(amount);
    // if (!isNaN(value) && value > 0) {
    //   this.userService.deposit(value).subscribe({
    //     next: () => this.loadUser(),
    //     error: (err) => alert('Eroare la depunere: ' + err.message)
    //   });
    // } else {
    //   alert('Suma introdusă nu este validă.');
    // }
  }

  onWithdraw(): void {
    // const amount = prompt('Introduce suma de retras:');
    // const value = Number(amount);
    // if (!isNaN(value) && value > 0) {
    //   this.userService.withdraw(value).subscribe({
    //     next: () => this.loadUser(),
    //     error: (err) => alert('Eroare la retragere: ' + err.message)
    //   });
    // } else {
    //   alert('Suma introdusă nu este validă.');
    // }
  }
}
