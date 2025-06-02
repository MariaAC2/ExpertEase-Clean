import {Component, EventEmitter, Input, Output} from '@angular/core';
import {CurrencyPipe, DatePipe, NgClass, NgIf} from "@angular/common";
import {JobStatusEnum, ReplyDTO, ServiceTaskDTO, StatusEnum} from '../../models/api.models';

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
export class ServiceMessageComponent {
  @Input() serviceTask: ServiceTaskDTO = {
    id: '',
    replyId: '',
    userId: '',
    specialistId: '',
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
  @Input() replyId: string = '';

  @Output() taskCompleted = new EventEmitter<{ replyId: string; taskId: string }>();
  @Output() taskCancelled = new EventEmitter<{ replyId: string; taskId: string }>();

  completeTask() {
    this.taskCompleted.emit({ replyId: this.replyId, taskId: this.serviceTask.id });
  }

  cancelTask() {
    this.taskCancelled.emit({ replyId: this.replyId, taskId: this.serviceTask.id });
  }
}
