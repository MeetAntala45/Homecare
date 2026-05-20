export interface ICreateBookingRequest {
  serviceId: number;
  addressId: number;
  couponCode: string | null;
  slotDate: string;
  slotStartTime: string;
  slotEndTime: string;
  paymentMethod: number;
  useWallet: boolean;
}

export interface ICreateBookingResponse {
  bookingId: number;
  slotDate: string;
  slotStartTime: string;
  slotEndTime: string;
  totalAmount: number;
  paymentMethod: number;
  bookingStatus: number;
  paymentStatus: number;
  couponCode?: string;
  walletAmountUsed: number;
}
