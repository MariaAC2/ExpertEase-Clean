import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable } from 'rxjs';
import {
  FirestoreConversationItemDTO,
  UserConversationDTO,
  MessageDTO,
  RequestDTO,
  ReplyDTO, ConversationItemDTO,
  PhotoDTO
} from '../models/api.models';
import { Timestamp } from 'firebase/firestore';
import Photo = google.maps.places.Photo;

export interface ConversationItemWrapper {
  item: ConversationItemDTO;
  typed: MessageDTO | RequestDTO | ReplyDTO | PhotoDTO | null;
  type: 'message' | 'request' | 'reply' | 'photo' | 'unknown';
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
  private readonly conversationItemsSubject = new BehaviorSubject<ConversationItemDTO[]>([]);
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

  private readonly selectedConversationIdSubject = new BehaviorSubject<string | null>(null);
  public selectedConversationId$ = this.selectedConversationIdSubject.asObservable();

  // Getters for current values
  get exchanges(): UserConversationDTO[] {
    return this.exchangesSubject.value;
  }

  get conversationItems(): ConversationItemDTO[] {
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

  get selectedConversationId(): string | null {
    return this.selectedConversationIdSubject.value;
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
  public setConversationItems(items: ConversationItemDTO[], append = false): void {
    const newItems = append
      ? [...items, ...this.conversationItems] // Prepend for messages (newer first)
      : [...this.conversationItems, ...items]; // Append for initial load
    this.conversationItemsSubject.next(newItems);
  }

  /**
   * Add optimistic message
   */
  public addOptimisticMessage(content: string, senderId: string): ConversationItemDTO {
    const tempMessage: ConversationItemDTO = {
      id: `temp_${Date.now()}`,
      conversationId: 'temp',
      senderId: senderId,
      type: 'message',
      createdAt: new Date(),
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

  public setSelectedConversation(conversationId: string, userId: string, userInfo: SelectedUserInfo): void {
    this.selectedConversationIdSubject.next(conversationId);
    this.selectedUserSubject.next(userId);
    this.selectedUserInfoSubject.next(userInfo);
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
    this.selectedConversationIdSubject.next(null); // ← ADD THIS
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
      // ✅ FIX: Include photo type in the mapping
      const type = typed ? item.type as ('message' | 'request' | 'reply' | 'photo') : 'unknown';
      return { item, typed, type };
    });
  }

  /**
   * Enhanced deserialize method with photo support
   */
  private deserializeItem(item: ConversationItemDTO): MessageDTO | RequestDTO | ReplyDTO | PhotoDTO | null {
    if (!item.data || !item.type) {
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
            status: data['Status'] || data['status'] || 'Pending',
            senderContactInfo: data['SenderContactInfo'] || data['senderContactInfo'],
            address: data['Address'] || data['address'],
            phoneNumber: data['PhoneNumber'] || data['phoneNumber']
          } as RequestDTO;

        case 'reply':
          return {
            ...baseFields,
            requestId: data['RequestId'] || data['requestId'] || '',
            startDate: this.convertTimestamp(data['StartDate'] || data['startDate']),
            endDate: this.convertTimestamp(data['EndDate'] || data['endDate']),
            price: Number(data['Price'] || data['price'] || 0),
            status: data['Status'] || data['status'] || 'Pending',
            serviceTask: data['ServiceTask'] || data['serviceTask'],
            specialistId: data['SpecialistId'] || data['specialistId'],
            acceptedAt: data['AcceptedAt'] ? this.convertTimestamp(data['AcceptedAt']) : undefined,
            rejectedAt: data['RejectedAt'] ? this.convertTimestamp(data['RejectedAt']) : undefined
          } as ReplyDTO;

        // ✅ ADD PHOTO CASE
        case 'photo':
          return {
            ...baseFields,
            url: data['Url'] || data['url'] || data['DownloadUrl'] || data['downloadUrl'] || '',
            fileName: data['FileName'] || data['fileName'] || data['Name'] || data['name'] || 'Unknown',
            contentType: data['ContentType'] || data['contentType'] || data['MimeType'] || data['mimeType'] || 'image/jpeg',
            caption: data['Caption'] || data['caption'] || '',
            createdAt: this.convertTimestamp(data['CreatedAt'] || data['createdAt'] || item.createdAt),
            fileSize: Number(data['FileSize'] || data['fileSize'] || data['Size'] || data['size'] || 0)
          } as PhotoDTO;

        default:
          console.warn(`Unknown conversation item type: ${item.type}`, item);
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
