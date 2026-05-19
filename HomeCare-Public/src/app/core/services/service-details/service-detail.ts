import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IService } from '../../models/service/IService';
import { IApiResponse } from '../../models/apiResponse/IApiResponse';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class ServiceDetailService {


  constructor(private http: HttpClient) { }

  getServicesBySubCategory(subCategoryId : number) {
    return this.http.get<IApiResponse<IService[]>>(
      `${API_BASE_URL}/api/services/get/subcategory/${subCategoryId}`
    );
  }

  getServiceById(serviceId: number): Observable<IApiResponse<IService>> {
    return this.http.get<IApiResponse<IService>>(
      `${API_BASE_URL}/api/services/get/${serviceId}`
    );
  }
}
