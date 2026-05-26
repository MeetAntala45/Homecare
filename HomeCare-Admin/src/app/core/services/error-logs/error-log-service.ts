import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  IErrorLogDetail,
  IErrorLogFilter,
  IErrorLogPagedResult,
} from '../../models/error-logs/IErrorLog';
import { API_BASE_URL } from '../../constants/environment-config';
import { ApiResponse } from '../../models/profile/profile.model';

@Injectable({ providedIn: 'root' })
export class ErrorLogService {
  private readonly base = `${API_BASE_URL}/api/admin/error-logs`;

  constructor(private http: HttpClient) {}

  getLogs(params: any): Observable<ApiResponse<IErrorLogPagedResult>> {
    return this.http.get<ApiResponse<IErrorLogPagedResult>>(this.base, { params });
  }

  getById(id: number): Observable<ApiResponse<IErrorLogDetail>> {
    return this.http.get<ApiResponse<IErrorLogDetail>>(`${this.base}/${id}`);
  }
}
