import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../models/profile/profile.model';
import { IService } from '../../models/service-management/service';
import { IServiceTypeHierarchy } from '../../models/my-service/my-service';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class PartnerMyService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${API_BASE_URL}/api`;

  getPartnerServiceHierarchy(): Observable<ApiResponse<IServiceTypeHierarchy[]>> {
    return this.http.get<ApiResponse<IServiceTypeHierarchy[]>>(
      `${this.apiUrl}/service-partner/service-hierarchy`
    );
  }

  getServiceById(serviceId: number): Observable<ApiResponse<IService>> {
    return this.http.get<ApiResponse<IService>>(
      `${this.apiUrl}/services/get/${serviceId}`
    );
  }

  removeSkillService(subCategoryId: number) {
    return this.http.delete<ApiResponse<string>>(
      `${this.apiUrl}/service-partner/remove-skill-service/${subCategoryId}`
    );
  }
}
