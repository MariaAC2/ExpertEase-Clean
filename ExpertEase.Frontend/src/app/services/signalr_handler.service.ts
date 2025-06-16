import { Injectable } from '@angular/core';
import { Subject } from 'rxjs';
import { SignalRService } from './signalr.service';

export interface NotificationEvent {
  type: 'success' | 'info' | 'warning' | 'error';
  message: string;
}

export interface ConversationUpdateEvent {
  type: 'refresh-current' | 'refresh-list' | 'refresh-both';
  userId?: string;
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

  // Private handler methods
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
}
