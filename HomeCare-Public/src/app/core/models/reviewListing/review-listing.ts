export interface IServiceReviewSummary {
    averageRating: number;
    totalReviews: number;
    reviews: IServiceReviewItem[];
  }
  
  export interface IServiceReviewItem {
    customerName: string;
    customerProfileImage?: string;
    rating: number;
    reviewText?: string;
    createdAt: string;
  }