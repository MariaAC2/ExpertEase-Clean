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
  @Input() replyId: string = '';

  @Output() taskCompleted = new EventEmitter<{ replyId: string; taskId: string }>();
  @Output() taskCancelled = new EventEmitter<{ replyId: string; taskId: string }>();

  constructor(
    private readonly authService: AuthService,
  ) {}

  ngOnInit(): void {
    this.userRole = this.authService.getUserRole();
    console.log(this.userRole);
  }

  completeTask() {
    this.taskCompleted.emit({ replyId: this.replyId, taskId: this.serviceTask.id });
  }

  cancelTask() {
    this.taskCancelled.emit({ replyId: this.replyId, taskId: this.serviceTask.id });
  }
}
