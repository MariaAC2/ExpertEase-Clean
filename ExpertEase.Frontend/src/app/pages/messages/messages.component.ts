import {Component, OnInit} from '@angular/core';
import {
  ConversationDTO,
  JobStatusEnum, MessageAddDTO,
  ReplyAddDTO,
  ReplyDTO,
  RequestDTO,
  ReviewAddDTO,
  ServiceTaskDTO,
  StatusEnum,
  UserConversationDTO,
  UserRoleEnum
} from '../../models/api.models';
import {CommonModule} from '@angular/common';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {AuthService} from '../../services/auth.service';
import {ExchangeService} from '../../services/exchange.service';
import {RouterLink} from '@angular/router';
import {ReplyService} from '../../services/reply.service';
import {RequestMessageComponent} from '../../shared/request-message/request-message.component';
import {ReplyFormComponent} from '../../shared/reply-form/reply-form.component';
import {ReplyMessageComponent} from '../../shared/reply-message/reply-message.component';
import {ServiceMessageComponent} from '../../shared/service-message/service-message.component';
import {TaskService} from '../../services/task.service';
import {ReviewService} from '../../services/review.service';
import {ReviewFormComponent} from '../../shared/review-form/review-form.component';
import {RequestService} from '../../services/request.service';
import {MessageBubbleComponent} from '../../shared/message-bubble/message-bubble.component';
import {MessageService} from '../../services/message.service';

@Component({
  selector: 'app-messages',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, ReactiveFormsModule, RequestMessageComponent, ReplyFormComponent, ReplyMessageComponent, ServiceMessageComponent, ReviewFormComponent, RouterLink, MessageBubbleComponent],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.scss'
})
export class MessagesComponent implements OnInit {
  messageContent: string = '';
  request: RequestDTO | undefined;
  userRole: string | null | undefined;
  userId: string | null | undefined;
  isReplyFormVisible: boolean = false;
  isReviewFormVisible: boolean = false;
  replyForm: {
    requestId: string;
    startDate: Date;
    endDate: Date;
    price: number;
  } = {
    requestId: '',
    startDate: new Date(),
    endDate: new Date(),
    price: 0
  }

  reviewForm: {
    receiverUserId: string;
    rating: number;
    content: string;
  } = {
    receiverUserId: '',
    rating: 5,
    content: '',
  }
  selectedRequestId: string | undefined | null;
  exchanges: ConversationDTO[] = [];
  selectedExchange: UserConversationDTO | undefined | null;
  selectedTaskId: string | undefined;

  constructor(private readonly authService: AuthService,
              private readonly requestService: RequestService,
              private readonly replyService: ReplyService,
              private readonly exchangeService: ExchangeService,
              private readonly messageService: MessageService,
              private readonly taskService: TaskService,
              private readonly reviewService: ReviewService) {}

  ngOnInit() {
    this.userRole = this.authService.getUserRole();
    this.userId = this.authService.getUserId();
    // this.userId = 'user-1234';
    // this.exchanges = [this.dummyExchange];
    // this.selectedExchange = this.dummyExchange;

    this.exchangeService.getExchanges().subscribe({
      next: (res) => {
        this.exchanges = res.response ?? [];
        console.log('Convorbiri încărcate:', this.exchanges);

        if (this.exchanges.length > 0) {
          const lastUser = this.exchanges[this.exchanges.length - 1];
          this.loadExchange(lastUser.id);
        }
      },
      error: (err) => {
        console.error('Eroare la încărcarea convorbirilor:', err);
      }
    });
  }

  loadExchange(senderUserId: string | undefined): void {
    console.log('Loading exchange for user:', senderUserId);
    this.exchangeService.getExchange(senderUserId).subscribe({
      next: (res) => {
        this.selectedExchange = res.response;
      },
      error: (err) => {
        console.error('Failed to reload exchange', err);
      }
    });
  }

  sendMessage(content: string) {
    console.log("Sending message:", content);
    if (!this.selectedExchange) {
      console.error('No exchange selected');
      return;
    }

    const message: MessageAddDTO = {
      receiverId: this.selectedExchange.id,
      content: content
    };

    this.messageService.sendMessage(message).subscribe({
      next: (res) => {
        console.log('Mesaj trimis cu succes:', res);
        this.loadExchange(this.selectedExchange?.id);
        this.messageContent = '';
      },
      error: (err) => {
        console.error('Eroare la trimiterea mesajului:', err);
      }
    });
  }

  acceptRequest(requestId: string) {
    this.requestService.acceptRequest(requestId).subscribe({
      next: () => {
        console.log('Cererea a fost acceptată');
        const request = this.selectedExchange?.requests.find(r => r.id === requestId);
        if (request) {
          this.loadExchange(this.selectedExchange?.id); // Refresh exchange
        }
      },
      error: (err) => {
        console.error('Eroare la acceptare:', err);
      }
    });
  }

  rejectRequest(requestId: string) {
    this.requestService.rejectRequest(requestId).subscribe({
      next: () => {
        console.log('Cererea a fost respinsă');
        const request = this.selectedExchange?.requests.find(r => r.id === requestId);
        if (request) {
          this.loadExchange(this.selectedExchange?.id); // Refresh exchange
        }
      },
      error: (err) => {
        console.error('Eroare la respingere:', err);
      }
    });
  }

