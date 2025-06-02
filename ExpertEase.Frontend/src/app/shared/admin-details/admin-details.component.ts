import { Component, EventEmitter, Input, Output } from '@angular/core';
import {CommonModule, DatePipe} from "@angular/common";
import {UserDTO, UserRoleEnum} from '../../models/api.models';

@Component({
  selector: 'app-admin-details',
    imports: [
      CommonModule
    ],
  templateUrl: './admin-details.component.html',
  styleUrl: './admin-details.component.scss'
})
export class AdminDetailsComponent {
  @Input() entity: Record<string, any> = {};
  @Input() title: string = '';
  @Output() close = new EventEmitter<void>(); // event to close the panel

  onClose(): void {
    this.close.emit();
  }

  objectKeys = Object.keys;

  formatKey(key: string): string {
    if (key.includes(' ') || key.includes('-')) {
      return key;
    }

    return key
      .replace(/([A-Z])/g, ' $1')
      .replace(/^./, str => str.toUpperCase());
  }

  formatValue(value: any): string {
    if (typeof value === 'boolean') return value ? 'Da' : 'Nu';
    if (value instanceof Date) return value.toLocaleDateString();
    if (typeof value === 'string' && value.toLowerCase().includes('password')) return '********';
    return value;
  }
}
