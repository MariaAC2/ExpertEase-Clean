// Create a new file: mock-exchange.service.ts

import { Injectable } from '@angular/core';
import { Observable, of, delay } from 'rxjs';
import {
  RequestResponse,
  PagedResponse,
  UserConversationDTO,
  FirestoreConversationItemDTO
} from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class MockExchangeService {

  private mockConversations: UserConversationDTO[] = [
    {
      userId: "user_specialist_456",
      userFullName: "Ana Popescu",
      userProfilePictureUrl: "assets/avatar.svg",
      lastMessage: 'Oferta este valabilă până mâine!',
      lastMessageAt: new Date("2024-01-01T10:45:00Z"),
      unreadCount: 1,
    },
    {
      userId: "user_specialist_789",
      userFullName: "Ion Vasile",
      userProfilePictureUrl: "assets/avatar.svg",
      lastMessage: 'Bună ziua! Cum merge treaba?',
      lastMessageAt: new Date("2024-01-01T10:00:00Z"),
      unreadCount: 0,
    },

  ];

  private mockConversationItems: FirestoreConversationItemDTO[] = [
    {
      id: "msg_001",
      conversationId: "conv_12345",
      senderId: "user_client_123",
      type: "message",
      createdAt: new Date("2024-01-01T10:00:00Z") as any,
      data: {
        Content: "Bună ziua! Cum merge treaba?",
        SenderId: "user_client_123",
        CreatedAt: new Date("2024-01-01T10:00:00Z"),
        IsRead: true
      }
    },
    {
      id: "req_001",
      conversationId: "conv_12345",
      senderId: "user_client_123",
      type: "request",
      createdAt: new Date("2024-01-01T10:15:00Z") as any,
      data: {
        RequestId: "req_001",
        RequestedStartDate: new Date("2024-01-02T09:00:00Z"),
        PhoneNumber: "0722123456",
        Address: "Strada Libertății nr. 25, București",
        Description: "Instalare aer condiționat în living",
        Status: "Accepted",
        SenderId: "user_client_123"
      }
    },
    {
      id: "reply_001",
      conversationId: "conv_12345",
      senderId: "user_specialist_456",
      type: "reply",
      createdAt: new Date("2024-01-01T10:30:00Z") as any,
      data: {
        RequestId: "req_001",
        StartDate: new Date("2024-01-02T09:00:00Z"),
        EndDate: new Date("2024-01-02T12:00:00Z"),
        Price: 450.00,
        Status: "Pending",
        SenderId: "user_specialist_456"
      }
    },
    {
      id: "msg_002",
      conversationId: "conv_12345",
      senderId: "user_specialist_456",
      type: "message",
      createdAt: new Date("2024-01-01T10:45:00Z") as any,
      data: {
        Content: "Oferta este valabilă până mâine!",
        CreatedAt: new Date("2024-01-01T10:45:00Z"),
        IsRead: false
      }
    }
  ];

  getExchanges(pagination?: { page: number; pageSize: number }): Observable<RequestResponse<PagedResponse<UserConversationDTO>>> {
    const response: RequestResponse<PagedResponse<UserConversationDTO>> = {
      response: {
        page: pagination?.page || 1,
        pageSize: pagination?.pageSize || 20,
        totalCount: this.mockConversations.length,
        data: this.mockConversations
      },
      errorMessage: undefined
    };

    return of(response).pipe(delay(300)); // Simulate network delay
  }

  getExchange(
    userId: string,
    pagination?: { page: number; pageSize: number }
  ): Observable<RequestResponse<PagedResponse<FirestoreConversationItemDTO>>> {
    const response: RequestResponse<PagedResponse<FirestoreConversationItemDTO>> = {
      response: {
        page: pagination?.page || 1,
        pageSize: pagination?.pageSize || 50,
        totalCount: this.mockConversationItems.length,
        data: this.mockConversationItems
      },
      errorMessage: undefined
    };

    return of(response).pipe(delay(500)); // Simulate network delay
  }
}
