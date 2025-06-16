import { ChangeDetectorRef, Component, OnDestroy, OnInit, TrackByFunction } from '@angular/core';
import {Observable, Subject, takeUntil} from 'rxjs';
import {
  UserConversationDTO,
  UserDTO,
  MessageDTO,
  RequestDTO,
  ReplyDTO, FirestoreConversationItemDTO, ConversationItemDTO
} from '../../models/api.models';
import { AuthService } from '../../services/auth.service';
import { ExchangeService } from '../../services/exchange.service';
import { MockExchangeService } from '../../services/mock-exchange.service';
import { UserService } from '../../services/user.service';
import { SignalRService } from '../../services/signalr.service';
import { PaymentFlowService } from '../../services/payment-flow.service';
import {ConversationItemWrapper, MessagesStateService, SelectedUserInfo} from '../../services/messages_state.service';
import {
  ConversationUpdateEvent,
  NotificationEvent,
  SignalRHandlerService
} from '../../services/signalr_handler.service';
import {ConversationActionsService} from '../../services/conversation_actions.service';
import {FormsModule} from '@angular/forms';
import {AsyncPipe, NgForOf, NgIf, NgSwitch, NgSwitchCase, SlicePipe} from '@angular/common';
import {MessageBubbleComponent} from '../../shared/message-bubble/message-bubble.component';
import {RequestMessageComponent} from '../../shared/request-message/request-message.component';
import {ReplyMessageComponent} from '../../shared/reply-message/reply-message.component';
import {ServiceMessageComponent} from '../../shared/service-message/service-message.component';
import {ReplyFormComponent} from '../../shared/reply-form/reply-form.component';
import {RouterLink} from '@angular/router';

@Component({
  selector: 'app-messages',
  templateUrl: './messages.component.html',
  imports: [
    FormsModule,
    AsyncPipe,
    NgIf,
    MessageBubbleComponent,
    NgSwitchCase,
    RequestMessageComponent,
    ReplyMessageComponent,
    NgSwitch,
    ServiceMessageComponent,
    ReplyFormComponent,
    NgForOf,
    RouterLink,
    SlicePipe
  ],
  styleUrl: './messages.component.scss'
})
export class MessagesComponent implements OnInit, OnDestroy {
  private readonly destroy$ = new Subject<void>();
  private readonly USE_MOCK_DATA = false;

  // Pagination state
  private readonly conversationListPagination = { page: 1, pageSize: 20 };
  private readonly messagesPagination = { page: 1, pageSize: 50 };

  // UI state
  messageContent: string = '';
  userId: string | null = null;
  currentUserDetails: UserDTO | null = null;
  isSendingMessage: boolean = false;

  // Reply form state
  isReplyFormVisible: boolean = false;
  currentRequestId: string | null = null;
  chatOverlayVisible: boolean = false;
  replyFormData = {
    day: null as number | null,
    month: null as number | null,
    year: null as number | null,
    startHour: null as number | null,
    startMinute: null as number | null,
    endHour: null as number | null,
    endMinute: null as number | null,
    price: null as number | null
  };

  // Service confirmations (keep existing payment flow logic)
  isServiceConfirmationVisible: boolean = false;
  currentPaymentDetails: any;
  currentServiceTask: any;
  showPaymentFlow: boolean = false;

  exchanges$: Observable<UserConversationDTO[]> | undefined;
  conversationItems$: Observable<ConversationItemDTO[]> | undefined;
  selectedUser$: Observable<string | null> | undefined;
  selectedUserInfo$: Observable<SelectedUserInfo | null> | undefined;
  loading$: Observable<boolean> | undefined;

  // TrackBy functions for performance
  trackByConversation: TrackByFunction<UserConversationDTO> = (index, item) => item.userId;
  trackByConversationItem: TrackByFunction<ConversationItemWrapper> = (index, itemWrapper) => itemWrapper.item.id;

