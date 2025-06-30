import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {CurrencyPipe, DatePipe, NgClass, NgIf} from "@angular/common";
import {JobStatusEnum, ServiceTaskDTO, StatusEnum} from '../../models/api.models';
import {AuthService} from '../../services/auth.service';

@Component({
  selector: 'app-service-message',
  imports: [
    CurrencyPipe,
    DatePipe,
    NgClass,
    NgIf
  ],
  templateUrl: './service-message.component.html',
  styleUrl: './service-message.component.scss'
})
export class ServiceMessageComponent implements OnInit {
  @Input() serviceTask: ServiceTaskDTO = {
    id: '',
    replyId: '',
    userId: '',
    specialistId: '',
    specialistFullName: '',
    startDate: new Date(),
    endDate: new Date(),
    description: '',
    address: '',
    price: 0,
    status: JobStatusEnum.Confirmed,
    completedAt: new Date(),
    cancelledAt: new Date(),
  };

  userRole: string | null = '';
  userId: string | null = '';
  @Input() replyId: string = '';

  @Output() taskCompleted = new EventEmitter<{ replyId: string; taskId: string }>();
  @Output() taskCancelled = new EventEmitter<{ replyId: string; taskId: string }>();
  @Output() openReviewForm = new EventEmitter<void>(); // 🆕 Add review form event

  constructor(
    private readonly authService: AuthService,
  ) {}

  ngOnInit(): void {
    this.userRole = this.authService.getUserRole();
    this.userId = this.authService.getUserId();
    console.log('User role:', this.userRole);
    console.log('User ID:', this.userId);
  }

  completeTask() {
    this.taskCompleted.emit({ replyId: this.replyId, taskId: this.serviceTask.id });
  }

  cancelTask() {
    this.taskCancelled.emit({ replyId: this.replyId, taskId: this.serviceTask.id });
  }

  // 🆕 Open review form
  openReview() {
    console.log('📝 Opening review form for service task:', this.serviceTask.id);
    this.openReviewForm.emit();
  }

  // 🆕 Check if current user is the specialist
  isSpecialist(): boolean {
    return this.userId === this.serviceTask.specialistId;
  }

  // 🆕 Check if current user is the client
  isClient(): boolean {
    return this.userId === this.serviceTask.userId;
  }

  // 🆕 Get status message for display
  getStatusMessage(): string {
    switch (this.serviceTask.status) {
      case JobStatusEnum.Confirmed:
        if (this.isClient()) {
          return 'Asteaptă, specialistul va ajunge imediat!';
        } else if (this.isSpecialist()) {
          return 'Serviciul este în desfășurare';
        }
        return 'Serviciul este confirmat';

      case JobStatusEnum.Completed:
        return 'Serviciul a fost finalizat cu succes!';

      case JobStatusEnum.Cancelled:
        return 'Serviciul a fost anulat';

      case JobStatusEnum.Reviewed:
        return 'Serviciul a fost evaluat';

      default:
        return 'Status necunoscut';
    }
  }

  // 🆕 Get payment message for completed services
  getPaymentMessage(): string {
    if (this.serviceTask.status !== JobStatusEnum.Completed) return '';

    if (this.isSpecialist()) {
      return '💰 Plata a fost transferată în contul tău Stripe.';
    } else if (this.isClient()) {
      return '💳 Plata a fost procesată și transferată specialistului.';
    }
    return '';
  }

  // 🆕 Check if can show action buttons
  canShowActions(): boolean {
    return this.serviceTask.status === JobStatusEnum.Confirmed && this.isSpecialist();
  }

  // 🆕 Check if can show review button
  canShowReviewButton(): boolean {
    return this.serviceTask.status === JobStatusEnum.Completed;
  }

  protected readonly JobStatusEnum = JobStatusEnum;
}
