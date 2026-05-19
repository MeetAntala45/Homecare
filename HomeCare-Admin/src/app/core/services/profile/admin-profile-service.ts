import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { AdminProfile, UpdateContactPayload, ChangePasswordPayload, ApiResponse } from '../../models/profile/profile.model';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({ providedIn: 'root' })

export class AdminProfileService {

  private baseUrl = `${API_BASE_URL}/api/admin/profile`;

  constructor(private http: HttpClient) { }

  getProfile(): Observable<ApiResponse<AdminProfile>> {
    return this.http.get<ApiResponse<AdminProfile>>(this.baseUrl);
  }

  updateContactInfo(data: UpdateContactPayload): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(`${this.baseUrl}/contact`, data);
  }

  changePassword(data: ChangePasswordPayload): Observable<ApiResponse<string>> {
    return this.http.put<ApiResponse<string>>(`${this.baseUrl}/change-password`, data);
  }

  uploadProfilePhoto(file: File): Observable<ApiResponse<string>> {
    const formData = new FormData();
    formData.append('photo', file);
    return this.http.post<ApiResponse<string>>(`${this.baseUrl}/photo`, formData);
  }
}