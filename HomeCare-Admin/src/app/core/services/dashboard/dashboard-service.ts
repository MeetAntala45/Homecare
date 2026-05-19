import { Injectable } from '@angular/core';
import { IDashboardMetricsResponse } from '../../models/dashboard/IDashboardMetricsResponse';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IApiResponse } from '../../models/auth/api.response';
import { IRevenueChartResponse } from '../../models/dashboard/IRevenueChartResponse';
import { ITopServices } from '../../models/dashboard/ITopService';
import { ITopServicePartnerResponse } from '../../models/dashboard/ITopServicePartnerResponse';
import { ITopCityResponseDto } from '../../models/dashboard/ITopCityResponseDto';
import { IGetChartRequest } from '../../models/dashboard/IGetChartRequest';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class DashboardService {
  private readonly base = `${API_BASE_URL}/api/dashboard`;

  constructor(private http: HttpClient) { }

  getAllMetricCards(): Observable<IApiResponse<IDashboardMetricsResponse>> {
    return this.http.get<IApiResponse<IDashboardMetricsResponse>>(`${this.base}/metrics`);
  }

  getRevenueChart(req: IGetChartRequest): Observable<IApiResponse<IRevenueChartResponse>> {
    let params = `period=${req.period}`;
    if (req.week) params += `&week=${req.week}`;

    return this.http.get<IApiResponse<IRevenueChartResponse>>(`${this.base}/revenue-chart?${params}`);
  }

  getTopServices(req: IGetChartRequest): Observable<IApiResponse<ITopServices[]>> {
    let params = `period=${req.period}`;
    if (req.week) params += `&week=${req.week}`;
    return this.http.get<IApiResponse<ITopServices[]>>(`${this.base}/top-services?${params}`)
  }

  getTopCities(req: IGetChartRequest): Observable<IApiResponse<ITopCityResponseDto[]>> {
    let params = `period=${req.period}`;
    if (req.week) params += `&week=${req.week}`;
    return this.http.get<IApiResponse<ITopCityResponseDto[]>>(`${this.base}/top-cities?${params}`);
  }

  getTopServicePartners(): Observable<IApiResponse<ITopServicePartnerResponse[]>> {
    return this.http.get<IApiResponse<ITopServicePartnerResponse[]>>(`${this.base}/top-service-partners`);
  }
}