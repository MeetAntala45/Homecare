import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { IApiResponse } from '../../../models/apiResponse/IApiResponse';
import { IServiceType } from '../../../models/serviceType/IServiceType';
import { IService } from '../../../models/service/IService';
import { API_BASE_URL } from '../../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class Service {
  constructor(private http: HttpClient) { }

  getAllServices(): Observable<IApiResponse<IService[]>> {
    return this.http.get<IApiResponse<IService[]>>(`${API_BASE_URL}/api/services/all`);
  }
}
