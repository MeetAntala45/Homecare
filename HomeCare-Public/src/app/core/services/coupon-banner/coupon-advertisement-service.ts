import { Injectable } from '@angular/core';
import { IApiResponse } from '../../models/apiResponse/IApiResponse';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../../constants/environment-config';

export interface ICouponAdvertisement {
  couponCode: string;
  description: string | null;
  discountPct: number;
  conditions: { label: string; summary: string }[];
}

@Injectable({
  providedIn: 'root',
})
export class CouponAdvertisementService {
  private readonly apiUrl = `${API_BASE_URL}/api/coupon-advertisement`;

  constructor(private http: HttpClient) { }

  getBestAdvertisement(): Observable<IApiResponse<ICouponAdvertisement | null>> {
    return this.http.get<IApiResponse<ICouponAdvertisement | null>>(this.apiUrl);
  }
}