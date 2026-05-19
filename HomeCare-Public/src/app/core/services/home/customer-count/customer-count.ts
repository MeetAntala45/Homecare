import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { ICustomerFilterRequest } from '../../../models/homepage/ICustomerFilterRequest';
import { ApiResponse } from '../../../models/service-partner/service-partner';
import { PagedResult } from '../../../models/homepage/IPageResults';
import { ICustomerList } from '../../../models/homepage/ICustomerList';
import { API_BASE_URL } from '../../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class CustomerCount {
  api_url = `${API_BASE_URL}/api/customer`

  constructor(private http: HttpClient) { }

  getCustomerList(filter: ICustomerFilterRequest) {
    return this.http.get<ApiResponse<PagedResult<ICustomerList>>>(`${this.api_url}/customer-list`,
      { params: filter as any }
    );
  }
}
