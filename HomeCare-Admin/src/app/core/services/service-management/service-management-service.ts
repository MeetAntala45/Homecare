import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';

import { ICategory, IService, IServiceType, ISubCategory } from '../../models/service-management/service';
import { ApiResponse } from '../../models/profile/profile.model';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root'
})
export class ServiceManagementService {

  private readonly base = `${API_BASE_URL}/api`;

  constructor(private http: HttpClient) {}

  getServiceTypes(): Observable<ApiResponse<IServiceType[]>> {
    return this.http.get<ApiResponse<IServiceType[]>>(
      `${this.base}/admin/service-types/all`
    );
  }

  getCategoriesByServiceType(serviceTypeId: number): Observable<ApiResponse<ICategory[]>> {
    return this.http.get<ApiResponse<ICategory[]>>(
      `${this.base}/admin/categories/service-type/${serviceTypeId}`
    );
  }

  getServicesByCategory(categoryId: number, params?: HttpParams) {
    return this.http.get<ApiResponse<IService[]>>(
      `${this.base}/services/get/categories/${categoryId}`,
      { params }
    );
  }

  getServiceById(serviceId: number): Observable<ApiResponse<IService>> {
    return this.http.get<ApiResponse<IService>>(
      `${this.base}/services/get/${serviceId}`
    );
  }

  upsertService(data: FormData): Observable<ApiResponse<IService>> {
    return this.http.post<ApiResponse<IService>>(
      `${this.base}/services/upsert`,
      data
    );
  }

  toggleAvailability(serviceId: number, isAvailable: boolean): Observable<ApiResponse<boolean>> {
    return this.http.patch<ApiResponse<boolean>>(
      `${this.base}/services/edit/${serviceId}/availability`,
      { isAvailable }
    );
  }

  deleteService(serviceId: number): Observable<ApiResponse<boolean>> {
    return this.http.delete<ApiResponse<boolean>>(
      `${this.base}/services/delete/${serviceId}`
    );
  }

  getSubCategoriesByCategory(categoryId: number): Observable<ApiResponse<ISubCategory[]>> {
  return this.http.get<ApiResponse<ISubCategory[]>>(
    `${this.base}/admin/sub-categories/category/${categoryId}`
  );
}
}