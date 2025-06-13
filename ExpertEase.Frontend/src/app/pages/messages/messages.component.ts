import { Component, OnDestroy, OnInit } from '@angular/core';
import { Timestamp } from 'firebase/firestore';
import {
  ConversationDTO,
  FirestoreConversationItemDTO,
  MessageDTO,
  ReplyDTO,
  RequestDTO,
  StatusEnum,
  UserConversationDTO,
  MessageAddDTO
} from '../../models/api.models';
import {AuthService} from '../../services/auth.service';
import {ExchangeService} from '../../services/exchange.service';
import {MessageService} from '../../services/message.service';
import {RequestService} from '../../services/request.service';
import {RequestMessageComponent} from '../../shared/request-message/request-message.component';
import {MessageBubbleComponent} from '../../shared/message-bubble/message-bubble.component';
import {JsonPipe, NgForOf, NgIf, NgSwitch, NgSwitchCase} from '@angular/common';
import {RouterLink} from '@angular/router';
import {FormsModule} from '@angular/forms';

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
    JsonPipe,
    FormsModule
  ],
  styleUrl: './messages.component.scss'
})
export class MessagesComponent implements OnInit, OnDestroy {
  messageContent: string = '';
  userId: string | null | undefined;
  exchanges: UserConversationDTO[] = [];
  selectedExchange: ConversationDTO | undefined | null;

  // Type guards for runtime validation
  private typeGuards: TypeGuards = {
    isMessage: (data: any): data is MessageDTO => {
      return data &&
        typeof data.id === 'string' &&
        typeof data.senderId === 'string' &&
        typeof data.content === 'string' &&
        typeof data.isRead === 'boolean' &&
        data.createdAt instanceof Date;
    },

    isRequest: (data: any): data is RequestDTO => {
      return data &&
        typeof data.id === 'string' &&
        typeof data.senderId === 'string' &&
        typeof data.description === 'string' &&
        data.requestedStartDate instanceof Date &&
        Object.values(StatusEnum).includes(data.status);
    },

    isReply: (data: any): data is ReplyDTO => {
      return data &&
        typeof data.id === 'string' &&
        typeof data.senderId === 'string' &&
        typeof data.requestId === 'string' &&
        typeof data.price === 'number' &&
        data.startDate instanceof Date &&
        data.endDate instanceof Date &&
        Object.values(StatusEnum).includes(data.status);
    }
  };

  constructor(
    private readonly authService: AuthService,
    private readonly exchangeService: ExchangeService,
    private readonly messageService: MessageService,
    private readonly requestService: RequestService
  ) {}

  ngOnInit() {
    this.userId = this.authService.getUserId();
    this.loadExchanges();
  }

  /**
   * Safe timestamp conversion utility
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
    console.warn('Unknown timestamp format:', timestamp);
    return new Date();
  }

  /**
   * Enhanced deserialization with proper validation and error handling
   */
  deserializeItem(item: FirestoreConversationItemDTO): MessageDTO | RequestDTO | ReplyDTO | null {
    if (!item || !item.data || !item.type) {
      console.error('Invalid conversation item structure:', item);
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
          const messageData = {
            ...baseFields,
            content: data['Content'] || data['content'] || '',
            createdAt: this.convertTimestamp(data['CreatedAt'] || data['createdAt'] || item.createdAt),
            isRead: Boolean(data['IsRead'] ?? data['isRead'] ?? false)
          } as MessageDTO;

          if (this.typeGuards.isMessage(messageData)) {
            return messageData;
          }
          break;

        case 'request':
          const requestData = {
            ...baseFields,
            requestedStartDate: this.convertTimestamp(
              data['RequestedStartDate'] || data['requestedStartDate']
            ),
            description: data['Description'] || data['description'] || '',
            status: data['Status'] || data['status'] || StatusEnum.Pending,
            senderContactInfo: data['SenderContactInfo'] || data['senderContactInfo']
          } as RequestDTO;

          if (this.typeGuards.isRequest(requestData)) {
            return requestData;
          }
          break;

        case 'reply':
          const replyData = {
            ...baseFields,
            requestId: data['RequestId'] || data['requestId'] || '',
            startDate: this.convertTimestamp(data['StartDate'] || data['startDate']),
            endDate: this.convertTimestamp(data['EndDate'] || data['endDate']),
            price: Number(data['Price'] || data['price'] || 0),
            status: data['Status'] || data['status'] || StatusEnum.Pending,
            serviceTask: data['ServiceTask'] || data['serviceTask']
          } as ReplyDTO;

          if (this.typeGuards.isReply(replyData)) {
            return replyData;
          }
          break;

        default:
          console.warn('Unknown conversation item type:', item.type);
          return null;
      }
    } catch (error) {
      console.error('Error deserializing conversation item:', error, item);
      return null;
    }

