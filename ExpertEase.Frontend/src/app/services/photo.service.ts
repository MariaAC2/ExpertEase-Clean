import { Injectable } from '@angular/core';
import {AuthService} from './auth.service';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {PortfolioPictureAddDTO} from '../models/api.models';

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
}
