export interface IUserPaymentFilterRequest {
    userId: number;
    currentPaymentId: number;
    pageNumber: number;
    pageSize: number;
    sortBy: string;
    sortOrder: string;
    minAmount: number | null;
    maxAmount: number | null;
    paymentMethod?: string;
  }