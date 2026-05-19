import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReviewService } from '../../../core/services/ratings-reviews/review-service';
import { IPartnerReviewSummary } from '../../../core/models/ratings-reviews/IReview';
import { API_BASE_URL } from '../../../core/constants/environment-config';

@Component({
  selector: 'app-ratings-reviews',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './ratings-reviews.html',
  styleUrl: './ratings-reviews.css'
})
export class RatingsReviews implements OnInit {
  summary: IPartnerReviewSummary | null = null;
  isLoading = true;
  readonly BASE_URL = API_BASE_URL;
  readonly stars = [5, 4, 3, 2, 1];

  constructor(private reviewService: ReviewService) { }

  ngOnInit(): void {
    setTimeout(() => {
      this.reviewService.getMyReviews().subscribe({
        next: (res) => {
          this.summary = res.data ?? null;
          this.isLoading = false;
        },
        error: () => { this.isLoading = false; }
      });
    }, 600)
  }

  getBarWidth(star: number): string {
    if (!this.summary?.totalReviews) return '0%';
    const count = this.summary.ratingBreakdown[star - 1];
    return `${Math.round((count / this.summary.totalReviews) * 100)}%`;
  }

  getBarCount(star: number): number {
    return this.summary?.ratingBreakdown[star - 1] ?? 0;
  }

  getRatingColor(avg: number): string {
    if (avg >= 4) return '#16a34a';
    if (avg >= 3) return '#d97706';
    return '#dc2626';
  }

  getRatingLabel(avg: number): string {
    if (avg >= 4.5) return 'Excellent';
    if (avg >= 4) return 'Very Good';
    if (avg >= 3) return 'Good';
    if (avg >= 2) return 'Fair';
    return 'Poor';
  }

  getInitial(name: string): string {
    return name?.charAt(0).toUpperCase() ?? '?';
  }

  getStars(rating: number): boolean[] {
    return Array.from({ length: 5 }, (_, i) => i < rating);
  }

  getTimeAgo(dateStr: string): string {
    const diff = Date.now() - new Date(dateStr).getTime();
    const days = Math.floor(diff / 86400000);
    if (days === 0) return 'Today';
    if (days === 1) return 'Yesterday';
    if (days < 30) return `${days}d ago`;
    if (days < 365) return `${Math.floor(days / 30)}mo ago`;
    return `${Math.floor(days / 365)}y ago`;
  }
}