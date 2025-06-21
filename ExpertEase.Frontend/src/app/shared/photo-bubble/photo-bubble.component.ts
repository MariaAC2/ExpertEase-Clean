import {Component, Input, OnInit} from '@angular/core';
import {ConversationItemDTO, PhotoDTO} from '../../models/api.models';
import {DatePipe, NgClass, NgIf} from '@angular/common';

@Component({
  selector: 'app-photo-bubble',
  imports: [
    NgIf,
    DatePipe,
    NgClass
  ],
  templateUrl: './photo-bubble.component.html',
  styleUrl: './photo-bubble.component.scss'
})
export class PhotoBubbleComponent {
  @Input() photoItem!: PhotoDTO; // Use ConversationItemDTO instead
  @Input() currentUserId: string | null | undefined;

  imageLoading = true;
  imageError = false;

  get isOwnMessage(): boolean {
    return this.photoItem.senderId === this.currentUserId;
  }

  onImageError(event: any): void {
    this.imageLoading = false;
    this.imageError = true;
  }

  onImageLoad(): void {
    this.imageLoading = false;
    this.imageError = false;
  }

  openPhotoViewer(): void {
    if (!this.imageError && this.photoItem.url) {
      // Open photo in new tab/window for now
      // You can implement a modal photo viewer later
      window.open(this.photoItem.url, '_blank');
    }
  }
}
