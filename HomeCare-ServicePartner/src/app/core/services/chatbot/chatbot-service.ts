import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { IChatRequest, IChatResponse } from '../../models/chatbot/IChatbot';
import { IApiResponse } from '../../models/api-response/api-response';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({ providedIn: 'root' })
export class ChatBotService {
  private readonly apiUrl = `${API_BASE_URL}/api/chatbot/reply`;

  private logoutResetSource = new Subject<void>();
  logoutReset$ = this.logoutResetSource.asObservable();

  constructor(private http: HttpClient) {}

  sendMessage(request: IChatRequest): Observable<IApiResponse<IChatResponse>> {
    return this.http.post<IApiResponse<IChatResponse>>(this.apiUrl, request);
  }

  triggerLogoutReset(): void {
    this.logoutResetSource.next();
  }
}
