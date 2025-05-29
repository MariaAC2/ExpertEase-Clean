import {Component, OnInit} from '@angular/core';
import {
  ReplyAddDTO, ReplyDTO,
  RequestAddDTO,
  RequestDTO, ReviewAddDTO, ServiceTaskDTO,
  StatusEnum,
  UserExchangeDTO,
  UserRoleEnum
} from '../../models/api.models';
import {CommonModule} from '@angular/common';
import {FormsModule, ReactiveFormsModule} from '@angular/forms';
import {AuthService} from '../../services/auth.service';
import {MessagesService} from '../../services/messages.service';
import {ActivatedRoute, Router} from '@angular/router';
import {SpecialistRequestService} from '../../services/specialist.request.service';
import {SpecialistReplyService} from '../../services/specialist.reply.service';
import {UserReplyService} from '../../services/user.reply.service';
import {RequestMessageComponent} from '../../shared/request-message/request-message.component';
import {ReplyFormComponent} from '../../shared/reply-form/reply-form.component';
import {ReplyMessageComponent} from '../../shared/reply-message/reply-message.component';
import {ServiceMessageComponent} from '../../shared/service-message/service-message.component';
import {TaskService} from '../../services/task.service';
import {ReviewService} from '../../services/review.service';
import {ReviewFormComponent} from '../../shared/review-form/review-form.component';

@Component({
  selector: 'app-messages',
  imports: [CommonModule, FormsModule, ReactiveFormsModule, ReactiveFormsModule, RequestMessageComponent, ReplyFormComponent, ReplyMessageComponent, ServiceMessageComponent, ReviewFormComponent],
  templateUrl: './messages.component.html',
  styleUrl: './messages.component.scss'
})
export class MessagesComponent implements OnInit {
  exchange: UserExchangeDTO | undefined;
  request: RequestDTO | undefined;
  userRole: string | null | undefined;
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

  exchanges: UserExchangeDTO[] = [];
  selectedExchange: UserExchangeDTO | undefined | null;

  selectedTask: ServiceTaskDTO | undefined;
  selectedTaskId: string | undefined;

  // ngOnInit() {
  //   this.userRole = this.authService.getUserRole();
  //   const userId = this.authService.getUserId();
  //
  //   this.messagesService.getExchanges('', 1, 100).subscribe({
  //     next: (res) => {
  //       this.exchanges = res.response?.data ?? [];
  //
  //       if (this.exchanges.length > 0) {
  //         // Select the last conversation
  //         this.selectedExchange = this.exchanges[this.exchanges.length - 1];
  //       }
  //     },
  //     error: (err) => {
  //       console.error('Eroare la încărcarea convorbirilor:', err);
  //     }
  //   });
  // }
  //

  constructor(private authService: AuthService,
              private specialistRequestService: SpecialistRequestService,
              private specialistReplyService: SpecialistReplyService,
              private userReplyService: UserReplyService,
              private messageService: MessagesService,
              private taskService: TaskService,
              private reviewService: ReviewService,
              private route: ActivatedRoute) {}

  // ngOnInit() {
  //   const receiverUserId = this.route.snapshot.paramMap.get('id');
  //   this.userRole = this.authService.getUserRole();
  //   console.log(this.userRole);
  //   if (receiverUserId) {
  //     this.messageService.getExchange(receiverUserId).subscribe({
  //       next: (res) => {
  //         this.exchange = res.response;
  //       },
  //       error: (err) => {
  //         console.error(err.message);
  //         console.error('Eroare la încărcarea conversației:', err);
  //       }
  //     });
  //   } else {
  //     console.error('Nu s-a putut obține ID-ul utilizatorului din token.');
  //   }
  // }

  ngOnInit() {
    this.userRole = this.authService.getUserRole();

    this.messageService.getExchanges('', 1, 100).subscribe({
      next: (res) => {
        this.exchanges = res.response?.data ?? [];
        console.log(this.exchanges);

        if (this.exchanges.length > 0) {
          // Select the last conversation
          this.selectedExchange = this.exchanges[this.exchanges.length - 1];
        }
      },
      error: (err) => {
        console.error('Eroare la încărcarea convorbirilor:', err);
      }
    });
  }

