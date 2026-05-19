import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { IApiResponse } from '../../../models/apiResponse/IApiResponse';
import { IServiceType } from '../../../models/serviceType/IServiceType';
import { Observable } from 'rxjs';
import { API_BASE_URL } from '../../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class ServiceType {
  constructor(private http: HttpClient) { }

  getAll(): Observable<IApiResponse<IServiceType[]>> {
    return this.http.get<IApiResponse<IServiceType[]>>(`${API_BASE_URL}/api/admin/service-types/all`);
  }

  getBookingCountByServiceType(serviceId : number): Observable<IApiResponse<number>>{
    return this.http.get<IApiResponse<number>>(`${API_BASE_URL}/api/booking/counts/serviceType/${serviceId}`)
  }
}
  