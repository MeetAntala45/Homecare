export interface ICustomerBookingSummary {
  customerId: number;
  customerName: string;
  mobileNumber: string;
  email: string;
  address: string;
  totalBookedServices: number;
  totalBookingAmount: number;
  paymentMethod: string;
  paymentMethodValue: number;
}

export interface ICustomerBookingSummaryPaged {
  data: ICustomerBookingSummary[];
  totalCount: number;
  pageNumber: number;
  pageSize: number;
  totalPages: number;
  minBookedServices: number;
  maxBookedServices: number;
  minAmount: number;
  maxAmount: number;
}
