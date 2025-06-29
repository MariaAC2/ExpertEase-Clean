import { ChangeDetectorRef, Component, OnDestroy, OnInit, TrackByFunction } from '@angular/core';
import { firstValueFrom, Observable, Subject, takeUntil } from 'rxjs';
import {
  UserConversationDTO,
  MessageDTO,
  RequestDTO,
  ReplyDTO,
  ConversationItemDTO,
  UserProfileDTO,
  PhotoDTO
} from '../../models/api.models';
import { AuthService } from '../../services/auth.service';
import { ExchangeService } from '../../services/exchange.service';
import { MockExchangeService } from '../../services/mock-exchange.service';
import { UserService } from '../../services/user.service';
import { SignalRService } from '../../services/signalr.service';
import { PaymentFlowService } from '../../services/payment-flow.service';
import { ConversationItemWrapper, MessagesStateService, SelectedUserInfo } from '../../services/messages_state.service';
import {
  ConversationUpdateEvent,
  SignalRHandlerService
} from '../../services/signalr_handler.service';
import { ConversationActionsService } from '../../services/conversation_actions.service';
import { FormsModule } from '@angular/forms';
import { AsyncPipe, NgClass, NgForOf, NgIf, NgSwitch, NgSwitchCase, SlicePipe } from '@angular/common';
import { MessageBubbleComponent } from '../../shared/message-bubble/message-bubble.component';
import { RequestMessageComponent } from '../../shared/request-message/request-message.component';
import { ReplyMessageComponent } from '../../shared/reply-message/reply-message.component';
import { ServiceMessageComponent } from '../../shared/service-message/service-message.component';
import { ReplyFormComponent } from '../../shared/reply-form/reply-form.component';
import { RouterLink } from '@angular/router';
import { ServicePaymentComponent } from '../../shared/service-payment/service-payment.component';
import { NotificationService } from '../../services/notification.service';
import { NotificationDisplayComponent } from '../../shared/notification-display/notification-display.component';
import { TaskService } from '../../services/task.service';
import { ReplyService } from '../../services/reply.service';
import { PhotoBubbleComponent } from '../../shared/photo-bubble/photo-bubble.component';
import { PhotoService } from '../../services/photo.service';
import { PaymentService } from '../../services/payment.service';

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
    SlicePipe,
    ServicePaymentComponent,
    NgClass,
    NotificationDisplayComponent,
    PhotoBubbleComponent
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
  currentUserDetails: UserProfileDTO | null = null;
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

  // Service confirmation state
  isServiceConfirmationVisible: boolean = false;
  currentPaymentDetails: any;
  currentServiceTask: any;
  showPaymentFlow: boolean = false;

  // Photo upload state
  isUploadingPhoto: boolean = false;
  selectedPhotoFile: File | null = null;
  photoCaption: string = '';

  // Observables
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
    private readonly userService: UserService,
    private readonly cdr: ChangeDetectorRef,
    private readonly mockExchangeService: MockExchangeService,
    private readonly paymentFlowService: PaymentFlowService,
    protected readonly signalRService: SignalRService,
    private readonly signalRHandler: SignalRHandlerService,
    protected readonly messagesState: MessagesStateService,
    private readonly conversationActions: ConversationActionsService,
    private readonly notificationService: NotificationService,
    private readonly taskService: TaskService,
    private readonly photoService: PhotoService,
    private readonly replyService: ReplyService,
    private readonly paymentService: PaymentService,
  ) {}

  async ngOnInit() {
    // Initialize observables
    this.exchanges$ = this.messagesState.exchanges$;
    this.conversationItems$ = this.messagesState.conversationItems$;
    this.selectedUser$ = this.messagesState.selectedUser$;
    this.selectedUserInfo$ = this.messagesState.selectedUserInfo$;
    this.loading$ = this.messagesState.loading$;
    this.userId = this.authService.getUserId();

    // Initialize services and load data
    await this.initializeServices();
    this.loadExchanges();
    this.loadCurrentUserDetails();

    // ✅ FAST: Load service task using new API
    setTimeout(() => {
      this.loadServiceTaskForCurrentConversation();
    }, 2000);
  }

  ngOnDestroy(): void {
    this.signalRHandler.cleanup();
    this.signalRService.disconnect();
    this.messagesState.clearAll();
    this.destroy$.next();
    this.destroy$.complete();
  }

  // Helper methods for typed conversation items
  getTypedConversationItems(): any[] {
    return this.messagesState.getTypedConversationItems();
  }

  asTypedMessage(itemWrapper: any): MessageDTO {
    return itemWrapper.typed as MessageDTO;
  }

  asTypedRequest(itemWrapper: any): RequestDTO {
    return itemWrapper.typed as RequestDTO;
  }

  asTypedPhoto(itemWrapper: any): PhotoDTO {
    return itemWrapper.typed as PhotoDTO;
  }

  asTypedReply(itemWrapper: any): ReplyDTO {
    return itemWrapper.typed as ReplyDTO;
  }

  // Public methods for manual testing/refreshing
  public async refreshServiceTask(): Promise<void> {
    console.log('🔄 Manually refreshing service task...');
    await this.loadCurrentServiceTaskFast();
  }

  public async findMyServiceTask(): Promise<void> {
    await this.loadCurrentServiceTaskFast();
  }

  // Add these methods to the MessagesComponent class

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
        .subscribe(notification => this.notificationService.showNotification(notification));

      // Subscribe to conversation updates
      this.signalRHandler.conversationUpdate$
        .pipe(takeUntil(this.destroy$))
        .subscribe(update => this.handleConversationUpdate(update));

      // Subscribe to payment flow state changes
      this.paymentFlowService.paymentFlow$
        .pipe(takeUntil(this.destroy$))
        .subscribe(state => {
          console.log('💳 Payment flow state changed:', state);
          this.showPaymentFlow = state.isActive;
          this.cdr.detectChanges();
        });

      // Subscribe to conversation action refresh events
      this.conversationActions.refresh$
        .pipe(takeUntil(this.destroy$))
        .subscribe(() => {
          this.refreshCurrentConversation();
          this.loadExchanges(false);
          // ✅ Also refresh service task when conversation updates
          setTimeout(() => {
            this.loadCurrentServiceTaskFast();
          }, 1000);
        });

      // Subscribe to payment completion events
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
        // ✅ Refresh service task when conversation refreshes
        setTimeout(() => {
          this.loadCurrentServiceTaskFast();
        }, 500);
        break;
      case 'refresh-list':
        this.loadExchanges(false);
        break;
      case 'refresh-both':
        this.refreshCurrentConversation();
        this.loadExchanges(false);
        // ✅ Refresh service task when both refresh
        setTimeout(() => {
          this.loadCurrentServiceTaskFast();
        }, 500);
        break;
    }
  }

  /**
   * Subscribe to payment completion events
   */
  private subscribeToPaymentCompletion(): void {
    this.paymentFlowService.paymentCompleted$
      .pipe(takeUntil(this.destroy$))
      .subscribe(paymentDetails => {
        if (paymentDetails) {
          this.handlePaymentCompletion(paymentDetails);
        }
      });
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

  // Add these methods to the MessagesComponent class

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
   * Select a conversation and load service task
   */
  selectConversation(conversation: UserConversationDTO): void {
    // Clear current service task when switching conversations
    this.currentServiceTask = null;
    this.isServiceConfirmationVisible = false;
    this.chatOverlayVisible = false;

    this.messagesState.setSelectedUserInfo({
      userId: conversation.userId,
      fullName: conversation.userFullName,
      profilePictureUrl: conversation.userProfilePictureUrl
    });
    this.loadConversationMessages(conversation.userId);

    // ✅ Load service task immediately for new conversation
    setTimeout(() => {
      this.loadCurrentServiceTaskFast();
    }, 1000);
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
   * Load more conversations
   */
  loadMoreExchanges(): void {
    if (this.messagesState.conversationListMeta.hasMore && !this.messagesState.isLoading) {
      this.conversationListPagination.page++;
      this.loadExchanges(true);
    }
  }

  /**
   * Load more messages in current conversation
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

  // Add these methods to the MessagesComponent class

  /**
   * Send a message
   */
  async sendMessage(content: string): Promise<void> {
    const receiverId = this.messagesState.selectedUser;
    if (!content.trim() || !receiverId || this.isSendingMessage || !this.currentUserDetails) return;

    this.isSendingMessage = true;
    this.cdr.detectChanges();

    const result = await this.conversationActions.sendMessage(
      content.trim(),
      receiverId,
      this.currentUserDetails
    );

    console.log(result);

    this.isSendingMessage = false;

    if (result.success) {
      this.messageContent = '';
      this.refreshCurrentConversation();
    } else {
      this.messageContent = content.trim();
      this.notificationService.showNotification({
        type: 'error',
        message: result.error || 'Failed to send message'
      });
    }

    this.cdr.detectChanges();
  }

  /**
   * Accept a request
   */
  async acceptRequest(requestId: string): Promise<void> {
    const result = await this.conversationActions.acceptRequest(requestId);
    if (!result.success) {
      this.notificationService.showNotification({
        type: 'error',
        message: result.error || 'Failed to accept request'
      });
    }
  }

  /**
   * Reject a request
   */
  async rejectRequest(requestId: string): Promise<void> {
    const result = await this.conversationActions.rejectRequest(requestId);
    if (!result.success) {
      this.notificationService.showNotification({
        type: 'error',
        message: result.error || 'Failed to reject request'
      });
    }
  }

  /**
   * Accept a reply and initiate payment flow
   */
  async acceptReply(replyId: string): Promise<void> {
    console.log('🚀 Initiating payment flow for reply:', replyId);

    const result = await this.conversationActions.acceptReply(replyId);
    if (!result.success) {
      this.notificationService.showNotification({
        type: 'error',
        message: result.error || 'Failed to initiate payment for reply'
      });
      return;
    }

    this.notificationService.showNotification({
      type: 'info',
      message: 'Payment required to confirm service booking. Please complete payment.'
    });

    this.triggerPaymentFlowForReply(replyId);
  }

  /**
   * Reject a reply
   */
  async rejectReply(replyId: string): Promise<void> {
    const result = await this.conversationActions.rejectReply(replyId);
    if (!result.success) {
      this.notificationService.showNotification({
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
      this.notificationService.showNotification({
        type: 'error',
        message: result.error || 'Failed to submit reply'
      });
    }
  }

  // Add these methods to the MessagesComponent class

  /**
   * Wait for conversation to be selected before loading service task
   */
  private async waitForConversationSelection(): Promise<void> {
    return new Promise((resolve) => {
      const checkConversation = () => {
        const selectedUser = this.messagesState.selectedUser;

        console.log('⏳ Checking for selected user:', selectedUser);

        if (selectedUser) {
          console.log('✅ User selected:', selectedUser);
          resolve();
        } else {
          console.log('⏳ Still waiting for user selection...');
          setTimeout(checkConversation, 500);
        }
      };

      checkConversation();
    });
  }

  /**
   * Load service task for current conversation (FAST VERSION - SINGLE API CALL)
   */
  private async loadServiceTaskForCurrentConversation(): Promise<void> {
    try {
      console.log('🔄 Loading service task for current conversation...');

      // Wait for conversation to be selected
      await this.waitForConversationSelection();

      // Load service task for selected user
      await this.loadCurrentServiceTaskFast();

    } catch (error) {
      console.error('❌ Error loading service task for conversation:', error);
    }
  }

  /**
   * Load current service task using the new fast API (SINGLE API CALL)
   */
  private async loadCurrentServiceTaskFast(): Promise<void> {
    try {
      const selectedUser = this.messagesState.selectedUser;
      if (!selectedUser) {
        console.log('❌ No selected user to get service task for');
        return;
      }

      console.log('⚡ Loading current service task for user:', selectedUser);

      // Single API call to get current service task
      const response = await firstValueFrom(
        this.taskService.getCurrentServiceTask(selectedUser)
      );

      if (response?.response) {
        this.currentServiceTask = response.response;
        console.log('✅ Service task loaded instantly:', this.currentServiceTask);

        this.showServiceConfirmation();

        this.notificationService.showNotification({
          type: 'success',
          message: 'Service task loaded!'
        });
      } else {
        console.log('ℹ️ No current service task found for this conversation');

        if (response?.errorMessage) {
          console.log('ℹ️ Details:', response.errorMessage.message);
        }
      }

    } catch (error) {
      console.error('❌ Error loading current service task:', error);
    }
  }

  /**
   * Check if current user should see service task
   */
  private shouldShowServiceTask(): boolean {
    if (!this.currentServiceTask || !this.userId) {
      return false;
    }

    // User should see service task if they are either the client or specialist
    return this.userId === this.currentServiceTask.userId ||
      this.userId === this.currentServiceTask.specialistId;
  }

  /**
   * Enhanced show service confirmation with role check
   */
  private showServiceConfirmation(): void {
    if (!this.currentServiceTask) {
      console.warn('⚠️ Attempting to show service confirmation without service task');
      return;
    }

    if (!this.shouldShowServiceTask()) {
      console.warn('⚠️ Current user should not see this service task');
      return;
    }

    console.log('🎉 Showing service confirmation with task details');
    console.log('👤 Current user role in service:', {
      userId: this.userId,
      isClient: this.userId === this.currentServiceTask.userId,
      isSpecialist: this.userId === this.currentServiceTask.specialistId
    });

    this.isServiceConfirmationVisible = true;
    this.chatOverlayVisible = true;
  }

  /**
   * Hide service confirmation overlay
   */
  hideServiceConfirmation(): void {
    this.isServiceConfirmationVisible = false;
    this.chatOverlayVisible = false;
  }

  /**
   * Handle service task completion
   */
  onServiceTaskCompleted(event: { replyId: string; taskId: string }): void {
    console.log('Service task completed:', event);
    if (this.currentServiceTask) {
      this.currentServiceTask.status = 'Completed';
      this.currentServiceTask.completedAt = new Date();
    }
    this.cdr.detectChanges();
  }

  /**
   * Handle service task cancellation
   */
  onServiceTaskCancelled(event: { replyId: string; taskId: string }): void {
    console.log('Service task cancelled:', event);
    if (this.currentServiceTask) {
      this.currentServiceTask.status = 'Cancelled';
      this.currentServiceTask.cancelledAt = new Date();
    }
    this.cdr.detectChanges();
  }

  // Add these methods to the MessagesComponent class

  /**
   * Trigger payment flow for accepted reply
   */
  private async triggerPaymentFlowForReply(replyId: string): Promise<void> {
    try {
      const replyRes = await firstValueFrom(this.replyService.getReply(replyId));
      if (!replyRes || !replyRes.response) {
        console.error('Reply not found');
        this.triggerTestPaymentFlow(replyId);
        return;
      }

      const reply = replyRes.response;

      const serviceDetails = {
        replyId: reply.replyId,
        startDate: reply.startDate,
        endDate: reply.endDate,
        description: reply.description,
        address: reply.address,
        price: reply.price
      };

      const clientDetails = await firstValueFrom(this.userService.getUserPaymentDetails(reply.clientId));
      if (!clientDetails || !clientDetails.response) {
        console.error('Client details not found');
        this.triggerTestPaymentFlow(replyId);
        return;
      }

      const specialistDetails = await firstValueFrom(this.userService.getUserPaymentDetails(reply.specialistId));
      if (!specialistDetails || !specialistDetails.response) {
        console.error('Specialist details not found');
        this.triggerTestPaymentFlow(replyId);
        return;
      }

      this.paymentFlowService.initiatePaymentFlow(
        replyId,
        serviceDetails,
        clientDetails.response,
        specialistDetails.response,
        this.messagesState.selectedUser || ''
      );

      console.log('✅ Payment flow successfully triggered');
    } catch (err) {
      console.error('❌ Error during payment flow setup:', err);
      this.triggerTestPaymentFlow(replyId);
    }
  }

  /**
   * Fallback test payment flow
   */
  private triggerTestPaymentFlow(replyId: string): void {
    console.log('🧪 Using test payment flow data');

    const serviceDetails = {
      replyId: replyId,
      startDate: new Date(),
      endDate: new Date(Date.now() + 2 * 60 * 60 * 1000), // 2 hours later
      description: 'Test service booking',
      address: 'Test service address',
      price: 150
    };

    const userDetails = {
      userId: this.userId || 'test-user',
      userFullName: this.currentUserDetails?.fullName || 'Test User',
      email: this.currentUserDetails?.email || 'test@example.com',
      phoneNumber: '1234567890'
    };

    const specialistDetails = {
      userId: this.messagesState.selectedUser || 'test-specialist',
      userFullName: this.messagesState.selectedUserInfo?.fullName || 'Test Specialist',
      email: 'specialist@example.com',
      phoneNumber: '0987654321'
    };

    this.paymentFlowService.initiatePaymentFlow(
      replyId,
      serviceDetails,
      userDetails,
      specialistDetails,
      'test-conversation'
    );

    console.log('🚀 Test payment flow triggered for reply:', replyId);
  }

  /**
   * Handle payment completion
   */
  private async handlePaymentCompletion(paymentDetails: any): Promise<void> {
    console.log('💳 Payment completed in messages component:', paymentDetails);

    // Store payment details
    this.currentPaymentDetails = paymentDetails;

    // Show immediate success notification
    this.notificationService.showNotification({
      type: 'success',
      message: 'Payment completed successfully! Getting service details...'
    });

    // ✅ UPDATED: Use fast service task loading after payment completion
    setTimeout(async () => {
      console.log('🔄 Getting service task from payment...');

      // Get service task created by the backend (keep existing method for payment flow)
      await this.getServiceTaskFromPayment(paymentDetails.id);

      // ✅ Also refresh using fast method as backup
      setTimeout(() => {
        this.loadCurrentServiceTaskFast();
      }, 1000);

      // Refresh conversations after getting service task
      this.refreshCurrentConversation();
      this.loadExchanges(false);

    }, 2000); // Wait for backend processing to complete
  }

  /**
   * Get service task from payment (used after payment completion)
   */
  private async getServiceTaskFromPayment(paymentId: string): Promise<void> {
    try {
      console.log('📋 Step 1: Getting payment status for payment:', paymentId);

      // Step 1: Get payment status to get the ServiceTaskId
      const paymentStatusResponse = await firstValueFrom(
        this.paymentService.getPaymentStatus(paymentId)
      );

      console.log('Service task id:', paymentStatusResponse?.response?.serviceTaskId);

      if (!paymentStatusResponse?.response) {
        console.error('❌ Payment status not found');
        this.handleServiceTaskRetrievalFailure('Payment status not found');
        return;
      }

      const paymentStatus = paymentStatusResponse.response;
      console.log('💳 Payment status retrieved:', {
        paymentId: paymentStatus.paymentId,
        status: paymentStatus.status,
        serviceTaskId: paymentStatus.serviceTaskId,
        isEscrowed: paymentStatus.isEscrowed
      });

      // Step 2: Check if payment has a ServiceTaskId
      if (!paymentStatus.serviceTaskId) {
        console.warn('⚠️ Payment does not have a service task ID yet');

        // Retry after a delay (backend might still be processing)
        setTimeout(() => {
          console.log('🔄 Retrying service task retrieval...');
          this.getServiceTaskFromPayment(paymentId);
        }, 3000);
        return;
      }

      // Step 3: Get the service task using the ServiceTaskId
      console.log('🎯 Step 2: Getting service task with ID:', paymentStatus.serviceTaskId);

      const serviceTaskResponse = await firstValueFrom(
        this.taskService.getServiceTask(paymentStatus.serviceTaskId)
      );

      if (serviceTaskResponse?.response) {
        this.currentServiceTask = serviceTaskResponse.response;
        console.log('✅ Service task retrieved successfully:', {
          taskId: this.currentServiceTask.id,
          userId: this.currentServiceTask.userId,
          specialistId: this.currentServiceTask.specialistId,
          status: this.currentServiceTask.status,
          startDate: this.currentServiceTask.startDate,
          endDate: this.currentServiceTask.endDate,
          price: this.currentServiceTask.price
        });

        // Show service confirmation overlay with task details
        this.showServiceConfirmation();

        // Show final success notification
        this.notificationService.showNotification({
          type: 'success',
          message: 'Service confirmed successfully! Check your service details.'
        });
      } else {
        console.error('❌ Service task not found:', serviceTaskResponse?.errorMessage);
        this.handleServiceTaskRetrievalFailure('Service task not found');
      }

    } catch (error) {
      console.error('❌ Error fetching service task:', error);
      this.handleServiceTaskRetrievalFailure('Error retrieving service task');
    }
  }

  /**
   * Handle service task retrieval failure
   */
  private handleServiceTaskRetrievalFailure(reason: string): void {
    console.log('💡 Handling service task retrieval failure:', reason);

    this.notificationService.showNotification({
      type: 'warning',
      message: 'Payment completed but service details are still being processed. They will appear shortly.'
    });

    // ✅ Fallback: Try using the fast method
    setTimeout(() => {
      console.log('🔄 Trying fast service task loading as fallback...');
      this.loadCurrentServiceTaskFast();
    }, 2000);
  }

  // Add these methods to the MessagesComponent class

  /**
   * Open media picker for photo upload
   */
  openMediaPicker(): void {
    // Create file input element
    const fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.accept = 'image/jpeg,image/jpg,image/png,image/gif,image/webp';
    fileInput.style.display = 'none';

    fileInput.onchange = (event: any) => {
      const file = event.target.files?.[0];
      if (file) {
        this.handlePhotoSelection(file);
      }
    };

    document.body.appendChild(fileInput);
    fileInput.click();
    document.body.removeChild(fileInput);
  }

  /**
   * Handle photo file selection
   */
  private handlePhotoSelection(file: File): void {
    console.log('File: ', file);

    // Validate file
    const validation = this.photoService.validatePhotoFile(file);
    console.log(validation);

    if (!validation.isValid) {
      this.notificationService.showNotification({
        type: 'error',
        message: validation.error || 'Invalid file selected'
      });
      return;
    }

    this.selectedPhotoFile = file;
    this.uploadSelectedPhoto();
  }

  /**
   * Upload selected photo
   */
  private async uploadSelectedPhoto(): Promise<void> {
    if (!this.selectedPhotoFile || !this.messagesState.selectedUser) {
      return;
    }

    this.isUploadingPhoto = true;
    this.cdr.detectChanges();

    console.log(this.selectedPhotoFile);

    try {
      const response = await this.photoService.uploadPhotoToConversation(
        this.messagesState.selectedUser,
        this.selectedPhotoFile
      ).toPromise();

      if (response?.response) {
        this.notificationService.showNotification({
          type: 'success',
          message: 'Photo uploaded successfully!'
        });

        // Refresh conversation to show new photo
        this.refreshCurrentConversation();
        this.loadExchanges(false);
      } else {
        throw new Error(response?.errorMessage?.message || 'Failed to upload photo');
      }
    } catch (error) {
      console.error('Error uploading photo:', error);
      this.notificationService.showNotification({
        type: 'error',
        message: 'Failed to upload photo. Please try again.'
      });
    } finally {
      this.isUploadingPhoto = false;
      this.selectedPhotoFile = null;
      this.photoCaption = '';
      this.cdr.detectChanges();
    }
  }

  /**
   * Helper methods for finding conversation items
   */
  private getRequestItemById(requestId: string): any {
    const items = this.messagesState.getTypedConversationItems();
    return items.find(item => item.item.id === requestId && item.type === 'request');
  }

  private getReplyItemById(replyId: string): any {
    const items = this.messagesState.getTypedConversationItems();
    return items.find(item => item.item.id === replyId && item.type === 'reply');
  }
}
