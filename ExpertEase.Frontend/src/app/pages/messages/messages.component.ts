import {ChangeDetectorRef, Component, OnDestroy, OnInit, TrackByFunction} from '@angular/core';
import { Timestamp } from 'firebase/firestore';
import {
  FirestoreConversationItemDTO, JobStatusEnum,
  MessageDTO, PaymentDetailsDTO,
  ReplyDTO,
  RequestDTO, ServiceTaskDTO,
  StatusEnum,
  UserConversationDTO, UserDTO,
} from '../../models/api.models';
import {AuthService} from '../../services/auth.service';
import {ExchangeService} from '../../services/exchange.service';
import {MessageService} from '../../services/message.service';
import {RequestService} from '../../services/request.service';
import {RequestMessageComponent} from '../../shared/request-message/request-message.component';
import {MessageBubbleComponent} from '../../shared/message-bubble/message-bubble.component';
import {
  AsyncPipe,
  NgClass,
  NgForOf,
  NgIf,
  NgSwitch,
  NgSwitchCase,
  SlicePipe
} from '@angular/common';
import {RouterLink} from '@angular/router';
import {FormsModule} from '@angular/forms';
import {BehaviorSubject, Subject, takeUntil, tap} from 'rxjs';
import {ReplyService} from '../../services/reply.service';
import {ReplyMessageComponent} from '../../shared/reply-message/reply-message.component';
import {MockExchangeService} from '../../services/mock-exchange.service';
import {UserService} from '../../services/user.service';
import {PaymentFlowService} from '../../services/payment-flow.service';
import {ServiceMessageComponent} from '../../shared/service-message/service-message.component';

