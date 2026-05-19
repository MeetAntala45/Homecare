export interface ICustomerBookingList {
  bookingId: number;
  serviceId: number;
  serviceName: string;
  serviceType: string;
  assignedExpert: string;
  expertImage: string | null;
  partnerId: number | null;
  partnerPhone: string | null;
  address: string;
  dateTime: string;
  amount: number;
  paymentMethod: string;
  status: string;
  actions?: { label: string; icon: string; action: string }[];
  reason: string;
}

export interface ICustomerBookingPaged {
  data: ICustomerBookingList[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  minAmount: number;
  maxAmount: number;
}
