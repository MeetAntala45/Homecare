import { Injectable } from '@angular/core';
import { IApiResponse } from '../../models/api-response/api-response';
import { IPartnerReviewSummary } from '../../models/ratings-reviews/IReview';
import { Observable } from 'rxjs';
import { HttpClient } from '@angular/common/http';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class ReviewService {

  baseUrl = `${API_BASE_URL}/api/review`
  
  constructor(private http: HttpClient){}
  getMyReviews(): Observable<IApiResponse<IPartnerReviewSummary>> {
    return this.http.get<IApiResponse<IPartnerReviewSummary>>(`${this.baseUrl}/my-reviews`);
  }
}
