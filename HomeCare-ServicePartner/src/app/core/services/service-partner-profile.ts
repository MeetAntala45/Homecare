import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { PartnerProfileApiResponse } from '../models/service-partner/service-partner-profile';
import { API_BASE_URL } from '../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class ServicePartnerProfile {
  private readonly apiUrl = `${API_BASE_URL}/api/service-partner`;
  constructor(private http: HttpClient) {}

  getProfile(): Observable<PartnerProfileApiResponse> {
    return this.http.get<PartnerProfileApiResponse>(
      `${this.apiUrl}/profile`
    );
  }

  updateProfile(form: FormData): Observable<PartnerProfileApiResponse> {
    return this.http.put<PartnerProfileApiResponse>(
      `${this.apiUrl}/profile`,
      form
    );
  }
}
