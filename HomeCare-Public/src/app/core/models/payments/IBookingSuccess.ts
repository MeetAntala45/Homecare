export interface IBookingSuccess {
    serviceId: number;
    failureReason?: 'expired' | 'refunded' | null;
    bookingId: number;
    serviceName: string;
    serviceCategory: string;
    durationMinutes: number;
    amountPaid: number;
    location: string;
    slotDate: string;
    slotStartTime: string;
    partnerAssigned: boolean;
    partnerName?: string;
    partnerImage?: string;
    partnerMobileNumber: string;
}
