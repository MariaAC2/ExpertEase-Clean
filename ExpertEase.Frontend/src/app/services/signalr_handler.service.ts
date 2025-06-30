import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { SignalRService } from './signalr.service';

export interface NotificationEvent {
  type: 'success' | 'info' | 'warning' | 'error';
  message: string;
}

export interface ConversationUpdateEvent {
  type: 'refresh-current' | 'refresh-list' | 'refresh-both' | 'service-completed' | 'review-prompt';
  userId?: string;
  data?: any; // 🆕 Add data field for additional info
}

@Injectable({
  providedIn: 'root'
})
export class SignalRHandlerService {
  private notificationSubject = new Subject<NotificationEvent>();
  private conversationUpdateSubject = new Subject<ConversationUpdateEvent>();

  public notification$ = this.notificationSubject.asObservable();
  public conversationUpdate$ = this.conversationUpdateSubject.asObservable();

  constructor(private signalRService: SignalRService) {}

  /**
   * Initialize SignalR listeners
   */
  public setupListeners(currentUserId: string): void {
    this.signalRService.removeAllListeners();

    // Message listeners
    this.signalRService.onNewMessage((payload: any) => {
      this.handleNewMessageReceived(payload, currentUserId);
    });

    this.signalRService.onMessageRead((payload: any) => {
      this.handleMessageRead(payload);
    });

    // Request listeners
    this.signalRService.onNewRequest((payload: any) => {
      this.handleNewRequestReceived(payload);
    });

    this.signalRService.on('ReceiveRequestAccepted', (payload: any) => {
      this.handleRequestAccepted(payload);
    });

    this.signalRService.on('ReceiveRequestRejected', (payload: any) => {
      this.handleRequestRejected(payload);
    });

    this.signalRService.on('ReceiveRequestCompleted', (payload: any) => {
      this.handleRequestRejected(payload);
    });

    this.signalRService.on('ReceiveRequestCancelled', (payload: any) => {
      this.handleRequestCancelled(payload);
    });

    // Reply listeners
    this.signalRService.onNewReply((payload: any) => {
      this.handleNewReplyReceived(payload);
    });

    this.signalRService.on('ReceiveReplyAccepted', (payload: any) => {
      this.handleReplyAccepted(payload);
    });

    this.signalRService.on('ReceiveReplyRejected', (payload: any) => {
      this.handleReplyRejected(payload);
    });

    this.signalRService.on('ReceiveReplyCancelled', (payload: any) => {
      this.handleReplyCancelled(payload);
    });

    // 🆕 Review system listeners - ADD THESE
    this.signalRService.onServiceCompleted((payload: any) => {
      this.handleServiceCompleted(payload);
    });

    this.signalRService.onReviewReceived((payload: any) => {
      this.handleReviewReceived(payload);
    });

    this.signalRService.onReviewPrompt((payload: any) => {
      this.handleReviewPrompt(payload);
    });

    this.signalRService.onServiceStatusChanged((payload: any) => {
      this.handleServiceStatusChanged(payload);
    });
  }

  /**
   * Send message notification
   */
  public async sendMessageNotification(recipientId: string, messageId: string, content: string, senderName: string, senderId: string): Promise<void> {
    try {
      const payload = {
        MessageId: messageId,
        SenderId: senderId,
        RecipientId: recipientId,
        Content: content,
        SenderName: senderName,
        SentAt: new Date(),
        Message: 'You have a new message'
      };

      await this.signalRService.notifyNewMessage(recipientId, payload);
    } catch (error) {
      console.error('Failed to send message notification:', error);
    }
  }

  /**
   * Cleanup listeners
   */
  public cleanup(): void {
    this.signalRService.removeAllListeners();
  }

  // Private handler methods - KEEP ALL EXISTING ONES

  private handleNewMessageReceived(payload: any, currentUserId: string): void {
    const { SenderId, Content, SenderName } = payload;

    if (SenderId !== currentUserId) {
      const shortContent = Content.length > 50 ? Content.substring(0, 50) + '...' : Content;
      this.notificationSubject.next({
        type: 'info',
        message: `New message from ${SenderName}: ${shortContent}`
      });
    }

    this.conversationUpdateSubject.next({ type: 'refresh-both' });
  }

  private handleMessageRead(payload: any): void {
    // This will be handled by the conversation component directly
    this.conversationUpdateSubject.next({ type: 'refresh-current' });
  }

  private handleNewRequestReceived(payload: any): void {
    const { SenderName } = payload;
    this.notificationSubject.next({
      type: 'info',
      message: `New service request from ${SenderName}`
    });
    this.conversationUpdateSubject.next({ type: 'refresh-both' });
  }

  private handleRequestAccepted(payload: any): void {
    this.notificationSubject.next({
      type: 'success',
      message: 'Your service request has been accepted!'
    });
    this.conversationUpdateSubject.next({ type: 'refresh-both' });
  }