    console.error('Failed type validation for item:', item.type, data);
    return null;
  }

  /**
   * Safe casting methods with fallback to deserialization
   */
  asMessage(item: FirestoreConversationItemDTO): MessageDTO {
    const deserialized = this.deserializeItem(item);
    if (deserialized && this.typeGuards.isMessage(deserialized)) {
      return deserialized;
    }

    // Fallback with basic validation
    console.warn('Using fallback message deserialization for item:', item.id);
    return {
      id: item.id,
      senderId: item.senderId,
      content: item.data['Content'] || item.data['content'] || '[Invalid message]',
      createdAt: this.convertTimestamp(item.createdAt),
      isRead: Boolean(item.data['IsRead'] ?? item.data['isRead'] ?? false)
    } as MessageDTO;
  }

  asRequest(item: FirestoreConversationItemDTO): RequestDTO {
    const deserialized = this.deserializeItem(item);
    if (deserialized && this.typeGuards.isRequest(deserialized)) {
      return deserialized;
    }

    console.warn('Using fallback request deserialization for item:', item.id);
    return {
      id: item.id,
      senderId: item.senderId,
      requestedStartDate: this.convertTimestamp(item.data['RequestedStartDate'] || item.data['requestedStartDate']),
      description: item.data['Description'] || item.data['description'] || '[Invalid request]',
      status: item.data['Status'] || item.data['status'] || StatusEnum.Pending
    } as RequestDTO;
  }

  asReply(item: FirestoreConversationItemDTO): ReplyDTO {
    const deserialized = this.deserializeItem(item);
    if (deserialized && this.typeGuards.isReply(deserialized)) {
      return deserialized;
    }

    console.warn('Using fallback reply deserialization for item:', item.id);
    return {
      id: item.id,
      senderId: item.senderId,
      requestId: item.data['RequestId'] || item.data['requestId'] || '',
      startDate: this.convertTimestamp(item.data['StartDate'] || item.data['startDate']),
      endDate: this.convertTimestamp(item.data['EndDate'] || item.data['endDate']),
      price: Number(item.data['Price'] || item.data['price'] || 0),
      status: item.data['Status'] || item.data['status'] || StatusEnum.Pending
    } as ReplyDTO;
  }

  /**
   * Get properly typed conversation items
   */
  getTypedConversationItems(): Array<{
    item: FirestoreConversationItemDTO;
    typed: MessageDTO | RequestDTO | ReplyDTO | null;
    type: 'message' | 'request' | 'reply' | 'unknown';
  }> {
    if (!this.selectedExchange?.conversationItems) return [];

    return this.selectedExchange.conversationItems.map(item => {
      const typed = this.deserializeItem(item);
      const type = typed ? item.type as ('message' | 'request' | 'reply') : 'unknown';

      return { item, typed, type };
    });
  }

  /**
   * Type-safe getters for specific item types
   */
  getMessages(): MessageDTO[] {
    return this.getTypedConversationItems()
      .filter(item => item.type === 'message' && item.typed)
      .map(item => item.typed as MessageDTO);
  }

  getRequests(): RequestDTO[] {
    return this.getTypedConversationItems()
      .filter(item => item.type === 'request' && item.typed)
      .map(item => item.typed as RequestDTO);
  }

  getReplies(): ReplyDTO[] {
    return this.getTypedConversationItems()
      .filter(item => item.type === 'reply' && item.typed)
      .map(item => item.typed as ReplyDTO);
  }

  /**
   * Safe type casting methods for template use
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

  loadExchanges(): void {
    this.exchangeService.getExchanges().subscribe({
      next: (res) => {
        this.exchanges = res.response ?? [];
        console.log('Conversations loaded:', this.exchanges);

        if (this.exchanges.length > 0) {
          const lastUser = this.exchanges[this.exchanges.length - 1];
          this.loadExchange(lastUser.userId);
        }
      },
      error: (err) => {
        console.error('Error loading conversations:', err);
      }
    });
  }

  loadExchange(senderUserId: string | undefined): void {
    if (!senderUserId) return;

    console.log('Loading exchange for user:', senderUserId);
    this.exchangeService.getExchange(senderUserId).subscribe({
      next: (res) => {
        console.log('Exchange loaded:', res.response);
        this.selectedExchange = res.response;

        // Validate conversation items
        if (this.selectedExchange?.conversationItems) {
          const typedItems = this.getTypedConversationItems();
          const invalidItems = typedItems.filter(item => item.type === 'unknown');

          if (invalidItems.length > 0) {
            console.warn(`Found ${invalidItems.length} invalid conversation items:`, invalidItems);
          }
        }
      },
      error: (err) => {
        console.error('Failed to reload exchange', err);
      }
    });
  }

  sendMessage(content: string): void {
    if (!content.trim() || !this.selectedExchange) {
      console.error('Cannot send empty message or no exchange selected');
      return;
    }

    const message: MessageAddDTO = { content: content.trim() };

    this.messageService.sendMessage(this.selectedExchange.conversationId, message).subscribe({
      next: (res) => {
        console.log('Message sent successfully:', res);
        this.loadExchange(this.selectedExchange?.userId);
        this.messageContent = '';
      },
      error: (err) => {
        console.error('Error sending message:', err);
      }
    });
  }

  acceptRequest(requestId: string): void {
    this.requestService.acceptRequest(requestId).subscribe({
      next: () => {
        console.log('Request accepted');
        this.loadExchange(this.selectedExchange?.userId);
      },
      error: (err) => {
        console.error('Error accepting request:', err);
      }
    });
  }

  rejectRequest(requestId: string): void {
    this.requestService.rejectRequest(requestId).subscribe({
      next: () => {
        console.log('Request rejected');
        this.loadExchange(this.selectedExchange?.userId);
      },
      error: (err) => {
        console.error('Error rejecting request:', err);
      }
    });
  }

  ngOnDestroy(): void {
    // Cleanup logic here
  }

  openMediaPicker() {

  }
}
