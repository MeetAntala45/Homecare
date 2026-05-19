import { Component, EventEmitter, Input, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Toaster } from '../../../core/services/toaster/toaster';
import { ReviewService } from '../../../core/services/my-bookings/review-service';
import { ICreateReview } from '../../../core/models/my-bookings/IReview';

@Component({
  selector: 'app-review-modal',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './review-modal.html',
  styleUrl: './review-modal.css'
})
export class ReviewModalComponent implements OnInit {
  @Input() bookingId!: number;
  @Input() serviceName!: string;
  @Output() closed = new EventEmitter<void>();
  @Output() submitted = new EventEmitter<number>();

  stars = [1, 2, 3, 4, 5];
  selectedRating = 0;
  hoveredRating = 0;
  reviewText = '';
  isSubmitting = false;

  readonly ratingLabels: Record<number, string> = {
    1: 'Poor',
    2: 'Fair',
    3: 'Good',
    4: 'Very Good',
    5: 'Excellent'
  };

  constructor(
    private reviewService: ReviewService,
    private toaster: Toaster
  ) {}

  ngOnInit(): void {
    document.body.style.overflow = 'hidden';
  }

  get activeRating(): number {
    return this.hoveredRating || this.selectedRating;
  }

  get ratingLabel(): string {
    return this.activeRating ? this.ratingLabels[this.activeRating] : 'Select a rating';
  }

  setRating(star: number): void {
    this.selectedRating = star;
  }

  hoverRating(star: number): void {
    this.hoveredRating = star;
  }

  clearHover(): void {
    this.hoveredRating = 0;
  }

  close(): void {
    document.body.style.overflow = '';
    this.closed.emit();
  }

  submit(): void {
    if (!this.selectedRating) {
      this.toaster.error('Please select a rating before submitting.');
      return;
    }

    const dto: ICreateReview = {
      bookingId: this.bookingId,
      rating: this.selectedRating,
      reviewText: this.reviewText.trim() || undefined
    };

    this.isSubmitting = true;

    this.reviewService.createReview(dto).subscribe({
      next: (res) => {
        if (res.success) {
          this.toaster.success('Review submitted successfully!');
          this.submitted.emit(this.bookingId);
          this.close();
        } else {
          this.toaster.error(res.message);
        }
        this.isSubmitting = false;
      },
      error: (err) => {
        this.toaster.error(err?.error?.message || 'Failed to submit review.');
        this.isSubmitting = false;
      }
    });
  }
}