  constructor(
    private readonly exchangeService: ExchangeService,
    private readonly authService: AuthService,
    private readonly cdr: ChangeDetectorRef,
    private readonly mockExchangeService: MockExchangeService,
    private readonly userService: UserService,
    private readonly paymentFlowService: PaymentFlowService,
    protected readonly signalRService: SignalRService,
    private readonly signalRHandler: SignalRHandlerService,
    protected readonly messagesState: MessagesStateService,
    private readonly conversationActions: ConversationActionsService
  ) {}

  async ngOnInit() {
    // Public observables from state service
    this.exchanges$ = this.messagesState.exchanges$;
    this.conversationItems$ = this.messagesState.conversationItems$;
    this.selectedUser$ = this.messagesState.selectedUser$;
    this.selectedUserInfo$ = this.messagesState.selectedUserInfo$;
    this.loading$ = this.messagesState.loading$;
    this.userId = this.authService.getUserId();

    // Initialize all services
    await this.initializeServices();

    // Load initial data
    this.loadExchanges();
    this.loadCurrentUserDetails();
  }

  /**
   * Initialize all services and subscriptions
   */
  private async initializeServices(): Promise<void> {
    try {
      // Connect to SignalR
      await this.signalRService.connect(this.userId || undefined);

      // Setup SignalR handlers
      this.signalRHandler.setupListeners(this.userId || '');

      // Subscribe to notifications
      this.signalRHandler.notification$
        .pipe(takeUntil(this.destroy$))
        .subscribe(notification => this.showNotification(notification));

      // Subscribe to conversation updates
      this.signalRHandler.conversationUpdate$
        .pipe(takeUntil(this.destroy$))
        .subscribe(update => this.handleConversationUpdate(update));

      // Subscribe to action refresh events
      this.conversationActions.refresh$
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.refreshCurrentConversation();
          this.loadExchanges(false);
        });

