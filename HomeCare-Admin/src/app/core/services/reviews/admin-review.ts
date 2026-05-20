import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ApiResponse } from '../../models/profile/profile.model';
import { IAdminReviewPagedResult } from '../../models/reviews/IAdminReview';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({ providedIn: 'root' })
export class AdminReviewService {
  private readonly apiUrl = `${API_BASE_URL}/api/Review/all-reviews`;
  constructor(private http: HttpClient) {}

  getReviews(params: any): Observable<ApiResponse<IAdminReviewPagedResult>> {
    return this.http.get<ApiResponse<IAdminReviewPagedResult>>(this.apiUrl, { params });
  }
}
