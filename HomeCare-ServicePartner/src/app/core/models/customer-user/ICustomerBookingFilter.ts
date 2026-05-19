export interface ICustomerBookingFilter {
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortOrder: string;
    serviceTypeId?: string;
    date?: string | null;
    time?: string | null;
    minAmount?: number | null;
    maxAmount?: number | null;
    paymentMethod?: string;
    status?: string;
  }