      // Subscribe to payment completion (keep existing logic)
      this.subscribeToPaymentCompletion();

    } catch (error) {
      console.error('Failed to initialize services:', error);
    }
  }

  /**
   * Handle conversation update events from SignalR
   */
  private handleConversationUpdate(update: ConversationUpdateEvent): void {
    switch (update.type) {
      case 'refresh-current':
        this.refreshCurrentConversation();
        break;
      case 'refresh-list':
        this.loadExchanges(false);
        break;
      case 'refresh-both':
        this.refreshCurrentConversation();
        this.loadExchanges(false);
        break;
    }
  }

  /**
   * Show notification to user
   */
  private showNotification(notification: NotificationEvent): void {
    console.log(`${notification.type.toUpperCase()}: ${notification.message}`);
    // TODO: Implement your actual notification system here
    // Examples:
    // this.toastService.show(notification.message, notification.type);
    // this.snackBar.open(notification.message, 'Close', { duration: 4000 });
  }

  /**
   * Load exchanges/conversations
   */
  loadExchanges(loadMore = false): void {
    if (!loadMore) {
      this.conversationListPagination.page = 1;
      this.messagesState.setExchanges([], false);
    }

    this.messagesState.setLoading(true);

    const service = this.USE_MOCK_DATA ? this.mockExchangeService : this.exchangeService;

    service.getExchanges(this.conversationListPagination)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.response) {
            const pagedData = response.response;
            this.messagesState.setExchanges(pagedData.data || [], loadMore);
            this.messagesState.conversationListMeta = {
              totalCount: pagedData.totalCount,
              hasMore: this.messagesState.exchanges.length < pagedData.totalCount
            };

            if (!loadMore && Array.isArray(pagedData.data) && pagedData.data.length > 0) {
              this.selectConversation(pagedData.data[0]);
            }
          }
          this.messagesState.setLoading(false);
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Error loading conversations:', err);
          this.messagesState.setLoading(false);
          this.cdr.detectChanges();
        }
      });
  }

  /**
   * Select a conversation
   */
  selectConversation(conversation: UserConversationDTO): void {
    this.messagesState.setSelectedUserInfo({
      userId: conversation.userId,
      fullName: conversation.userFullName,
      profilePictureUrl: conversation.userProfilePictureUrl
    });
    this.loadConversationMessages(conversation.userId);
  }

  /**
   * Load conversation messages
   */
  loadConversationMessages(userId: string, loadMore = false): void {
    if (!loadMore) {
      this.messagesPagination.page = 1;
      this.messagesState.clearConversationItems();
    }

    const service = this.USE_MOCK_DATA ? this.mockExchangeService : this.exchangeService;

    this.messagesState.setSelectedUser(userId);
    this.messagesState.setLoading(true);

    service.getExchange(userId, this.messagesPagination)
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          if (response.response) {
            const pagedData = response.response;
            this.messagesState.setConversationItems(pagedData.data || [], loadMore);
            this.messagesState.messagesMeta = {
              totalCount: pagedData.totalCount,
              hasMore: this.messagesState.conversationItems.length < pagedData.totalCount
            };
          }
          this.messagesState.setLoading(false);
          this.cdr.detectChanges();
        },
        error: (err) => {
          console.error('Error loading conversation messages:', err);
          this.messagesState.setLoading(false);
          this.cdr.detectChanges();
        }
      });
  }

  /**
   * Load more exchanges
   */
  loadMoreExchanges(): void {
    if (this.messagesState.conversationListMeta.hasMore && !this.messagesState.isLoading) {
      this.conversationListPagination.page++;
      this.loadExchanges(true);
    }
  }

  /**
   * Load more messages
   */
  loadMoreMessages(): void {
    const selectedUser = this.messagesState.selectedUser;
    if (selectedUser && this.messagesState.messagesMeta.hasMore && !this.messagesState.isLoading) {
      this.messagesPagination.page++;
      this.loadConversationMessages(selectedUser, true);
    }
  }

  /**
   * Refresh current conversation
   */
  private refreshCurrentConversation(): void {
    const selectedUser = this.messagesState.selectedUser;
    if (selectedUser) {
      this.loadConversationMessages(selectedUser, false);
    }
  }

  /**
   * Send message
   */
  async sendMessage(content: string): Promise<void> {
    const selectedUser = this.messagesState.selectedUser;
    if (!content.trim() || !selectedUser || this.isSendingMessage || !this.currentUserDetails) return;

    this.isSendingMessage = true;
    this.cdr.detectChanges();

    const result = await this.conversationActions.sendMessage(
      content.trim(),
      selectedUser,
      this.currentUserDetails
    );

    this.isSendingMessage = false;

    if (result.success) {
      this.messageContent = '';
      this.refreshCurrentConversation();
      this.loadExchanges(false);
    } else {
      // Restore message content so user can try again
      this.messageContent = content.trim();
      this.showNotification({
        type: 'error',
        message: result.error || 'Failed to send message'
      });
    }

    this.cdr.detectChanges();
  }

  /**
   * Accept request
   */
  async acceptRequest(requestId: string): Promise<void> {
    const result = await this.conversationActions.acceptRequest(requestId);
    if (!result.success) {
      this.showNotification({
        type: 'error',
        message: result.error || 'Failed to accept request'
      });
    }
  }

  /**
   * Reject request
   */
  async rejectRequest(requestId: string): Promise<void> {
    const result = await this.conversationActions.rejectRequest(requestId);
    if (!result.success) {
      this.showNotification({
        type: 'error',
        message: result.error || 'Failed to reject request'
      });
    }
  }

  /**
   * Accept reply
   */
  async acceptReply(replyId: string): Promise<void> {
    const result = await this.conversationActions.acceptReply(replyId);
    if (!result.success) {
      this.showNotification({
        type: 'error',
        message: result.error || 'Failed to accept reply'
      });
    }
  }

  /**
   * Reject reply
   */
  async rejectReply(replyId: string): Promise<void> {
    const result = await this.conversationActions.rejectReply(replyId);
    if (!result.success) {
      this.showNotification({
        type: 'error',
        message: result.error || 'Failed to reject reply'
      });
    }
  }

  /**
   * Show reply form
   */
  showReplyForm(requestId: string): void {
    this.currentRequestId = requestId;
    this.isReplyFormVisible = true;
    this.chatOverlayVisible = true;

    // Reset form data
    this.replyFormData = {
      day: null,
      month: null,
      year: null,
      startHour: null,
      startMinute: null,
      endHour: null,
      endMinute: null,
      price: null
    };

    this.cdr.detectChanges();
  }

  /**
   * Hide reply form
   */
  hideReplyForm(): void {
    this.isReplyFormVisible = false;
    this.chatOverlayVisible = false;
    this.currentRequestId = null;
    this.cdr.detectChanges();
  }

  /**
   * Handle reply form submission
   */
  async onReplySubmit(replyData: any): Promise<void> {
    if (!this.currentRequestId) {
      console.error('No current request ID for reply');
      return;
    }

    const result = await this.conversationActions.submitReply(this.currentRequestId, replyData);

    if (result.success) {
      this.hideReplyForm();
    } else {
      this.showNotification({
        type: 'error',
        message: result.error || 'Failed to submit reply'
      });
    }
  }

  /**
   * Get typed conversation items for template
   */
  getTypedConversationItems(): any[] {
    return this.messagesState.getTypedConversationItems();
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
   * Load current user details
   */
  private loadCurrentUserDetails(): void {
    this.userService.getUserProfile().subscribe({
      next: (res) => {
        this.currentUserDetails = res.response || null;
      },
      error: (err) => {
        console.error('Error loading user details:', err);
      }
    });
  }

  // 🆕 Keep existing payment flow methods (simplified)
  private subscribeToPaymentCompletion(): void {
    this.paymentFlowService.paymentCompleted$
      .pipe(takeUntil(this.destroy$))
      .subscribe(paymentDetails => {
        if (paymentDetails) {
          this.handlePaymentCompletion(paymentDetails);
        }
      });
  }

  private handlePaymentCompletion(paymentDetails: any): void {
    console.log('Payment completed in messages:', paymentDetails);
    this.currentPaymentDetails = paymentDetails;
    this.createServiceTaskFromPayment(paymentDetails);
    this.showServiceConfirmation();
    this.loadExchanges(false);
  }

  private createServiceTaskFromPayment(paymentDetails: any): void {
    const paymentFlowState = this.paymentFlowService.getCurrentState();

    if (!paymentFlowState.serviceDetails || !paymentFlowState.specialistDetails || !paymentFlowState.userDetails) {
      console.error('Missing service details for task creation');
      return;
    }

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
      status: 'Confirmed',
      completedAt: new Date(),
      cancelledAt: new Date()
    };

    this.cdr.detectChanges();
  }

  private showServiceConfirmation(): void {
    this.isServiceConfirmationVisible = true;
    this.chatOverlayVisible = true;

    setTimeout(() => {
      this.hideServiceConfirmation();
    }, 10000);
  }

  hideServiceConfirmation(): void {
    this.isServiceConfirmationVisible = false;
    this.chatOverlayVisible = false;
  }

  onServiceTaskCompleted(event: { replyId: string; taskId: string }): void {
    console.log('Service task completed:', event);
    if (this.currentServiceTask) {
      this.currentServiceTask.status = 'Completed';
      this.currentServiceTask.completedAt = new Date();
    }
    this.cdr.detectChanges();
  }

  onServiceTaskCancelled(event: { replyId: string; taskId: string }): void {
    console.log('Service task cancelled:', event);
    if (this.currentServiceTask) {
      this.currentServiceTask.status = 'Cancelled';
      this.currentServiceTask.cancelledAt = new Date();
    }
    this.cdr.detectChanges();
  }

  openMediaPicker(): void {
    console.log('Open media picker');
    // this.isReplyFormVisible = true;
  }

  ngOnDestroy(): void {
    // Cleanup SignalR
    this.signalRHandler.cleanup();
    this.signalRService.disconnect();

    // Clear state
    this.messagesState.clearAll();

    // Complete subscriptions
    this.destroy$.next();
    this.destroy$.complete();
  }
}