  loadExchange(senderUserId: string): void {
    this.messageService.getExchange(senderUserId).subscribe({
      next: (res) => {
        this.selectedExchange = res.response;
      },
      error: (err) => {
        console.error('Failed to reload exchange', err);
      }
    });
  }

  acceptRequest(requestId: string) {
    this.specialistRequestService.acceptRequest(requestId).subscribe({
      next: () => {
        console.log('Cererea a fost acceptată');
      },
      error: (err) => {
        console.error('Eroare la acceptare:', err);
      }
    });
  }

  rejectRequest(requestId: string) {
    this.specialistRequestService.rejectRequest(requestId).subscribe({
      next: () => {
        console.log('Cererea a fost acceptată');
      },
      error: (err) => {
        console.error('Eroare la acceptare:', err);
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
    this.specialistReplyService.addReply(this.selectedRequestId, replyDataToSend).subscribe({
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

  selectExchange(exchange: UserExchangeDTO) {
    this.selectedExchange = exchange;
    this.isReplyFormVisible = false;
    this.selectedRequestId = null;
  }

  // acceptReply(requestId: string, replyId: string): void {
  //   this.userReplyService.acceptReply(requestId, replyId).subscribe({
  //     next: () => {
  //       // Refresh the thread or update the reply status locally
  //       console.log('Reply accepted');
  //       // this.refreshExchange(); // optional: reload data
  //     },
  //     error: (err) => console.error('Error accepting reply:', err)
  //   });
  // }

  // acceptReply(requestId: string, replyId: string): void {
  //   this.userReplyService.acceptReply(requestId, replyId).subscribe(() => {
  //     const request = this.selectedExchange?.requests.find(r => r.id === requestId);
  //     if (request) {
  //       this.loadExchange(request.senderUserId); // This is your senderUserId
  //     } else {
  //       console.warn('Request not found in selectedExchange');
  //     }
  //   });
  // }

  acceptReply(requestId: string, replyId: string): void {
    this.userReplyService.acceptReply(requestId, replyId).subscribe(() => {
      const request = this.selectedExchange?.requests.find(r => r.id === requestId);

      const currentUserId = this.authService.getUserId();
      const otherUserId =
        request?.senderUserId === currentUserId
          ? request.receiverUserId
          : request?.senderUserId;

      if (otherUserId) {
        this.loadExchange(otherUserId);
      } else {
        console.warn('Could not determine other user in conversation.');
      }
    });
  }

  rejectReply(requestId: string, replyId: string): void {
    this.userReplyService.rejectReply(requestId, replyId).subscribe({
      next: () => {
        console.log('Reply rejected');
        // this.refreshExchange(); // optional
      },
      error: (err) => console.error('Error rejecting reply:', err)
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

  // completeTask(replyId: string, taskId: string) {
  //   this.taskService.completeServiceTask(replyId, taskId).subscribe({
  //     next: () => {
  //       console.log('Cererea a fost acceptată');
  //     },
  //     error: (err) => {
  //       console.error('Eroare la acceptare:', err);
  //     }
  //   });
  // }

  completeTask(replyId: string, taskId: string) {
    this.taskService.completeServiceTask(replyId, taskId).subscribe({
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
          this.prepareReviewForm(task);
        } else {
          console.warn("Task not found in local data");
        }
      },
      error: (err) => {
        console.error('Error completing task:', err);
      }
    });
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
    const currentUserRole = this.authService.getUserRole();

    // Determine who the recipient of the review is
    const recipientId = currentUserRole === UserRoleEnum.Client
      ? task.userId
      : task.specialistId;

    this.reviewForm = {
      receiverUserId: recipientId,
      rating: 0, // default value before user input
      content: '',
    };

    this.isReviewFormVisible = true;
  }
  cancelTask(replyId: string, taskId: string) {
    this.taskService.cancelServiceTask(replyId, taskId).subscribe({
      next: () => {
        console.log('Cererea a fost acceptată');
      },
      error: (err) => {
        console.error('Eroare la acceptare:', err);
      }
    });
  }
}
