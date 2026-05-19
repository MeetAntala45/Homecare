export interface IMyJobs {
    bookingId: number;
    serviceName: string;
    customerName: string;
    address: string;
    bookingDate: string;
    slotTime: string;
    amount: number;
    bookingStatus: string;
    paymentMethod: string;
}

export interface IMyJobRequest {
    pageNumber: number;
    pageSize: number;
    sortBy?: string;
    sortOrder?: string;
    status: string;
    serviceName: '';
    customerName: '';
    bookingDate: '';
    paymentMethod: '';
    minAmount: '';
    maxAmount: '';
}

export interface IMyJobCalendarRequest {
    year: number;
    month: number;
}

export interface IMyJobCalendarItem {
    bookingId: number;
    serviceName: string;
    customerName: string;
    bookingDate: string;
    slotTime: string;
    bookingStatus: string;
}