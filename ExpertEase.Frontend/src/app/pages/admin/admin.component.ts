// admin.component.ts
import { Component, OnInit } from '@angular/core';
import {RouterLink, RouterOutlet} from '@angular/router';

@Component({
  selector: 'app-admin',
  templateUrl: './admin.component.html',
  imports: [
    RouterOutlet,
    RouterLink
  ],
  styleUrls: ['./admin.component.scss']
})
export class AdminComponent {
}
