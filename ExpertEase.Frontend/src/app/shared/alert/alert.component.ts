import {Component, EventEmitter, Input, Output} from '@angular/core';
import {NgClass, NgIf} from '@angular/common';

@Component({
  selector: 'app-alert',
  imports: [
    NgClass,
    NgIf
  ],
  templateUrl: './alert.component.html',
  styleUrl: './alert.component.scss'
})
export class AlertComponent {
  @Input() type: 'info' | 'success' | 'warning' | 'error' = 'info';
  @Input() title = '';
  @Input() message = '';
  @Input() showActions = false;
  @Input() confirmText = 'Da';
  @Input() cancelText = 'Nu';
  @Output() close = new EventEmitter<void>();
  @Output() confirm = new EventEmitter<void>();
  @Output() cancel = new EventEmitter<void>();

  onClose() {
    this.close.emit();
  }

  onConfirm() {
    this.confirm.emit();
  }

  onCancel() {
    this.cancel.emit();
  }
}