// Enhanced type guards for runtime validation
interface TypeGuards {
  isMessage(data: any): data is MessageDTO;
  isRequest(data: any): data is RequestDTO;
  isReply(data: any): data is ReplyDTO;
}

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  imports: [
    RequestMessageComponent,
    MessageBubbleComponent,
    NgSwitch,
    NgForOf,
    RouterLink,
    NgIf,
    NgSwitchCase,
    FormsModule,
    AsyncPipe,
    SlicePipe,
    ReplyMessageComponent,
    ServiceMessageComponent,
  ],
  styleUrl: './messages.component.scss'
})
export class MessagesComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();

  // Pagination state
  private readonly conversationListPagination = { page: 1, pageSize: 20 };
  private readonly messagesPagination = { page: 1, pageSize: 50 };

  private readonly USE_MOCK_DATA = false;

  // Reactive state
  private readonly exchangesSubject = new BehaviorSubject<UserConversationDTO[]>([]);
  private readonly conversationItemsSubject = new BehaviorSubject<FirestoreConversationItemDTO[]>([]);
  private readonly selectedUserSubject = new BehaviorSubject<string | null>(null);
  private readonly loadingSubject = new BehaviorSubject<boolean>(false);
  private readonly selectedUserInfoSubject = new BehaviorSubject<{userId: string, fullName: string, profilePictureUrl?: string} | null>(null);

  // Public observables
  exchanges$ = this.exchangesSubject.asObservable();
  conversationItems$ = this.conversationItemsSubject.asObservable();
  selectedUser$ = this.selectedUserSubject.asObservable();
  selectedUserInfo$ = this.selectedUserInfoSubject.asObservable();
  loading$ = this.loadingSubject.asObservable();

  // Pagination metadata
  conversationListMeta: { totalCount: number; hasMore: boolean } = { totalCount: 0, hasMore: false };
  messagesMeta: { totalCount: number; hasMore: boolean } = { totalCount: 0, hasMore: false };

  // UI state
  messageContent: string = '';
  userId: string | null = null;

  // TrackBy functions for performance
  trackByConversation: TrackByFunction<UserConversationDTO> = (index, item) => item.userId;
  trackByConversationItem: TrackByFunction<{
    item: FirestoreConversationItemDTO;
    typed: MessageDTO | RequestDTO | ReplyDTO | null;
    type: 'message' | 'request' | 'reply' | 'unknown';
  }> = (index, itemWrapper) => itemWrapper.item.id;
  private currentUserDetails: UserDTO | undefined;
  isServiceConfirmationVisible: boolean = false;
  chatOverlayVisible: boolean = false;
  currentPaymentDetails: PaymentDetailsDTO | undefined;
  currentServiceTask: ServiceTaskDTO | undefined;
  showPaymentFlow: boolean = false;

  constructor(
    private readonly exchangeService: ExchangeService,
    private readonly messageService: MessageService,
    private readonly requestService: RequestService,
    private readonly replyService: ReplyService,
    private readonly authService: AuthService,
    private readonly cdr: ChangeDetectorRef,
    private readonly mockExchangeService: MockExchangeService,
    private readonly userService: UserService,
    private readonly paymentFlowService: PaymentFlowService,
  ) {}

  ngOnInit() {
    this.userId = this.authService.getUserId();
    this.loadExchanges();
    this.loadCurrentUserDetails();
    this.subscribeToPaymentCompletion();
  }

  loadExchanges(loadMore = false): void {
    if (!loadMore) {
      this.conversationListPagination.page = 1;
      this.exchangesSubject.next([]);
    }

    this.loadingSubject.next(true);

    const service = this.USE_MOCK_DATA ? this.mockExchangeService : this.exchangeService;

    service.getExchanges(this.conversationListPagination)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.response) {
            const pagedData = response.response;
            const currentExchanges = loadMore ? this.exchangesSubject.value : [];
            const newExchanges = [...currentExchanges, ...(pagedData.data || [])];

            this.exchangesSubject.next(newExchanges);
            this.conversationListMeta = {
              totalCount: pagedData.totalCount,
              hasMore: newExchanges.length < pagedData.totalCount
            };

            if (!loadMore && Array.isArray(pagedData.data) && pagedData.data.length > 0) {
              this.selectConversation(pagedData.data[0]);
            }
          }
          this.loadingSubject.next(false);
          this.cdr.markForCheck();
        },
        error: (err) => {
          console.error('Error loading conversations:', err);
          this.loadingSubject.next(false);
          this.cdr.markForCheck();
        }
      });
  }

  /**
   * Load more conversations (infinite scroll)
   */
  loadMoreExchanges(): void {
    if (this.conversationListMeta.hasMore && !this.loadingSubject.value) {
      this.conversationListPagination.page++;
      this.loadExchanges(true);
    }
  }

  /**
   * Select a conversation and load its messages
   */
  selectConversation(conversation: UserConversationDTO): void {
    this.selectedUserInfoSubject.next({
      userId: conversation.userId,
      fullName: conversation.userFullName,
      profilePictureUrl: conversation.userProfilePictureUrl
    });
    this.loadConversationMessages(conversation.userId);
  }

  /**
   * Load paginated conversation messages
   */
  loadConversationMessages(userId: string, loadMore = false): void {
    if (!loadMore) {
      this.messagesPagination.page = 1;
      this.conversationItemsSubject.next([]);
    }

    const service = this.USE_MOCK_DATA ? this.mockExchangeService : this.exchangeService;

    this.selectedUserSubject.next(userId);
    this.loadingSubject.next(true);

    service.getExchange(userId, this.messagesPagination)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.response) {
            const pagedData = response.response;
            const currentItems = loadMore ? this.conversationItemsSubject.value : [];

            const newItems = loadMore
              ? [...(pagedData.data || []), ...currentItems]
              : [...currentItems, ...(pagedData.data || [])];

            this.conversationItemsSubject.next(newItems);
            this.messagesMeta = {
              totalCount: pagedData.totalCount,
              hasMore: newItems.length < pagedData.totalCount
            };
          }
          this.loadingSubject.next(false);
          this.cdr.markForCheck();
        },
        error: (err) => {
          console.error('Error loading conversation messages:', err);
          this.loadingSubject.next(false);
          this.cdr.markForCheck();
        }
      });
  }

  /**
   * Load more messages (infinite scroll)
   */
  loadMoreMessages(): void {
    const selectedUser = this.selectedUserSubject.value;
    if (selectedUser && this.messagesMeta.hasMore && !this.loadingSubject.value) {
      this.messagesPagination.page++;
      this.loadConversationMessages(selectedUser, true);
    }
  }

  private loadCurrentUserDetails(): void {
    this.userService.getUserProfile().subscribe({
      next: (res) => {
        this.currentUserDetails = res.response;
      },
      error: (err) => {
        console.error('Error loading user details:', err);
      }
    });
  }

  private subscribeToPaymentFlow(): void {
    this.paymentFlowService.paymentFlow$
      .pipe(takeUntil(this.destroy$))
      .subscribe(state => {
        this.showPaymentFlow = state.isActive;
        this.cdr.markForCheck();
      });
  }

  // Update the existing subscribeToPaymentCompletion method
  private subscribeToPaymentCompletion(): void {
    this.paymentFlowService.paymentCompleted$
      .pipe(takeUntil(this.destroy$))
      .subscribe(paymentDetails => {
        if (paymentDetails) {
          this.handlePaymentCompletion(paymentDetails);
        }
      });
  }

  // Update the existing handlePaymentCompletion method
  private handlePaymentCompletion(paymentDetails: PaymentDetailsDTO): void {
    console.log('Payment completed in messages:', paymentDetails);

    // Store payment details for the service message
    this.currentPaymentDetails = paymentDetails;

    // Create service task from payment details and add to conversation
    + this.createServiceTaskFromPayment(paymentDetails);

    // Show overlay and freeze chat
    this.showServiceConfirmation();

    // Send email notification (you can implement this)
    this.sendPaymentCompletionEmail(paymentDetails);

    // Reload conversations to reflect new status
    this.loadExchanges(false);
  }

  private createServiceTaskFromPayment(paymentDetails: PaymentDetailsDTO): void {
    const paymentFlowState = this.paymentFlowService.getCurrentState();

    if (!paymentFlowState.serviceDetails || !paymentFlowState.specialistDetails || !paymentFlowState.userDetails) {
      console.error('Missing service details for task creation');
      return;
    }

    // Create ServiceTaskDTO from payment and flow state
    this.currentServiceTask = {
      id: paymentDetails.serviceTaskId,
      replyId: paymentFlowState.replyId || '',
      userId: paymentFlowState.userDetails.userId,
      specialistId: paymentFlowState.specialistDetails.userId,
      specialistFullName: paymentFlowState.specialistDetails.userFullName,
      startDate: paymentFlowState.serviceDetails.startDate,
      endDate: paymentFlowState.serviceDetails.endDate,
      description: paymentFlowState.serviceDetails.description,
      address: paymentFlowState.serviceDetails.address,
      price: paymentFlowState.serviceDetails.price,
      status: JobStatusEnum.Confirmed,
      completedAt: new Date(),
      cancelledAt: new Date()
    };

    this.cdr.markForCheck();
  }

  // Add email notification method
  private sendPaymentCompletionEmail(paymentDetails: PaymentDetailsDTO): void {
    // TODO: Implement email service call
    console.log('Sending payment completion email for:', paymentDetails);

    // Example implementation:
    // this.emailService.sendPaymentConfirmation(paymentDetails).subscribe({
    //   next: () => console.log('Email sent successfully'),
    //   error: (err) => console.error('Failed to send email:', err)
    // });
  }

  // Update the existing showServiceConfirmation method
  private showServiceConfirmation(): void {
    this.isServiceConfirmationVisible = true;
    this.chatOverlayVisible = true;

    // Auto-hide overlay after 5 seconds
    setTimeout(() => {
      this.hideServiceConfirmation();
      }, 10000);
  }

  // Update the existing hideServiceConfirmation method
  hideServiceConfirmation(): void {
    this.isServiceConfirmationVisible = false;
    this.chatOverlayVisible = false;
  }

  /**
   * Get typed conversation items for template
   */
  getTypedConversationItems(): Array<{
    item: FirestoreConversationItemDTO;
    typed: MessageDTO | RequestDTO | ReplyDTO | null;
    type: 'message' | 'request' | 'reply' | 'unknown';
  }> {
    const items = this.conversationItemsSubject.value;
    if (!items) return [];

    return items.map(item => {
      const typed = this.deserializeItem(item);
      const type = typed ? item.type as ('message' | 'request' | 'reply') : 'unknown';

      return { item, typed, type };
    });
  }

  /**
   * Deserialize conversation item
   */
  private deserializeItem(item: FirestoreConversationItemDTO): MessageDTO | RequestDTO | ReplyDTO | null {
    if (!item || !item.data || !item.type) {
      return null;
    }

    const data = item.data;
    const baseFields = {
      id: item.id,
      senderId: item.senderId || data['SenderId'] || data['senderId']
    };

    try {
      switch (item.type.toLowerCase()) {
        case 'message':
          return {
            ...baseFields,
            content: data['Content'] || data['content'] || '',
            createdAt: this.convertTimestamp(data['CreatedAt'] || data['createdAt'] || item.createdAt),
            isRead: Boolean(data['IsRead'] ?? data['isRead'] ?? false)
          } as MessageDTO;

        case 'request':
          return {
            ...baseFields,
            requestedStartDate: this.convertTimestamp(data['RequestedStartDate'] || data['requestedStartDate']),
            description: data['Description'] || data['description'] || '',
            status: data['Status'] || data['status'] || StatusEnum.Pending,
            senderContactInfo: data['SenderContactInfo'] || data['senderContactInfo']
          } as RequestDTO;

        case 'reply':
          return {
            ...baseFields,
            requestId: data['RequestId'] || data['requestId'] || '',
            startDate: this.convertTimestamp(data['StartDate'] || data['startDate']),
            endDate: this.convertTimestamp(data['EndDate'] || data['endDate']),
            price: Number(data['Price'] || data['price'] || 0),
            status: data['Status'] || data['status'] || StatusEnum.Pending,
            serviceTask: data['ServiceTask'] || data['serviceTask']
          } as ReplyDTO;

        default:
          return null;
      }
    } catch (error) {
      console.error('Error deserializing conversation item:', error, item);
      return null;
    }
  }

  /**
   * Safe timestamp conversion
   */
  private convertTimestamp(timestamp: any): Date {
    if (timestamp instanceof Date) return timestamp;
    if (timestamp && typeof timestamp.toDate === 'function') {
      return timestamp.toDate();
    }
    if (timestamp && typeof timestamp.seconds === 'number') {
      return new Date(timestamp.seconds * 1000);
    }
    if (typeof timestamp === 'string') {
      return new Date(timestamp);
    }
    return new Date();
  }

  /**
   * Type casting methods for template
   */
  asTypedMessage(itemWrapper: any): MessageDTO {
    return itemWrapper.typed as MessageDTO;
  }

  asTypedRequest(itemWrapper: any): RequestDTO {
    return itemWrapper.typed as RequestDTO;
  }

  asTypedReply(itemWrapper: any): ReplyDTO {
    return itemWrapper.typed as ReplyDTO;
  }

  /**
   * Send message with optimistic update
   */
  sendMessage(content: string): void {
    const selectedUser = this.selectedUserSubject.value;
    if (!content.trim() || !selectedUser) return;

    // Optimistic update
    const tempMessage = {
      id: `temp_${Date.now()}`,
      conversationId: 'temp',
      senderId: this.userId!,
      type: 'message' as const,
      createdAt: Timestamp.fromDate(new Date()),
      data: {
        Content: content.trim(),
        SenderId: this.userId!,
        CreatedAt: new Date(),
        IsRead: false
      }
    };

    const currentItems = this.conversationItemsSubject.value;
    this.conversationItemsSubject.next([...currentItems, tempMessage]);
    this.messageContent = '';
    this.cdr.markForCheck();

    // Send actual message
    this.messageService.sendMessage(selectedUser, { content: content.trim() })
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: () => {
          this.loadConversationMessages(selectedUser);
        },
        error: (err) => {
          console.error('Error sending message:', err);
          this.conversationItemsSubject.next(currentItems);
          this.cdr.markForCheck();
        }
      });
  }

  /**
   * Accept request - called by your request-message component
   */
  acceptRequest(requestId: string): void {
    this.requestService.acceptRequest(requestId)
      .pipe(
        tap(() => {
          const selectedUser = this.selectedUserSubject.value;
          if (selectedUser) {
            this.loadConversationMessages(selectedUser);
          }
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: () => console.log('Request accepted'),
        error: (err) => console.error('Error accepting request:', err)
      });
  }

  /**
   * Reject request - called by your request-message component
   */
  rejectRequest(requestId: string): void {
    this.requestService.rejectRequest(requestId)
      .pipe(
        tap(() => {
          const selectedUser = this.selectedUserSubject.value;
          if (selectedUser) {
            this.loadConversationMessages(selectedUser);
          }
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: () => console.log('Request rejected'),
        error: (err) => console.error('Error rejecting request:', err)
      });
  }

  /**
   * Accept reply - called by your reply-message component
   */
  acceptReply(replyId: string): void {
    this.replyService.acceptReply(replyId)
      .pipe(
        tap(() => {
          const selectedUser = this.selectedUserSubject.value;
          if (selectedUser) {
            this.loadConversationMessages(selectedUser);
          }
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: () => console.log('Reply accepted'),
        error: (err) => console.error('Error accepting reply:', err)
      });
  }

  /**
   * Reject reply - called by your reply-message component
   */
  rejectReply(replyId: string): void {
    this.replyService.rejectReply(replyId)
      .pipe(
        tap(() => {
          const selectedUser = this.selectedUserSubject.value;
          if (selectedUser) {
            this.loadConversationMessages(selectedUser);
          }
        }),
        takeUntil(this.destroy$)
      )
      .subscribe({
        next: () => console.log('Reply rejected'),
        error: (err) => console.error('Error rejecting reply:', err)
      });
  }

  /**
   * Open media picker - your existing method
   */
  openMediaPicker(): void {
    console.log('Open media picker');
    // Your existing implementation
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  onServiceTaskCompleted(event: { replyId: string; taskId: string }): void {
    console.log('Service task completed:', event);

    // Update the current service task status
    if (this.currentServiceTask) {
      this.currentServiceTask.status = JobStatusEnum.Completed;
      this.currentServiceTask.completedAt = new Date();
    }

    // TODO: Call backend service to mark task as completed
    // this.serviceTaskService.completeTask(event.taskId).subscribe({
    //   next: () => {
    //     console.log('Task marked as completed on backend');
    //     // Send completion email
    //     // this.emailService.sendServiceCompletionNotification(event.taskId);
    //   },
    //   error: (err) => console.error('Error completing task:', err)
    // });

    this.cdr.markForCheck();
  }

  onServiceTaskCancelled(event: { replyId: string; taskId: string }): void {
    console.log('Service task cancelled:', event);

    // Update the current service task status
    if (this.currentServiceTask) {
      this.currentServiceTask.status = JobStatusEnum.Cancelled;
      this.currentServiceTask.cancelledAt = new Date();
    }

    // TODO: Call backend service to cancel task
    // this.serviceTaskService.cancelTask(event.taskId).subscribe({
    //   next: () => {
    //     console.log('Task cancelled on backend');
    //     // Send cancellation email
    //     // this.emailService.sendServiceCancellationNotification(event.taskId);
    //   },
    //   error: (err) => console.error('Error cancelling task:', err)
    // });

    this.cdr.markForCheck();
  }
}
