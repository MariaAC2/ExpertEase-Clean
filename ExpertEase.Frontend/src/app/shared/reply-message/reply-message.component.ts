import {Component, EventEmitter, Input, OnInit, Output} from '@angular/core';
import {ReplyDTO, StatusEnum} from '../../models/api.models';
import {AuthService} from '../../services/auth.service';
import {CurrencyPipe, DatePipe, NgClass, NgIf} from '@angular/common';

@Component({
  selector: 'app-reply-message',
  imports: [
    DatePipe,
    NgIf,
    CurrencyPipe,
    NgClass
  ],
  templateUrl: './reply-message.component.html',
  styleUrl: './reply-message.component.scss'
})
export class ReplyMessageComponent implements OnInit {
  @Input() reply: ReplyDTO = {
      id: '',
      senderId: '',
      startDate: new Date(),
      endDate: new Date(),
      price: 0,
      status: StatusEnum.Pending,
      requestId: '',
  }
  userRole: string | null = '';
  @Input() requestId: string = '';
  @Input() currentUserId: string | null | undefined; // Add this input

  @Output() replyAccepted = new EventEmitter<{ requestId: string; replyId: string }>();
  @Output() replyRejected = new EventEmitter<{ requestId: string; replyId: string }>();

  constructor(
    private readonly authService: AuthService,
  ) {}

  ngOnInit(): void {
    this.userRole = this.authService.getUserRole();
    console.log('Reply info:', this.reply);
  }

  acceptReply() {
    this.replyAccepted.emit({ requestId: this.requestId, replyId: this.reply.id });
  }

  rejectReply() {
    this.replyRejected.emit({ requestId: this.requestId, replyId: this.reply.id });
  }

  get isOwnMessage(): boolean {
    return this.reply.senderId === this.currentUserId;
  }
}
