import {Component, Input} from '@angular/core';
import {MessageDTO} from '../../models/api.models';
import {DatePipe, NgClass} from '@angular/common';

@Component({
  selector: 'app-message-bubble',
  imports: [
    DatePipe,
    NgClass
  ],
  templateUrl: './message-bubble.component.html',
  styleUrl: './message-bubble.component.scss'
})
export class MessageBubbleComponent {
  @Input() message!: MessageDTO;
  @Input() currentUserId: string | null | undefined;

  get isOwnMessage(): boolean {
    return this.message.senderId === this.currentUserId;
  }
}