  openReplyForm(requestId: string) {
    this.selectedRequestId = requestId;
    this.isReplyFormVisible = true;
  }

  closeReplyForm() {
    this.isReplyFormVisible = false;
  }

  submitReply(replyData: { [key: string]: any }) {
    if (!this.selectedRequestId) return;

    if (!this.selectedRequestId) {
      console.error('No requestId found to submit reply.');
      return;
    }

    const replyDataToSend : ReplyAddDTO = replyData as ReplyAddDTO;
    this.replyService.addReply(this.selectedRequestId, replyDataToSend).subscribe({
      next: () => {
        console.log('Reply submitted successfully!');
        this.closeReplyForm();
        // Refresh messages if needed
      },
      error: (err: any) => {
        console.error('Eroare la trimiterea ofertei:', err);
      }
    });
  }

  acceptReply(requestId: string, replyId: string): void {
    this.replyService.acceptReply(replyId).subscribe({
      next: () => {
        console.log('Reply accepted');
        const request = this.selectedExchange?.requests.find(r => r.id === requestId);
        if (request) {
          const currentUserId = this.authService.getUserId();
          const otherUserId = request.senderUserId === currentUserId
            ? request.receiverUserId
            : request.senderUserId;

          if (otherUserId) {
            this.loadExchange(this.selectedExchange?.id);
          }
        }
      },
      error: (err) => console.error('Error accepting reply:', err)
    });
  }

  rejectReply(requestId: string, replyId: string): void {
    this.replyService.rejectReply(replyId).subscribe({
      next: () => {
        console.log('Reply rejected');
        const request = this.selectedExchange?.requests.find(r => r.id === requestId);
        if (request) {
          const currentUserId = this.authService.getUserId();
          const otherUserId = request.senderUserId === currentUserId
            ? request.receiverUserId
            : request.senderUserId;

          if (otherUserId) {
            this.loadExchange(this.selectedExchange?.id);
          }
        }
      },
      error: (err) => {
        console.error('Error rejecting reply:', err);
      }
    });
  }

  hasActiveServiceTask(request: RequestDTO): boolean {
    const replies = request.replies ?? [];
    const lastReply = replies[replies.length - 1];
    return !!lastReply?.serviceTask && lastReply.serviceTask.status !== 'Completed';
  }

  getLastReplyWithServiceTask(request: RequestDTO): ReplyDTO | undefined {
    const replies = request.replies ?? [];
    const lastReply = replies[replies.length - 1];
    return lastReply?.serviceTask ? lastReply : undefined;
  }

  completeTask(replyId: string, taskId: string) {
    this.taskService.completeServiceTask(taskId).subscribe({
      next: () => {
        console.log('Task marked as completed');
        this.selectedTaskId = taskId;

        // Try to find the task locally
        const request = this.selectedExchange?.requests.find(req =>
          req.replies?.some(r => r.id === replyId)
        );

        const reply = request?.replies?.find(r => r.id === replyId);
        const task = reply?.serviceTask;

        if (task) {
          this.showReviewForm(task);
        } else {
          console.warn("Task not found in local data");
        }
      },
      error: (err) => {
        console.error('Error completing task:', err);
      }
    });
  }

  showReviewForm(task: ServiceTaskDTO): void {
    const currentUserId = this.authService.getUserId();

    const isUserInvolved = currentUserId === task.userId || currentUserId === task.specialistId;
    const isCompleted = task.status === JobStatusEnum.Completed;

    if (isUserInvolved && isCompleted) {
      // Optional: also check if a review has already been left by this user for this task
      this.prepareReviewForm(task);
    }
  }


  submitReview(replyData: { [key: string]: any }) {
    console.log(this.selectedTaskId);
    if (!this.selectedTaskId) {
      console.error('No requestId found to submit reply.');
      return;
    }

    console.log(replyData);

    const replyDataToSend : ReviewAddDTO = replyData as ReviewAddDTO;
    this.reviewService.addReview(this.selectedTaskId, replyDataToSend).subscribe({
      next: () => {
        console.log('Reply submitted successfully!');
        this.closeReplyForm();
        // Refresh messages if needed
      },
      error: (err) => {
        console.error('Eroare la trimiterea ofertei:', err);
      }
    });
  }

  closeReviewForm(): void {
    this.isReviewFormVisible = false;
    this.reviewForm = {
      receiverUserId: '',
      rating: 5,
      content: '',
    }
  }

  prepareReviewForm(task: ServiceTaskDTO): void {
    const currentUserId = this.authService.getUserId();

    const recipientId = currentUserId === task.userId
      ? task.specialistId
      : task.userId;

    this.reviewForm = {
      receiverUserId: recipientId,
      rating: 0,
      content: '',
    };

    this.isReviewFormVisible = true;
  }

  cancelTask(replyId: string, taskId: string) {
    this.taskService.cancelServiceTask(taskId).subscribe({
      next: () => {
        console.log('Cererea a fost acceptată');
      },
      error: (err) => {
        console.error('Eroare la acceptare:', err);
      }
    });
  }
}
