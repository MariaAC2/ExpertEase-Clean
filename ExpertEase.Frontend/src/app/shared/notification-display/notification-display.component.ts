import {Component, OnDestroy, OnInit} from '@angular/core';
import {NotificationService} from '../../services/notification.service';
import {Subject, takeUntil} from 'rxjs';
import {AppNotification} from '../../models/api.models';
import {DatePipe, NgClass, NgForOf, NgSwitch, NgSwitchCase, NgSwitchDefault} from '@angular/common'; // Adjust the import path as necessary

@Component({
  selector: 'app-notification-display',
  imports: [
    DatePipe,
    NgSwitch,
    NgSwitchCase,
    NgSwitchDefault,
    NgClass,
    NgForOf
  ],
  templateUrl: './notification-display.component.html',
  styleUrl: './notification-display.component.scss'
})
export class NotificationDisplayComponent implements OnInit, OnDestroy {
  notifications: AppNotification[] = [];
  private readonly destroy$ = new Subject<void>();

  constructor(private readonly notificationService: NotificationService) {}

  ngOnInit() {
    console.log("First notification:", this.notifications[0].type);
    this.notificationService.notifications$
      .pipe(takeUntil(this.destroy$))
      .subscribe(notifications => {
        this.notifications = notifications;
      });
  }

  ngOnDestroy() {
    this.destroy$.next();
    this.destroy$.complete();
  }

  dismiss(id: string) {
    this.notificationService.dismiss(id);
  }

  trackByNotification(index: number, notification: AppNotification) {
    return notification.id;
  }
}
