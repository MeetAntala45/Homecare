import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { Observable, Observer } from 'rxjs';
import { ISupportResponse } from '../../models/support/support-response';
import { IApiResponse } from '../../models/api-response/api-response';
import { PagedResult } from '../../models/paged-result';
import { API_BASE_URL } from '../../constants/environment-config';


@Injectable({
  providedIn: 'root',
})
export class SupportService {
  private readonly base = `${API_BASE_URL}/api/support`;

  constructor(private http: HttpClient) { }

  getAll(params: any): Observable<IApiResponse<PagedResult<ISupportResponse>>> {
    return this.http.get<IApiResponse<PagedResult<ISupportResponse>>>(`${this.base}/all`,{params});
  }
}
