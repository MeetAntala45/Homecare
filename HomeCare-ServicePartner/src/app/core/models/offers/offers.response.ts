import { OfferStatus } from '../../enums/offers/offer-status';
import { IConditionResponse } from './coupon-condition.response';

export interface IOfferResponse {
  id: number;
  offerCode: string;
  description?: string;
  discountPct?: string;
  discount: string;
  timesApplied: number;
  status: OfferStatus;
  conditions?: IConditionResponse[]
}