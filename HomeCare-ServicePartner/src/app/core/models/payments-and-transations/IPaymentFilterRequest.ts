export interface IPaymentFilterRequest {
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortOrder: string;
    userName?: string;
    minAmount?: number | null;
    maxAmount?: number | null;
    paymentMethod?: string;
  }