export interface ICheckoutSummaryResponse {
    serviceName: string;
    servicePrice: number;
    taxPct: number;
    taxAmount: number;
    discountAmount: number;
    appliedCouponCode: string | null;
    totalAmount: number;
  }