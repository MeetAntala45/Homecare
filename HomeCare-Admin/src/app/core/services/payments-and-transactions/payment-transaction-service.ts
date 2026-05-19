import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ApiResponse } from '../../models/profile/profile.model';
import { PagedResult } from '../../models/paged-result';
import { IPaymentList } from '../../models/payments-and-transations/IPaymentList';
import { IPaymentPagedResult } from '../../models/payments-and-transations/IPaymentPagedResult';
import { Observable } from 'rxjs';
import { IUserPaymentDetail } from '../../models/payments-and-transations/IUserPaymentDetail';
import { IUserPaymentList } from '../../models/payments-and-transations/IUserPaymentList';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class PaymentTransactionService {
  
  api_url = `${API_BASE_URL}/api/admin/payment`;

  constructor (private http: HttpClient) {}

  getPayments(filter: any){
    return this.http.get<ApiResponse<IPaymentPagedResult<IPaymentList>>>(
      `${this.api_url}/get`, 
      {params: filter}
    )
  }

  getUserPaymentDetail(id: number): Observable<ApiResponse<IUserPaymentDetail>> {
    return this.http.get<ApiResponse<IUserPaymentDetail>>(
      `${this.api_url}/${id}`
    );
  }

  getUserPayments(params: any) {
    return this.http.get<
      ApiResponse<IPaymentPagedResult<IUserPaymentList>>
    >(`${this.api_url}/user-payments`, { params });
  }

}
