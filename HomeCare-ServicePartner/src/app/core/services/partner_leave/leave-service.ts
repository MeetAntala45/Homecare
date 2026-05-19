import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  ApiResponse, PagedResult,
  ApplyLeaveRequest, LeaveResponse,
  AdminLeaveItem, ReviewLeaveDto, LeaveFilter
} from '../../../core/models/servicepartner_leave/leave.js';
import { API_BASE_URL } from '../../constants/environment-config.js';

@Injectable({ providedIn: 'root' })
export class LeaveService {

  private readonly base = `${API_BASE_URL}/api/partner/leaves`;
  private readonly adminBase = `${API_BASE_URL}/api/admin/leaves`;

  constructor(private http: HttpClient) { }


  applyLeave(dto: ApplyLeaveRequest): Observable<ApiResponse<LeaveResponse>> {
    return this.http.post<ApiResponse<LeaveResponse>>(`${this.base}`, dto);
  }

  getMyLeaves(filter: LeaveFilter): Observable<ApiResponse<PagedResult<LeaveResponse>>> {
    let params = new HttpParams()
      .set('pageNumber', filter.pageNumber ?? 1)
      .set('pageSize', filter.pageSize ?? 10);

    if (filter.statusId != null)
      params = params.set('statusId', filter.statusId);

    return this.http.get<ApiResponse<PagedResult<LeaveResponse>>>(this.base, { params });
  }

  cancelLeave(id: number): Observable<ApiResponse<string>> {
    return this.http.delete<ApiResponse<string>>(`${this.base}/${id}`);
  }



  reviewLeave(id: number, dto: ReviewLeaveDto): Observable<ApiResponse<string>> {
    return this.http.patch<ApiResponse<string>>(`${this.adminBase}/${id}/review`, dto);
  }
}