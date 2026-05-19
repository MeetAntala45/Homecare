import { Status } from '../../enums/status';

export interface Offer {
  id: number;
  couponCode: string;
  couponDescription: string;
  couponDiscount: number;
  couponApplied: number;
  status: Status;
  createdAt?: Date;
}

export interface OfferForm {
  couponCode: string;
  couponDescription: string;
  couponDiscount: number | null;
  status?: Status;
}

export interface PaginatedResponse<T> {
  data: T[];
  total: number;
  page: number;
  rowsPerPage: number;
}