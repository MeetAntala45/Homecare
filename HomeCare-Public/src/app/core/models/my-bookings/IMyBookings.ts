export interface IMyBooking {
    id: number;
    customerId?: number;
    profileImage?: string;
    serviceName: string;
    partnerName?: string;
    mobileNumber: string;
    duration: number;
    houseFlatNo: string;
    landMark: string;
    cancellationReason?: string;
    address: string;
    bookingStatus: string;
    price: number;
    bookingDate: Date;
    slotStartTime: String
    date?: string;
    month?: string;
    hasReview?: boolean;
    latitude?: number;
    longitude?: number;
}