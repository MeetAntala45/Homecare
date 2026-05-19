import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse, IDropdownOption, IServicePartnerResponse } from '../../models/service-partner/service-partner';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class PartnerService {

  private readonly apiUrl = `${API_BASE_URL}/api/service-partner`;

  constructor(private http: HttpClient) {}

  submitApplication(form: FormData): Observable<IServicePartnerResponse> {
    return this.http.post<IServicePartnerResponse>(`${this.apiUrl}/apply`, form);
  }

  getServiceTypes(): Observable<ApiResponse<IDropdownOption[]>> {
    return this.http.get<ApiResponse<IDropdownOption[]>>(`${this.apiUrl}/service-types`);
  }

  getCategoriesByServiceType(serviceTypeId: string): Observable<ApiResponse<IDropdownOption[]>> {
    return this.http.get<ApiResponse<IDropdownOption[]>>(`${this.apiUrl}/categories/${serviceTypeId}`);
  }

  getSubCategoriesByCategory(categoryId: string): Observable<ApiResponse<IDropdownOption[]>> {
    return this.http.get<ApiResponse<IDropdownOption[]>>(`${this.apiUrl}/sub-categories/${categoryId}`);
  }
}