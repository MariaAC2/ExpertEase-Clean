import { ChangeDetectorRef, Component, OnDestroy, OnInit, TrackByFunction } from '@angular/core';
import {firstValueFrom, Observable, Subject, takeUntil} from 'rxjs';
import {
  UserConversationDTO,
  UserDTO,
  MessageDTO,
  RequestDTO,
  ReplyDTO,
  ConversationItemDTO, UserProfileDTO, PhotoDTO // Updated import
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
import {AsyncPipe, NgClass, NgForOf, NgIf, NgSwitch, NgSwitchCase, SlicePipe} from '@angular/common';
import {MessageBubbleComponent} from '../../shared/message-bubble/message-bubble.component';
import {RequestMessageComponent} from '../../shared/request-message/request-message.component';
import {ReplyMessageComponent} from '../../shared/reply-message/reply-message.component';
import {ServiceMessageComponent} from '../../shared/service-message/service-message.component';
import {ReplyFormComponent} from '../../shared/reply-form/reply-form.component';
import {RouterLink} from '@angular/router';
import {ServicePaymentComponent} from '../../shared/service-payment/service-payment.component';
import {NotificationService} from '../../services/notification.service';
import {NotificationDisplayComponent} from '../../shared/notification-display/notification-display.component';
import {TaskService} from '../../services/task.service';
import {RequestService} from '../../services/request.service';
import {ReplyService} from '../../services/reply.service';
import {PhotoBubbleComponent} from '../../shared/photo-bubble/photo-bubble.component';
import {PhotoService} from '../../services/photo.service';

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

  // Service confirmations (keep existing payment flow logic)
  isServiceConfirmationVisible: boolean = false;
  currentPaymentDetails: any;
  currentServiceTask: any;
  showPaymentFlow: boolean = false;

  isUploadingPhoto: boolean = false;
  selectedPhotoFile: File | null = null;
  photoCaption: string = '';

  // Updated observables to use ConversationItemDTO
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
    private readonly replyService: ReplyService
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
        .subscribe(notification => this.notificationService.showNotification(notification));

      // Subscribe to conversation updates
      this.signalRHandler.conversationUpdate$
        .pipe(takeUntil(this.destroy$))
        .subscribe(update => this.handleConversationUpdate(update));

      // Subscribe to action refresh events
      this.paymentFlowService.paymentFlow$
        .pipe(takeUntil(this.destroy$))
        .subscribe(state => {
          console.log('💳 Payment flow state changed:', state);
          this.showPaymentFlow = state.isActive;
          this.cdr.detectChanges();
        });

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

  // 🆕 ADD: Trigger payment flow for accepted reply
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

      const client = clientDetails.response;

      const specialistDetails = await firstValueFrom(this.userService.getUserPaymentDetails(reply.specialistId));
      if (!specialistDetails || !specialistDetails.response) {
        console.error('Specialist details not found');
        this.triggerTestPaymentFlow(replyId);
        return;
      }
      const specialist = specialistDetails.response;
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

  private getRequestItemById(requestId: string): any {
    const items = this.messagesState.getTypedConversationItems();
    return items.find(item => item.item.id === requestId && item.type === 'request');
  }

// 🆕 ADD: Helper method to find reply by ID
  private getReplyItemById(replyId: string): any {
    const items = this.messagesState.getTypedConversationItems();
    return items.find(item => item.item.id === replyId && item.type === 'reply');
  }

// 🆕 ADD: Fallback test payment flow
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

  // Rest of the methods remain the same...
  loadMoreExchanges(): void {
    if (this.messagesState.conversationListMeta.hasMore && !this.messagesState.isLoading) {
      this.conversationListPagination.page++;
      this.loadExchanges(true);
    }
  }

  loadMoreMessages(): void {
    const selectedUser = this.messagesState.selectedUser;
    if (selectedUser && this.messagesState.messagesMeta.hasMore && !this.messagesState.isLoading) {
      this.messagesPagination.page++;
      this.loadConversationMessages(selectedUser, true);
    }
  }

  private refreshCurrentConversation(): void {
    const selectedUser = this.messagesState.selectedUser;
    if (selectedUser) {
      this.loadConversationMessages(selectedUser, false);
    }
  }

  async sendMessage(content: string): Promise<void> {
    const receiverId = this.messagesState.selectedUser; // ← This is the receiver's user ID
    if (!content.trim() || !receiverId || this.isSendingMessage || !this.currentUserDetails) return;

    this.isSendingMessage = true;
    this.cdr.detectChanges();

    const result = await this.conversationActions.sendMessage(
      content.trim(),
      receiverId, // ← Pass receiver ID directly
      this.currentUserDetails
    );

    console.log(result);

    this.isSendingMessage = false;

    if (result.success) {
      this.messageContent = '';
      this.refreshCurrentConversation(); // ← This bypasses SignalR
    } else {
      this.messageContent = content.trim();
      this.notificationService.showNotification({
        type: 'error',
        message: result.error || 'Failed to send message'
      });
    }

    this.cdr.detectChanges();
  }

  async acceptRequest(requestId: string): Promise<void> {
    const result = await this.conversationActions.acceptRequest(requestId);
    if (!result.success) {
      this.notificationService.showNotification({
        type: 'error',
        message: result.error || 'Failed to accept request'
      });
    }
  }

  async rejectRequest(requestId: string): Promise<void> {
    const result = await this.conversationActions.rejectRequest(requestId);
    if (!result.success) {
      this.notificationService.showNotification({
        type: 'error',
        message: result.error || 'Failed to reject request'
      });
    }
  }

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

    // ✅ Show informative notification about the process
    this.notificationService.showNotification({
      type: 'info',
      message: 'Payment required to confirm service booking. Please complete payment.'
    });

    // ✅ Trigger payment flow - payment component will handle the rest
    this.triggerPaymentFlowForReply(replyId);
  }

  async rejectReply(replyId: string): Promise<void> {
    const result = await this.conversationActions.rejectReply(replyId);
    if (!result.success) {
      this.notificationService.showNotification({
        type: 'error',
        message: result.error || 'Failed to reject reply'
      });
    }
  }

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

  hideReplyForm(): void {
    this.isReplyFormVisible = false;
    this.chatOverlayVisible = false;
    this.currentRequestId = null;
    this.cdr.detectChanges();
  }

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

  // Keep existing payment flow methods...
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
    console.log('💳 Payment completed in messages component:', paymentDetails);

    // ✅ Enhanced: Wait a moment for backend processing then refresh
    setTimeout(() => {
      console.log('🔄 Refreshing conversations after payment completion...');
      this.refreshCurrentConversation();
      this.loadExchanges(false);
    }, 1500); // Slightly longer delay to ensure backend processing completes

    // ✅ Optional: Create service task if needed
    if (paymentDetails) {
      this.currentPaymentDetails = paymentDetails;
      this.createServiceTaskFromPayment(paymentDetails);

      // ✅ Show service confirmation overlay
      this.showServiceConfirmation();
    }
  }

  private async createServiceTaskFromPayment(paymentDetails: any): Promise<void> {
    console.log('🛠️ Creating service task from payment:', paymentDetails);

    if (!this.authService.getUserId()) {
      console.error('❌ User not authenticated');
      this.notificationService.showNotification({
        type: 'error',
        message: 'Authentication required to create service task'
      });
      return;
    }

    try {
      // Using firstValueFrom for better async/await support
      const response = await firstValueFrom(this.taskService.addTaskFromPayment(paymentDetails.id));
      console.log('The service task in question: ', response.response?.serviceTask);

      if (response.response?.serviceTask) {
        this.currentServiceTask = response.response.serviceTask;
        console.log('✅ Service task created successfully:', this.currentServiceTask);
      } else {
        const errorMsg = response.errorMessage?.message || 'Failed to create service task after payment';
        console.error('❌ Failed to create service task:', response.errorMessage);
        this.notificationService.showNotification({
          type: 'error',
          message: errorMsg
        });
        return;
      }
    } catch (error) {
      console.error('❌ Error creating service task:', error);
      this.notificationService.showNotification({
        type: 'error',
        message: 'Error creating service task after payment'
      });
      return;
    }

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
    // Create file input element
    const fileInput = document.createElement('input');
    fileInput.type = 'file';
    fileInput.accept = 'image/jpeg,image/jpg,image/png,image/gif,image/webp';
    fileInput.style.display = 'none';

    fileInput.onchange = (event: any) => {
      const file = event.target.files?.[0];
      // console.log('File:', file);
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
      // console.log("Poza invalida");
      // this.notificationService.showNotification({
      //   type: 'error',
      //   message: validation.error || 'Invalid file selected'
      // });
      return;
    }

    this.selectedPhotoFile = file;
    // console.log(this.selectedPhotoFile);
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

  ngOnDestroy(): void {
    this.signalRHandler.cleanup();
    this.signalRService.disconnect();
    this.messagesState.clearAll();
    this.destroy$.next();
    this.destroy$.complete();
  }
}
