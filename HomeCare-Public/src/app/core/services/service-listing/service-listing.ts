import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { IApiResponse } from '../../models/apiResponse/IApiResponse';
import { ICategory } from '../../models/service-listing/ICategory';
import { ISubCategory } from '../../models/service-listing/ISubCategory';
import { IService } from '../../models/service/IService';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class ServiceListing {
  private apiUrl = `${API_BASE_URL}/api`;

  constructor(private http: HttpClient) { }

  getCategoryById(id: number) {
    return this.http.get<IApiResponse<ICategory[]>>(`${this.apiUrl}/admin/categories/service-type/${id}`);
  }

  getSubCategories(id: number) {
    return this.http.get<IApiResponse<ISubCategory[]>>(`${this.apiUrl}/admin/sub-categories/category/${id}`);
  }

  getSearchService(id: number, search?: string) {
    const params: Record<string, string> = {};
    if (search?.trim()) {
      params['search'] = search.trim();
    }
    return this.http.get<IApiResponse<IService[]>>(`${this.apiUrl}/services/get/service-type/${id}`, { params });
  }

}
