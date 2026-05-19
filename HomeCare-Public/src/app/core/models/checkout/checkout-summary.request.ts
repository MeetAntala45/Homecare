export interface ICheckoutSummaryRequest {
    serviceId: number;
    slotDate: string | null;
    slotStartTime: string | null;
    couponCode?: string | null;
  }