  private handleRequestRejected(payload: any): void {
    this.notificationSubject.next({
      type: 'info',
      message: 'Your service request has been declined.'
    });
    this.conversationUpdateSubject.next({ type: 'refresh-both' });
  }

  private handleRequestCancelled(payload: any): void {
    this.notificationSubject.next({
      type: 'warning',
      message: 'A service request has been cancelled.'
    });
    this.conversationUpdateSubject.next({ type: 'refresh-both' });
  }

  private handleNewReplyReceived(payload: any): void {
    const { SpecialistName, Price, StartDate } = payload;
    const startDate = new Date(StartDate).toLocaleDateString();
    this.notificationSubject.next({
      type: 'info',
      message: `New offer from ${SpecialistName}: $${Price} (${startDate})`
    });
    this.conversationUpdateSubject.next({ type: 'refresh-both' });
  }

  private handleReplyAccepted(payload: any): void {
    this.notificationSubject.next({
      type: 'success',
      message: 'Great! Your service offer has been accepted.'
    });
    this.notificationSubject.next({
      type: 'info',
      message: 'Payment processing has begun.'
    });
    this.conversationUpdateSubject.next({ type: 'refresh-both' });
  }

  private handleReplyRejected(payload: any): void {
    this.notificationSubject.next({
      type: 'info',
      message: 'Your service offer has been declined.'
    });
    this.conversationUpdateSubject.next({ type: 'refresh-both' });
  }

  private handleReplyCancelled(payload: any): void {
    this.notificationSubject.next({
      type: 'warning',
      message: 'A service offer has been cancelled.'
    });
    this.conversationUpdateSubject.next({ type: 'refresh-both' });
  }

  // 🆕 ADD THESE NEW REVIEW SYSTEM HANDLERS

  /**
   * Handle service completion notification
   */
  private handleServiceCompleted(payload: any): void {
    console.log('🎉 Service completed event received:', payload);

    const { ServiceDescription, TransferCompleted, Message } = payload;

    this.notificationSubject.next({
      type: 'success',
      message: Message || `Serviciul "${ServiceDescription}" a fost finalizat cu succes! ${TransferCompleted ? 'Plata a fost transferată.' : ''}`
    });

    // Trigger service task refresh
    this.conversationUpdateSubject.next({
      type: 'service-completed',
      data: payload
    });

    // Also refresh conversations
    this.conversationUpdateSubject.next({ type: 'refresh-both' });
  }

  /**
   * Handle review received notification
   */
  private handleReviewReceived(payload: any): void {
    console.log('⭐ Review received event:', payload);

    const { ReviewerName, Rating, Message } = payload;

    this.notificationSubject.next({
      type: 'success',
      message: Message || `Ai primit o nouă recenzie de ${Rating} stele de la ${ReviewerName}!`
    });

    // Don't refresh conversations for review notifications
    // Reviews don't change the conversation content directly
  }

  /**
   * Handle review prompt notification
   */
  private handleReviewPrompt(payload: any): void {
    console.log('📝 Review prompt event received:', payload);

    const { ReviewTargetName, ReviewTargetRole, TaskId } = payload;
    const roleText = ReviewTargetRole === 'specialist' ? 'specialistul' : 'clientul';

    this.notificationSubject.next({
      type: 'info',
      message: `Poți lăsa o recenzie pentru ${roleText} ${ReviewTargetName}!`
    });

    // Trigger review prompt
    this.conversationUpdateSubject.next({
      type: 'review-prompt',
      data: payload
    });
  }

  /**
   * Handle service status change notification
   */
  private handleServiceStatusChanged(payload: any): void {
    console.log('🔄 Service status changed event:', payload);

    const { NewStatus, OldStatus, Message, HasMoneyTransfer } = payload;

    let notificationType: 'success' | 'info' | 'warning' | 'error' = 'info';
    let defaultMessage = `Status serviciu actualizat: ${NewStatus}`;

    switch (NewStatus) {
      case 'Completed':
        notificationType = 'success';
        defaultMessage = HasMoneyTransfer ?
          'Serviciul a fost finalizat și plata a fost transferată!' :
          'Serviciul a fost finalizat cu succes!';
        break;
      case 'Cancelled':
        notificationType = 'warning';
        defaultMessage = 'Serviciul a fost anulat.';
        break;
      case 'Reviewed':
        notificationType = 'success';
        defaultMessage = 'Procesul de recenzie a fost finalizat. Mulțumim!';
        break;
      case 'Confirmed':
        notificationType = 'info';
        defaultMessage = 'Serviciul este acum în desfășurare.';
        break;
    }

    this.notificationSubject.next({
      type: notificationType,
      message: Message || defaultMessage
    });

    // Refresh service task and conversations
    this.conversationUpdateSubject.next({ type: 'refresh-both' });
  }
}
