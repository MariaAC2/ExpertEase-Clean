import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import {
  FirestoreConversationItemDTO,
  UserConversationDTO,
  MessageDTO,
  RequestDTO,
  ReplyDTO
} from '../models/api.models';
import { Timestamp } from 'firebase/firestore';

export interface ConversationItemWrapper {
  item: FirestoreConversationItemDTO;
  typed: MessageDTO | RequestDTO | ReplyDTO | null;
  type: 'message' | 'request' | 'reply' | 'unknown';
}

export interface SelectedUserInfo {
  userId: string;
  fullName: string;
  profilePictureUrl?: string;
}

export interface PaginationMeta {
  totalCount: number;
  hasMore: boolean;
}

@Injectable({
  providedIn: 'root'
})
export class MessagesStateService {
  // State subjects
  private readonly exchangesSubject = new BehaviorSubject<UserConversationDTO[]>([]);
  private readonly conversationItemsSubject = new BehaviorSubject<FirestoreConversationItemDTO[]>([]);
  private readonly selectedUserSubject = new BehaviorSubject<string | null>(null);
  private readonly selectedUserInfoSubject = new BehaviorSubject<SelectedUserInfo | null>(null);
  private readonly loadingSubject = new BehaviorSubject<boolean>(false);

  // Public observables
  public exchanges$ = this.exchangesSubject.asObservable();
  public conversationItems$ = this.conversationItemsSubject.asObservable();
  public selectedUser$ = this.selectedUserSubject.asObservable();
  public selectedUserInfo$ = this.selectedUserInfoSubject.asObservable();
  public loading$ = this.loadingSubject.asObservable();

  // Pagination metadata
  public conversationListMeta: PaginationMeta = { totalCount: 0, hasMore: false };
  public messagesMeta: PaginationMeta = { totalCount: 0, hasMore: false };

  // Getters for current values
  get exchanges(): UserConversationDTO[] {
    return this.exchangesSubject.value;
  }

  get conversationItems(): FirestoreConversationItemDTO[] {
    return this.conversationItemsSubject.value;
  }

  get selectedUser(): string | null {
    return this.selectedUserSubject.value;
  }

  get selectedUserInfo(): SelectedUserInfo | null {
    return this.selectedUserInfoSubject.value;
  }

  get isLoading(): boolean {
    return this.loadingSubject.value;
  }

  /**
   * Update exchanges list
   */
  public setExchanges(exchanges: UserConversationDTO[], append = false): void {
    const newExchanges = append ? [...this.exchanges, ...exchanges] : exchanges;
    this.exchangesSubject.next(newExchanges);
  }

  /**
   * Update conversation items
   */
  public setConversationItems(items: FirestoreConversationItemDTO[], append = false): void {
    const newItems = append
      ? [...items, ...this.conversationItems] // Prepend for messages (newer first)
      : [...this.conversationItems, ...items]; // Append for initial load
    this.conversationItemsSubject.next(newItems);
  }

  /**
   * Add optimistic message
   */
  public addOptimisticMessage(content: string, senderId: string): FirestoreConversationItemDTO {
    const tempMessage: FirestoreConversationItemDTO = {
      id: `temp_${Date.now()}`,
      conversationId: 'temp',
      senderId: senderId,
      type: 'message',
      createdAt: Timestamp.fromDate(new Date()),
      data: {
        Content: content.trim(),
        SenderId: senderId,
        CreatedAt: new Date(),
        IsRead: false
      }
    };

    const currentItems = this.conversationItems;
    this.conversationItemsSubject.next([...currentItems, tempMessage]);
    return tempMessage;
  }

  /**
   * Remove optimistic message (on error)
   */
  public removeOptimisticMessage(tempId: string): void {
    const filteredItems = this.conversationItems.filter(item => item.id !== tempId);
    this.conversationItemsSubject.next(filteredItems);
  }

  /**
   * Update message read status
   */
  public updateMessageReadStatus(messageId: string, readAt: Date): void {
    const updatedItems = this.conversationItems.map(item => {
      if (item.id === messageId && item.type === 'message') {
        return {
          ...item,
          data: {
            ...item.data,
            IsRead: true,
            ReadAt: readAt
          }
        };
      }
      return item;
    });

    this.conversationItemsSubject.next(updatedItems);
  }

  /**
   * Set selected user
   */
  public setSelectedUser(userId: string | null): void {
    this.selectedUserSubject.next(userId);
  }

  /**
   * Set selected user info
   */
  public setSelectedUserInfo(userInfo: SelectedUserInfo | null): void {
    this.selectedUserInfoSubject.next(userInfo);
  }

  /**
   * Set loading state
   */
  public setLoading(loading: boolean): void {
    this.loadingSubject.next(loading);
  }

  /**
   * Clear conversation items
   */
  public clearConversationItems(): void {
    this.conversationItemsSubject.next([]);
  }

  /**
   * Clear all state
   */
  public clearAll(): void {
    this.exchangesSubject.next([]);
    this.conversationItemsSubject.next([]);
    this.selectedUserSubject.next(null);
    this.selectedUserInfoSubject.next(null);
    this.loadingSubject.next(false);
    this.conversationListMeta = { totalCount: 0, hasMore: false };
    this.messagesMeta = { totalCount: 0, hasMore: false };
  }

  /**
   * Get typed conversation items for template
   */
  public getTypedConversationItems(): ConversationItemWrapper[] {
    const items = this.conversationItems;
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
    console.log('Deserializing conversation item:', item);

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
            // Backend uses PascalCase, so prioritize those
            requestedStartDate: this.convertTimestamp(data['RequestedStartDate'] || data['requestedStartDate']),
            description: data['Description'] || data['description'] || '',
            status: data['Status'] || data['status'] || 'Pending',
            senderContactInfo: data['SenderContactInfo'] || data['senderContactInfo'],
            // Additional backend fields that might be present
            address: data['Address'] || data['address'],
            phoneNumber: data['PhoneNumber'] || data['phoneNumber']
          } as RequestDTO;

        case 'reply':
          return {
            ...baseFields,
            // Backend uses PascalCase, so prioritize those
            requestId: data['RequestId'] || data['requestId'] || '',
            startDate: this.convertTimestamp(data['StartDate'] || data['startDate']),
            endDate: this.convertTimestamp(data['EndDate'] || data['endDate']),
            price: Number(data['Price'] || data['price'] || 0),
            status: data['Status'] || data['status'] || 'Pending',
            serviceTask: data['ServiceTask'] || data['serviceTask'],
            // Additional backend fields that might be present
            specialistId: data['SpecialistId'] || data['specialistId'],
            acceptedAt: data['AcceptedAt'] ? this.convertTimestamp(data['AcceptedAt']) : undefined,
            rejectedAt: data['RejectedAt'] ? this.convertTimestamp(data['RejectedAt']) : undefined
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
}
