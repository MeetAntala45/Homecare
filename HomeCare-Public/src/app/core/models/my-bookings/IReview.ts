export interface ICreateReview {
    bookingId: number;
    rating: number;
    reviewText?: string;
}

export interface IReviewResponse {
    id: number;
    bookingId: number;
    rating: number;
    reviewText?: string;
    createdAt: string;
}