import { Injectable } from '@angular/core';
import {AuthService} from './auth.service';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {PortfolioPictureAddDTO, RequestResponse} from '../models/api.models';
import {Observable} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class PhotoService {
  private readonly baseUrl = `http://localhost:5241/api/Photo`
  constructor(private readonly http: HttpClient, private readonly authService: AuthService) { }

  addProfilePicture(file: File) {
    const formData = new FormData();
    formData.append('file', file);
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.post(`${this.baseUrl}/AddProfilePicture`, formData, { headers });
  }

  updateProfilePicture(file: File){
    const formData = new FormData();
    formData.append('file', file);
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.patch(`${this.baseUrl}/UpdateProfilePicture`, formData, { headers });
  }

  addPortfolioPicture(fileData: PortfolioPictureAddDTO) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.post(`${this.baseUrl}/AddPortfolioPicture`, fileData, {headers});
  }

  deletePortfolioPicture(photoId: string){
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.delete(`${this.baseUrl}/DeletePortfolioPicture/${photoId}`, {headers});
  }

  uploadPhotoToConversation(
    conversationId: string,
    file: File,
    caption?: string
  ): Observable<RequestResponse<any>> {
    const formData = new FormData();
    formData.append('file', file);
    formData.append('conversationId', conversationId);

    if (caption) {
      formData.append('caption', caption);
    }

    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });

    return this.http.post<RequestResponse<any>>(
      `${this.baseUrl}/AddConversationPhoto/${conversationId}`,
      formData,
      { headers }
    );
  }

  /**
   * Validate file before upload
   */
  validatePhotoFile(file: File): { isValid: boolean; error?: string } {
    const maxSize = 10 * 1024 * 1024; // 10MB
    const allowedTypes = ['image/jpeg', 'image/jpg', 'image/png', 'image/gif', 'image/webp'];

    if (!allowedTypes.includes(file.type)) {
      return {
        isValid: false,
        error: 'Invalid file type. Please select a JPEG, PNG, GIF, or WebP image.'
      };
    }

    if (file.size > maxSize) {
      return {
        isValid: false,
        error: 'File size too large. Maximum size is 10MB.'
      };
    }

    return { isValid: true };
  }
}
