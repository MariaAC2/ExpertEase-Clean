import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {AuthService} from './auth.service';
import {RequestResponse, ServiceTaskDTO, ServiceTaskUpdateDTO} from '../models/api.models';

@Injectable({
  providedIn: 'root'
})
export class TaskService {

  private baseUrl: string = 'http://localhost:5241/api';
  constructor(private http: HttpClient, private authService: AuthService) { }

  getServiceTask(replyId: string, taskId: string){
    const headers = this.authService.getAuthHeaders();
    return this.http.get<RequestResponse<ServiceTaskDTO>>(
      `${this.baseUrl}/user/replies/${replyId}/task/${taskId}`,
      { headers }
    );
  }
  updateServiceTask(replyId: string, task: ServiceTaskUpdateDTO){
    const headers = this.authService.getAuthHeaders();
    return this.http.patch(
      `${this.baseUrl}/specialist/replies/${replyId}/task/${task.id}`,
      task,
      { headers }
    );
  }

  completeServiceTask(replyId: string, taskId: string){
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.patch(
      `${this.baseUrl}/specialist/replies/${replyId}/task/${taskId}/complete`,
      {},
      { headers }
    );
  }

  cancelServiceTask(replyId: string, taskId: string) {
    const headers = this.authService.getAuthHeaders();
    return this.http.patch(
      `${this.baseUrl}/specialist/replies/${replyId}/task/${taskId}/cancel`, {}, { headers }
    );
  }
}
