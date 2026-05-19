import { IActionItem } from '../shared-components/IActionDropDownModel';

export interface ICustomerBookingDetail {
  bookingId: number;
  serviceId: number;
  serviceName: string;
  serviceType: string;
  dateTime: string;
  expertName: string | null;
  expertPhoto: string | null;
  bookingStatus: string;
  bookingStatusValue: number;
  amount: number;
  partnerId: number | null;
  isPartnerDeleted: boolean;
  partnerPhone: string | null;
}

export interface ICustomerBookingDetailPaged {
  items: ICustomerBookingDetail[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
}

export interface IBookingDetailRow extends ICustomerBookingDetail {
  assignedExpert: string;
  expertImage: string | null;
  actions: IActionItem[];
  _showPopup?: boolean;
}
