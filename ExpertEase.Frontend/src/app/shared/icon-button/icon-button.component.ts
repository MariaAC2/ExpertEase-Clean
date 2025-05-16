import { Component, Input, Output, EventEmitter } from '@angular/core';
import {CommonModule} from '@angular/common';

@Component({
  selector: 'app-icon-button',
  imports: [CommonModule],
  standalone: true,
  templateUrl: './icon-button.component.html',
  styleUrls: ['./icon-button.component.scss']
})
export class IconButtonComponent {
  @Input() label!: string;
  @Input() icon?: string;
  @Output() onClick = new EventEmitter<void>();
}
