import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ISubCategory } from '../../models/master-data/sub-category/sub-category';
import { IApiResponse } from '../../models/api-response/api-response';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class SubCategoryService {
  
  private apiUrl = `${API_BASE_URL}/api/admin/sub-categories`;

  constructor(private http: HttpClient) { }

  getSubCategories(id: number) {
    return this.http.get<IApiResponse<ISubCategory[]>>(`${this.apiUrl}/category/${id}`);
  }

  addSubCategory(subcategory : ISubCategory) {
    return this.http.post<IApiResponse<string>>(`${this.apiUrl}/add`, subcategory);
  }

  deleteSubCategory(id: number) {
    return this.http.delete(`${this.apiUrl}/delete/${id}`);
  }
}
