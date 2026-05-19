import { OfferStatus } from '../../enums/offers/offer-status';

export interface IUpdateOfferRequest {
  id: number;
  offerCode: string;
  description: string;
  discountPct: number;
  status: OfferStatus;
}