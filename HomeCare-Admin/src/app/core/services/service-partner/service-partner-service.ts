import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { IServicePartnerListResponse } from '../../models/service-partner/service-partner.response';
import { IServicePartnerFilterRequest } from '../../models/service-partner/service-partner.filter';
import { ApiResponse } from '../../models/profile/profile.model';
import { IServicePartnerDetail } from '../../models/service-partner/serivce-partner-detail';
import { IDropdownOption } from '../../models/service-partner/dropdown-option';
import { IApiResponse } from '../../models/auth/api.response';
import { PagedResult } from '../../models/paged-result';
import { IPartnerAssignedService } from '../../models/service-partner/partner-assigned-service';
import { IFilterPagedResult } from '../../models/shared-components/IFilterPagedResult';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class ServicePartnerService {
  private http = inject(HttpClient);
  private readonly apiUrl = `${API_BASE_URL}/api/service-partner`;

  getAll(params: HttpParams): Observable<IApiResponse<IFilterPagedResult<IServicePartnerListResponse>>> {
    return this.http.get<IApiResponse<IFilterPagedResult<IServicePartnerListResponse>>>(
      `${this.apiUrl}/getAll`,
      { params }
    );
  }

  getById(id: number): Observable<ApiResponse<IServicePartnerDetail>> {
    return this.http.get<ApiResponse<IServicePartnerDetail>>(`${this.apiUrl}/${id}`);
  }

  updateStatus(id: number): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${id}/status`, {});
  }

  approve(id: number): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${id}/approve`, {});
  }

  reject(id: number): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(`${this.apiUrl}/${id}/reject`, {});
  }

  delete(id: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(`${this.apiUrl}/${id}/delete`);
  }
  getServiceTypes(): Observable<ApiResponse<IDropdownOption[]>> {
    return this.http.get<ApiResponse<IDropdownOption[]>>(`${this.apiUrl}/service-types`);
  }

  getCategoriesByServiceType(serviceTypeId: string): Observable<ApiResponse<IDropdownOption[]>> {
    return this.http.get<ApiResponse<IDropdownOption[]>>(
      `${this.apiUrl}/categories/${serviceTypeId}`
    );
  }

  getSubCategoriesByCategory(categoryId: string): Observable<ApiResponse<IDropdownOption[]>> {
    return this.http.get<ApiResponse<IDropdownOption[]>>(
      `${this.apiUrl}/sub-categories/${categoryId}`
    );
  }
  getAssignedServices(
    partnerId: number,
    params: Record<string, any>
  ): Observable<IApiResponse<PagedResult<IPartnerAssignedService>>> {
    let httpParams = new HttpParams();
    for (const key of Object.keys(params)) {
      if (params[key] !== null && params[key] !== undefined && params[key] !== '') {
        httpParams = httpParams.set(key, String(params[key]));
      }
    }
    return this.http.get<IApiResponse<PagedResult<IPartnerAssignedService>>>(
      `${this.apiUrl}/${partnerId}/assigned-services`,
      { params: httpParams }
    );
  }
   
}
