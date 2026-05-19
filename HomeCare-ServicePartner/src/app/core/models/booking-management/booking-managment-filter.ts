export interface IBookingGridFilter {
  pageNumber: number;
  pageSize: number;
  sortBy: string;
  sortOrder: string;
  serviceType?: string;
  fromDate?: string | null;
  time?: string | null;
  paymentMethod?: number | null;
  bookingStatus?: number | null;
  minBookings?: number | null;
  maxBookings?: number | null;
  minAmount?: number | null;
  maxAmount?: number | null;
}

export interface IBookingDetailFilter {
  pageNumber: number;
  pageSize: number;
  sortBy: string;
  sortOrder: string;
  serviceType?: string;
  fromDate?: string | null;
  time?: string | null;
  bookingStatus?: number | null;
}

export interface IActiveBookingFilters {
  serviceType?: string;
  date?: string | null;
  time?: string | null;
  bookedServices?: { min: number; max: number } | null;
  amount?: { min: number; max: number } | null;
  paymentMethod?: number | null;
  bookingStatus?: number | null;
}

export interface IDropdownOption {
  label: string;
  value: string | number;
}

export interface IBookingFilterRange {
  minBookings: number;
  maxBookings: number;
  minAmount: number;
  maxAmount: number;
}
