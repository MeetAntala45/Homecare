import { OfferStatus } from '../../enums/offers/offer-status';

export interface IOfferFilterRequest {
  pageNumber: number;
  pageSize: number;

  sortBy: string;
  sortOrder: string;

  couponCode?: string;
  minDiscount?: number | null;
  minUsage?: number | null;
  maxUsage?: number | null;

  status?: OfferStatus | null;
}

export interface FilterResult {
  discountPct: number | null;
  minUsage: number | null;
  maxUsage: number | null;
  activeOnly: boolean;
}