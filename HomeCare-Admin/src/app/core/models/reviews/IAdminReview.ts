export interface IAdminReview {
  id: number;
  bookingId: number;
  customerName: string;
  partnerName: string;
  serviceName: string;
  rating: number;
  reviewText: string | null;
  createdAt: string;
}

export interface IAdminReviewPagedResult {
  data: IAdminReview[];
  totalCount: number;
}

export interface IAdminReviewFilterRequest {
  pageNumber: number;
  pageSize: number;
  sortBy: string;
  sortOrder: string;
  customerName?: string;
  partnerName?: string;
  serviceName?: string;
  minRating?: number | null;
  maxRating?: number | null;
}
