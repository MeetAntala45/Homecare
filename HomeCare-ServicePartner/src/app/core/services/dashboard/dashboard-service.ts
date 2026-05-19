import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IPartnerDashboardMetricsResponse } from '../../models/dashboard/IPartnerDashboardMetricsResponse';
import { IApiResponse } from '../../models/api-response/api-response';
import { IGetPartnerChartRequest } from '../../models/dashboard/IGetPartnerChartRequest';
import { IPartnerRevenueChartResponse } from '../../models/dashboard/IPartnerRevenueChartResponse';
import { IPartnerTopService } from '../../models/dashboard/IPartnerTopService';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class PartnerDashboardService {
  private readonly base = `${API_BASE_URL}/api/partner/dashboard`;

  constructor(private http: HttpClient) {}

  getAllMetricCards(): Observable<IApiResponse<IPartnerDashboardMetricsResponse>> {
    return this.http.get<IApiResponse<IPartnerDashboardMetricsResponse>>(`${this.base}/metrics`);
  }

  getRevenueChart(
    req: IGetPartnerChartRequest
  ): Observable<IApiResponse<IPartnerRevenueChartResponse>> {
    let params = `period=${req.period}`;
    if (req.week) params += `&week=${req.week}`;
    return this.http.get<IApiResponse<IPartnerRevenueChartResponse>>(
      `${this.base}/revenue-chart?${params}`
    );
  }

  getTopServices(req: IGetPartnerChartRequest): Observable<IApiResponse<IPartnerTopService[]>> {
    let params = `period=${req.period}`;
    if (req.week) params += `&week=${req.week}`;
    return this.http.get<IApiResponse<IPartnerTopService[]>>(`${this.base}/top-services?${params}`);
  }
}
