import { Injectable } from '@angular/core';
import {HttpClient, HttpHeaders} from '@angular/common/http';
import {AuthService} from './auth.service';
import {RequestResponse, ServiceTaskDTO, ServiceTaskUpdateDTO} from '../models/api.models';
import {Observable} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class TaskService {

  private readonly baseUrl: string = 'http://localhost:5241/api/ServiceTask';
  constructor(private readonly http: HttpClient, private readonly authService: AuthService) { }

  addTaskFromPayment(paymentId: string): Observable<RequestResponse<any>> {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.post(`${this.baseUrl}/AddTaskToPayment/${paymentId}`, {}, { headers });
  }

  getServiceTask(taskId: string){
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.get<RequestResponse<ServiceTaskDTO>>(
      `${this.baseUrl}/GetById/${taskId}`,
      { headers }
    );
  }
  getCurrentServiceTask(otherUserId: string){
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.get<RequestResponse<ServiceTaskDTO>>(
      `${this.baseUrl}/GetCurrent/${otherUserId}`,
      { headers }
    );
  }
  updateServiceTask(task: ServiceTaskUpdateDTO){
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.patch(
      `${this.baseUrl}/Update/${task.id}`,
      task,
      { headers }
    );
  }

  completeServiceTask(taskId: string){
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.patch(
      `${this.baseUrl}/Complete/${taskId}`,
      {},
      { headers }
    );
  }

  cancelServiceTask(taskId: string) {
    const token = this.authService.getToken();
    const headers = new HttpHeaders({
      Authorization: `Bearer ${token}`
    });
    return this.http.patch(
      `${this.baseUrl}/Cancel/${taskId}`, {}, { headers }
    );
  }
}
