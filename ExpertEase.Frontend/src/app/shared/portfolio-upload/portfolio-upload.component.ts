import {Component, EventEmitter, Input, Output, OnDestroy} from '@angular/core';
import {PhotoService} from '../../services/photo.service';
import {DomSanitizer} from '@angular/platform-browser';
import {NgForOf, NgIf} from '@angular/common';

@Component({
  selector: 'app-portfolio-upload',
  imports: [
    NgForOf,
    NgIf
  ],
  templateUrl: './portfolio-upload.component.html',
  styleUrl: './portfolio-upload.component.scss'
})
export class PortfolioUploadComponent implements OnDestroy {
  @Input() uploadedFiles: File[] = [];
  @Output() portfolioChange = new EventEmitter<File[]>();

  imagePreviews: string[] = [];

  constructor(private readonly photoService: PhotoService, private readonly sanitizer: DomSanitizer) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;

    for (let i = 0; i < input.files.length; i++) {
      const file = input.files[i];

      // Add file to the array
      this.uploadedFiles.push(file);

      // Create preview URL
      const previewUrl = URL.createObjectURL(file);
      this.imagePreviews.push(previewUrl);
    }

    // Emit the updated files array
    this.portfolioChange.emit([...this.uploadedFiles]);

    // Reset the input
    input.value = '';
  }

  remove(index: number) {
    // Revoke the object URL to prevent memory leaks
    if (this.imagePreviews[index]) {
      URL.revokeObjectURL(this.imagePreviews[index]);
    }

    // Remove from both arrays
    this.uploadedFiles.splice(index, 1);
    this.imagePreviews.splice(index, 1);

    // Emit the updated files array
    this.portfolioChange.emit([...this.uploadedFiles]);
  }

  // Helper method to get file size in readable format
  getFileSize(file: File): string {
    const bytes = file.size;
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  }

  // Cleanup when component is destroyed
  ngOnDestroy() {
    // Revoke all object URLs to prevent memory leaks
    this.imagePreviews.forEach(url => {
      if (url.startsWith('blob:')) {
        URL.revokeObjectURL(url);
      }
    });
  }

  // Clear all files
  clearAll() {
    // Revoke all object URLs
    this.imagePreviews.forEach(url => {
      if (url.startsWith('blob:')) {
        URL.revokeObjectURL(url);
      }
    });

    // Clear arrays
    this.uploadedFiles = [];
    this.imagePreviews = [];

    // Emit empty array
    this.portfolioChange.emit([]);
  }
}
