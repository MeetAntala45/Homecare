// src/app/core/services/referral/referral.service.ts
import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IApiResponse } from '../../models/apiResponse/IApiResponse';
import { IWallet, IReferralInfo } from '../../models/referral/IReferral';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({ providedIn: 'root' })
export class ReferralService {
  private readonly base = `${API_BASE_URL}/api/referral`;

  constructor(private http: HttpClient) {}

  getWallet(): Observable<IApiResponse<IWallet>> {
    return this.http.get<IApiResponse<IWallet>>(`${this.base}/wallet`);
  }

  getReferralInfo(): Observable<IApiResponse<IReferralInfo>> {
    return this.http.get<IApiResponse<IReferralInfo>>(`${this.base}/info`);
  }
}
