import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IApiResponse } from '../../models/apiResponse/IApiResponse';
import { ICreateReview, IReviewResponse } from '../../models/my-bookings/IReview';
import { IServiceReviewSummary } from '../../models/reviewListing/review-listing';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({ providedIn: 'root' })
export class ReviewService {
  private readonly baseUrl = `${API_BASE_URL}/api/review`;

  constructor(private http: HttpClient) {}

  createReview(dto: ICreateReview): Observable<IApiResponse<IReviewResponse>> {
    return this.http.post<IApiResponse<IReviewResponse>>(this.baseUrl, dto);
  }

  getReviewByBookingId(bookingId: number): Observable<IApiResponse<IReviewResponse>> {
    return this.http.get<IApiResponse<IReviewResponse>>(`${this.baseUrl}/booking/${bookingId}`);
  }

  getServiceReviews(serviceId: number): Observable<IApiResponse<IServiceReviewSummary>> {
    return this.http.get<IApiResponse<IServiceReviewSummary>>(
      `${this.baseUrl}/service/${serviceId}`
    );
  }
}
