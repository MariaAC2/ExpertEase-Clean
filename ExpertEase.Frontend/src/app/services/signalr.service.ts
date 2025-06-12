import { Injectable } from '@angular/core';
import * as signalR from '@microsoft/signalr';

@Injectable({ providedIn: 'root' })
export class SignalRService {
  private hubConnection!: signalR.HubConnection;

  connect(userId: string | undefined): void {
    this.hubConnection = new signalR.HubConnectionBuilder()
      .withUrl('http://localhost:5241/hubs/conversations') // Replace with your actual URL
      .build();

    this.hubConnection
      .start()
      .then(() => {
        console.log('SignalR connected');
        this.hubConnection.invoke('Join', userId);
      })
      .catch(err => console.error('SignalR failed to connect:', err));
  }

  onNewMessage(callback: (message: any) => void): void {
    this.hubConnection.on('ReceiveNewMessage', callback);
  }

  disconnect(): void {
    this.hubConnection.stop();
  }
}
