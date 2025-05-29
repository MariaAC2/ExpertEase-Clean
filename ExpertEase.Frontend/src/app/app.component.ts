import {Component, OnInit} from '@angular/core';
import {NavigationEnd, Router, RouterLink, RouterOutlet} from '@angular/router';
import {filter} from 'rxjs';
import {CommonModule} from '@angular/common';
import {AuthService} from './services/auth.service';

@Component({
  selector: 'app-root',
  standalone: true,
  imports: [CommonModule, RouterOutlet, RouterLink],
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent {
  title = 'ExpertEase.Frontend';
  navbarOpen = false;

  toggleNavbar(): void {
    this.navbarOpen = !this.navbarOpen;
  }

  constructor(public authService: AuthService, private router: Router) {
    this.router.events
      .pipe(filter(event => event instanceof NavigationEnd))
      .subscribe(event => {
        console.log('Navigated to:', event);
      });
  }
}
