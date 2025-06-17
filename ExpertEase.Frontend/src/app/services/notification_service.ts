import { Injectable } from '@angular/core';
import { BehaviorSubject } from 'rxjs';
import {AppNotification} from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private notifications = new BehaviorSubject<AppNotification[]>([]);
  public notifications$ = this.notifications.asObservable();

  private readonly defaultDuration = 5000; // 5 seconds

  /**
   * Show a notification
   */
  show(message: string, type: AppNotification['type'] = 'info', duration?: number): string {
    const notification: AppNotification = {
      id: `notification_${Date.now()}_${Math.random()}`,
      type,
      message,
      timestamp: new Date(),
      duration: duration ?? this.defaultDuration
    };

    const current = this.notifications.value;
    this.notifications.next([...current, notification]);

    // Auto-dismiss after duration
    if (notification.duration && notification.duration > 0) {
      setTimeout(() => {
        this.dismiss(notification.id);
      }, notification.duration);
    }

    return notification.id;
  }

  /**
   * Dismiss a specific notification
   */
  dismiss(id: string): void {
    const current = this.notifications.value;
    const filtered = current.filter(n => n.id !== id);
    this.notifications.next(filtered);
  }

  /**
   * Clear all notifications
   */
  clear(): void {
    this.notifications.next([]);
  }

  /**
   * Convenience methods
   */
  success(message: string, duration?: number): string {
    return this.show(message, 'success', duration);
  }

  info(message: string, duration?: number): string {
    return this.show(message, 'info', duration);
  }

  warning(message: string, duration?: number): string {
    return this.show(message, 'warning', duration);
  }

  error(message: string, duration?: number): string {
    return this.show(message, 'error', duration);
  }
}
