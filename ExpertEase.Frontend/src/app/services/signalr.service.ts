import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel, HubConnectionState } from '@microsoft/signalr';
import { BehaviorSubject, Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class SignalRService {
  private hubConnection: HubConnection | null = null;
  private readonly connectionStateSubject = new BehaviorSubject<boolean>(false);
  private reconnectAttempts = 0;
  private readonly maxReconnectAttempts = 5;
  private readonly reconnectInterval = 2000; // Start with 2 seconds

  public connectionState$ = this.connectionStateSubject.asObservable();

  constructor() {
    // Connection will be initialized when connect() is called
  }

  /**
   * Connect to SignalR hub with user authentication
   */
  public async connect(userId: string | undefined): Promise<void> {
    if (!userId) {
      console.warn('Cannot connect to SignalR without userId');
      return;
    }

    try {
      // Create connection if it doesn't exist
      if (!this.hubConnection) {
        this.createConnection();
      }

      // Only connect if not already connected
      if (this.hubConnection?.state === HubConnectionState.Disconnected) {
        await this.hubConnection.start();
        console.log('SignalR connected successfully');

        // Join user-specific group
        await this.hubConnection.invoke('Join', userId);
        console.log(`Joined SignalR group for user: ${userId}`);

        this.connectionStateSubject.next(true);
        this.reconnectAttempts = 0; // Reset reconnect attempts on successful connection
      }
    } catch (error) {
      console.error('Failed to connect to SignalR:', error);
      this.connectionStateSubject.next(false);
      this.scheduleReconnect(userId);
    }
  }

  /**
   * Create SignalR hub connection with configuration
   */
  private createConnection(): void {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl('http://localhost:5241/hubs/conversations', {
        withCredentials: true, // Include authentication cookies
        skipNegotiation: false
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect([0, 2000, 10000, 30000]) // Custom retry intervals
      .build();

    this.setupConnectionEventHandlers();
  }

  /**
   * Setup connection event handlers
   */
  private setupConnectionEventHandlers(): void {
    if (!this.hubConnection) return;

    // Handle reconnecting
    this.hubConnection.onreconnecting((error) => {
      console.log('SignalR reconnecting...', error);
      this.connectionStateSubject.next(false);
    });

    // Handle reconnected
    this.hubConnection.onreconnected((connectionId) => {
      console.log('SignalR reconnected with connection ID:', connectionId);
      this.connectionStateSubject.next(true);
      this.reconnectAttempts = 0;
    });

    // Handle connection closed
    this.hubConnection.onclose((error) => {
      console.log('SignalR connection closed:', error);
      this.connectionStateSubject.next(false);
    });
  }

  /**
   * Schedule automatic reconnection
   */
  private scheduleReconnect(userId: string): void {
    if (this.reconnectAttempts >= this.maxReconnectAttempts) {
      console.error('Max reconnection attempts reached. Giving up.');
      return;
    }

    this.reconnectAttempts++;
    const delay = this.reconnectInterval * Math.pow(2, this.reconnectAttempts - 1); // Exponential backoff

    console.log(`Scheduling reconnection attempt ${this.reconnectAttempts} in ${delay}ms`);

    setTimeout(() => {
      this.connect(userId);
    }, delay);
  }

  /**
   * Disconnect from SignalR hub
   */
  public async disconnect(): Promise<void> {
    if (this.hubConnection) {
      try {
        await this.hubConnection.stop();
        console.log('SignalR disconnected');
        this.connectionStateSubject.next(false);
      } catch (error) {
        console.error('Error disconnecting SignalR:', error);
      }
    }
  }

  /**
   * Register event listener
   */
  public on(methodName: string, callback: (...args: any[]) => void): void {
    if (this.hubConnection) {
      this.hubConnection.on(methodName, callback);
    } else {
      console.warn(`Cannot register listener for ${methodName}: connection not established`);
    }
  }

  /**
   * Remove event listener
   */
  public off(methodName: string): void {
    if (this.hubConnection) {
      this.hubConnection.off(methodName);
    }
  }

  /**
   * Invoke server method
   */
  public async invoke(methodName: string, ...args: any[]): Promise<any> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      try {
        return await this.hubConnection.invoke(methodName, ...args);
      } catch (error) {
        console.error(`Error invoking ${methodName}:`, error);
        throw error;
      }
    } else {
      const errorMsg = `Cannot invoke ${methodName}: SignalR not connected`;
      console.warn(errorMsg);
      throw new Error(errorMsg);
    }
  }

  /**
   * Send message to server (fire and forget)
   */
  public async send(methodName: string, ...args: any[]): Promise<void> {
    if (this.hubConnection?.state === HubConnectionState.Connected) {
      try {
        await this.hubConnection.send(methodName, ...args);
      } catch (error) {
        console.error(`Error sending ${methodName}:`, error);
        throw error;
      }
    } else {
      const errorMsg = `Cannot send ${methodName}: SignalR not connected`;
      console.warn(errorMsg);
      throw new Error(errorMsg);
    }
  }

  /**
   * Check if connected
   */
  public isConnected(): boolean {
    return this.hubConnection?.state === HubConnectionState.Connected;
  }

  /**
   * Get current connection state
   */
  public getConnectionState(): HubConnectionState {
    return this.hubConnection?.state || HubConnectionState.Disconnected;
  }

  /**
   * Get connection ID
   */
  public getConnectionId(): string | null {
    return this.hubConnection?.connectionId || null;
  }

  /**
   * Send message notification
   */
  public async notifyNewMessage(recipientId: string, payload: any): Promise<void> {
    await this.invoke('NotifyNewMessage', recipientId, payload);
  }

  /**
   * Send request notification
   */
  public async notifyNewRequest(recipientId: string, payload: any): Promise<void> {
    await this.invoke('NotifyNewRequest', recipientId, payload);
  }

  /**
   * Send reply notification
   */
  public async notifyNewReply(recipientId: string, payload: any): Promise<void> {
    await this.invoke('NotifyNewReply', recipientId, payload);
  }

  /**
   * Join a specific group (useful for different chat rooms or conversations)
   */
  public async joinGroup(groupName: string): Promise<void> {
    await this.invoke('JoinGroup', groupName);
  }

  /**
   * Leave a specific group
   */
  public async leaveGroup(groupName: string): Promise<void> {
    await this.invoke('LeaveGroup', groupName);
  }

  /**
   * Listen for new messages
   */
  public onNewMessage(callback: (payload: any) => void): void {
    this.on('ReceiveNewMessage', callback);
  }

  /**
   * Listen for message read notifications
   */
  public onMessageRead(callback: (payload: any) => void): void {
    this.on('ReceiveMessageRead', callback);
  }

  /**
   * Listen for new requests
   */
  public onNewRequest(callback: (payload: any) => void): void {
    this.on('ReceiveNewRequest', callback);
  }

  /**
   * Listen for request status changes
   */
  public onRequestStatusChanged(callback: (payload: any) => void): void {
    this.on('ReceiveNewRequestAccepted', callback);
    this.on('ReceiveNewRequestRejected', callback);
    this.on('ReceiveNewRequestCancelled', callback);
  }

  /**
   * Listen for new replies
   */
  public onNewReply(callback: (payload: any) => void): void {
    this.on('ReceiveNewReply', callback);
  }

  /**
   * Listen for reply status changes
   */
  public onReplyStatusChanged(callback: (payload: any) => void): void {
    this.on('ReceiveNewReplyAccepted', callback);
    this.on('ReceiveNewReplyRejected', callback);
    this.on('ReceiveNewReplyCancelled', callback);
  }

  /**
   * Remove all message listeners
   */
  public removeMessageListeners(): void {
    this.off('ReceiveNewMessage');
    this.off('ReceiveMessageRead');
  }

  /**
   * Remove all request listeners
   */
  public removeRequestListeners(): void {
    this.off('ReceiveNewRequest');
    this.off('ReceiveNewRequestAccepted');
    this.off('ReceiveNewRequestRejected');
    this.off('ReceiveNewRequestCancelled');
  }

  /**
   * Remove all reply listeners
   */
  public removeReplyListeners(): void {
    this.off('ReceiveNewReply');
    this.off('ReceiveNewReplyAccepted');
    this.off('ReceiveNewReplyRejected');
    this.off('ReceiveNewReplyCancelled');
  }

  /**
   * Listen for service completion notifications
   */
  public onServiceCompleted(callback: (payload: any) => void): void {
    this.on('ServiceCompleted', callback);
  }

  /**
   * Listen for review received notifications
   */
  public onReviewReceived(callback: (payload: any) => void): void {
    this.on('ReviewReceived', callback);
  }

  /**
   * Listen for review prompt notifications
   */
  public onReviewPrompt(callback: (payload: any) => void): void {
    this.on('ReviewPrompt', callback);
  }

  /**
   * Listen for service status changes
   */
  public onServiceStatusChanged(callback: (payload: any) => void): void {
    this.on('ServiceStatusChanged', callback);
  }

  public removeReviewListeners(): void {
    this.off('ServiceCompleted');
    this.off('ReviewReceived');
    this.off('ReviewPrompt');
    this.off('ServiceStatusChanged');
  }

  /**
   * Remove all listeners
   */
  public removeAllListeners(): void {
    this.removeMessageListeners();
    this.removeRequestListeners();
    this.removeReplyListeners();
    this.removeReviewListeners(); // 🆕 Add this line
  }
}
