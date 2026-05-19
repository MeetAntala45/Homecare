import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { ApiResponse } from '../../../models/service-partner/service-partner';
import { Observable } from 'rxjs';
import { IAvailableCouponResponse } from '../../../models/checkout/available-coupon.response';
import { IApplyCouponRequest } from '../../../models/checkout/apply-coupon.request';
import { IApplyCouponResponse } from '../../../models/checkout/apply-coupon-response';
import { ICheckoutSummaryRequest } from '../../../models/checkout/checkout-summary.request';
import { ICheckoutSummaryResponse } from '../../../models/checkout/checkout-summary.response';
import { IAvailableCouponRequest } from '../../../models/checkout/available-coupon.request';
import { API_BASE_URL } from '../../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class CheckoutService {
  private baseUrl = `${API_BASE_URL}/api/checkout`;

  constructor(private http: HttpClient) { }

  getAvailableCoupons(
     req: IAvailableCouponRequest
  ): Observable<ApiResponse<IAvailableCouponResponse[]>> {
    return this.http.get<ApiResponse<IAvailableCouponResponse[]>>(
      `${this.baseUrl}/coupons`,
      {
        params: {
          serviceId : req.serviceId,
          slotDate: req.slotDate,
          slotStartTime: req.slotStartTime
        }
      }
    );
  }

  applyCoupon(
    req: IApplyCouponRequest
  ): Observable<ApiResponse<IApplyCouponResponse>> {

    return this.http.post<ApiResponse<IApplyCouponResponse>>(
      `${this.baseUrl}/apply-coupon`,
      req
    );
  }

  getSummary(
    req: ICheckoutSummaryRequest
  ): Observable<ApiResponse<ICheckoutSummaryResponse>> {

    return this.http.post<ApiResponse<ICheckoutSummaryResponse>>(
      `${this.baseUrl}/summary`,
      req
    );
  }
}
