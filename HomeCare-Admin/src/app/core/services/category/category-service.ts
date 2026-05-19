import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ICategory } from '../../models/master-data/category/category';
import { IApiResponse } from '../../models/api-response/api-response';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class CategoryService {
  private apiUrl = `${API_BASE_URL}/api/admin/categories`;

  constructor(private http: HttpClient) { }

  getCategoryById(id: number) {
    return this.http.get<IApiResponse<ICategory[]>>(`${this.apiUrl}/service-type/${id}`);
  }

  addCategory(category : ICategory) {
    return this.http.post<IApiResponse<string>>(`${this.apiUrl}/add`, category);
  }

  deleteCategory(id: number) {
    return this.http.delete(`${this.apiUrl}/delete/${id}`);
  }
}
