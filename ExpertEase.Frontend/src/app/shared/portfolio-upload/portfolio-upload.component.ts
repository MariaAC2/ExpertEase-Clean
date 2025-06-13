import {Component, EventEmitter, Input, Output} from '@angular/core';
import {PhotoService} from '../../services/photo.service';
import {DomSanitizer} from '@angular/platform-browser';
import {PortfolioPictureAddDTO} from '../../models/api.models';
import {NgForOf} from '@angular/common';

@Component({
  selector: 'app-portfolio-upload',
  imports: [
    NgForOf
  ],
  templateUrl: './portfolio-upload.component.html',
  styleUrl: './portfolio-upload.component.scss'
})
export class PortfolioUploadComponent {
  @Input() uploadedPictures: PortfolioPictureAddDTO[] = [];
  @Output() portfolioChange = new EventEmitter<PortfolioPictureAddDTO[]>();

  imagePreviews: string[] = [];

  constructor(private readonly photoService: PhotoService, private readonly sanitizer: DomSanitizer) {}

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;

    for (const element of input.files) {
      const file = element;
      const newImage: PortfolioPictureAddDTO = {
        fileStream: file,
        contentType: file.type,
        fileName: file.name
      };

      this.uploadedPictures.push(newImage);
      this.portfolioChange.emit(this.uploadedPictures);

      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreviews.push(reader.result as string);
      };
      reader.readAsDataURL(file);
    }
  }

  remove(index: number) {
    this.uploadedPictures.splice(index, 1);
    this.imagePreviews.splice(index, 1);
    this.portfolioChange.emit(this.uploadedPictures);
  }
}
