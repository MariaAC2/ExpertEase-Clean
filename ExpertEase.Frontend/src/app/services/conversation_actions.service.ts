import { Injectable } from '@angular/core';
import { Observable, Subject } from 'rxjs';
import { MessageService } from './message.service';
import { RequestService } from './request.service';
import { ReplyService } from './reply.service';
import { UserDTO } from '../models/api.models';
import {SignalRHandlerService} from './signalr_handler.service';
import {MessagesStateService} from './messages_state.service';

@Injectable({
  providedIn: 'root'
})
export class ConversationActionsService {
  private refreshTrigger = new Subject<void>();
  public refresh$ = this.refreshTrigger.asObservable();

  constructor(
    private messageService: MessageService,
    private requestService: RequestService,
    private replyService: ReplyService,
    private signalRHandler: SignalRHandlerService,
    private messagesState: MessagesStateService
  ) {}

  /**
   * Send a message
   */
  public async sendMessage(
    content: string,
    recipientId: string,
    currentUser: UserDTO
  ): Promise<{ success: boolean; error?: string }> {
    if (!content.trim()) {
      return { success: false, error: 'Message content cannot be empty' };
    }

    // Add optimistic update
    const tempMessage = this.messagesState.addOptimisticMessage(content.trim(), currentUser.id);

    try {
      const response = await this.messageService.sendMessage(recipientId, { content: content.trim() }).toPromise();

      if (response?.response?.id) {
        // Send SignalR notification
        await this.signalRHandler.sendMessageNotification(
          recipientId,
          response.response.id,
          content.trim(),
          currentUser.fullName || 'Someone',
          currentUser.id
        );

        // Trigger refresh
        this.triggerRefresh();
        return { success: true };
      } else {
        // Remove optimistic update on failure
        this.messagesState.removeOptimisticMessage(tempMessage.id);
        return { success: false, error: 'Failed to send message' };
      }
    } catch (error) {
      console.error('Error sending message:', error);
      // Remove optimistic update on error
      this.messagesState.removeOptimisticMessage(tempMessage.id);
      return { success: false, error: 'Failed to send message' };
    }
  }

  /**
   * Accept a request
   */
  public async acceptRequest(requestId: string): Promise<{ success: boolean; error?: string }> {
    try {
      await this.requestService.acceptRequest(requestId).toPromise();
      this.triggerRefresh();
      return { success: true };
    } catch (error) {
      console.error('Error accepting request:', error);
      return { success: false, error: 'Failed to accept request' };
    }
  }

  /**
   * Reject a request
   */
  public async rejectRequest(requestId: string): Promise<{ success: boolean; error?: string }> {
    try {
      await this.requestService.rejectRequest(requestId).toPromise();
      this.triggerRefresh();
      return { success: true };
    } catch (error) {
      console.error('Error rejecting request:', error);
      return { success: false, error: 'Failed to reject request' };
    }
  }

  /**
   * Accept a reply
   */
  public async acceptReply(replyId: string): Promise<{ success: boolean; error?: string }> {
    try {
      await this.replyService.acceptReply(replyId);
      this.triggerRefresh();
      return { success: true };
    } catch (error) {
      console.error('Error accepting reply:', error);
      return { success: false, error: 'Failed to accept reply' };
    }
  }

  /**
   * Reject a reply
   */
  public async rejectReply(replyId: string): Promise<{ success: boolean; error?: string }> {
    try {
      await this.replyService.rejectReply(replyId).toPromise();
      this.triggerRefresh();
      return { success: true };
    } catch (error) {
      console.error('Error rejecting reply:', error);
      return { success: false, error: 'Failed to reject reply' };
    }
  }

  /**
   * Submit a reply
   */
  public async submitReply(requestId: string, replyData: any): Promise<{ success: boolean; error?: string }> {
    try {
      await this.replyService.addReply(requestId, replyData).toPromise();
      this.triggerRefresh();
      return { success: true };
    } catch (error) {
      console.error('Error submitting reply:', error);
      return { success: false, error: 'Failed to submit reply' };
    }
  }

  /**
   * Trigger a refresh of conversations and messages
   */
  private triggerRefresh(): void {
    console.log('Triggering refresh of conversations and messages');
    this.refreshTrigger.next();
  }
}
