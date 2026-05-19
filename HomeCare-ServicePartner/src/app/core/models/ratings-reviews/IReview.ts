export interface IPartnerReviewSummary {
    averageRating: number;
    totalReviews: number;
    ratingBreakdown: number[]; 
    reviews: IPartnerReviewItem[];
}
  
export interface IPartnerReviewItem {
    customerName: string;
    customerProfileImage?: string;
    serviceName: string;
    rating: number;
    reviewText?: string;
    createdAt: string;
}