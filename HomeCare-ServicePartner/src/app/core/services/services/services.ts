import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IApiResponse } from '../../models/api-response/api-response';
import { IService } from '../../models/service/IService';
import { Observable } from 'rxjs';
import { ICategory, ISubCategory } from '../../models/service-management/service';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class Services {
  private apiUrl = `${API_BASE_URL}/api`;

  constructor(private http: HttpClient) { }

  getSubCategories(id: number) {
    return this.http.get<IApiResponse<ISubCategory[]>>(`${this.apiUrl}/admin/sub-categories/category/${id}`);
  }

  getCategoryById(id: number) {
    return this.http.get<IApiResponse<ICategory[]>>(`${this.apiUrl}/admin/categories/service-type/${id}`);
  }

  getServicesByPartnerId(partnerId: number): Observable<IApiResponse<IService[]>> {
    return this.http.get<IApiResponse<IService[]>>(
      `${this.apiUrl}/service-partner/services/${partnerId}`
    );
  }

  getCustomersByServiceId(serviceId: number): Observable<IApiResponse<any[]>> {
    return this.http.get<IApiResponse<IService[]>>(
      `${this.apiUrl}/service-partner/customers/service/${serviceId}`
    );
  }

  getServicesBySubCategoryId(subCategoryId: number) {
    return this.http.get<IApiResponse<IService[]>>(
      `${this.apiUrl}/services/get/subcategory/${subCategoryId}`
    );
  }

  addSkillService(payload: { categoryIds: number[]; subCategoryIds: number[] }) {
    return this.http.post<IApiResponse<string>>(
      `${this.apiUrl}/service-partner/add-skill-service`,
      payload
    );
  }
}