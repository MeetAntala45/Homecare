export interface IAvailableCouponResponse {
    id: number;
    couponCode: string;
    description: string;
    discount: string;
    discountPct: number;
    isEligible: boolean;
    ineligibleReason: string | null;
  }