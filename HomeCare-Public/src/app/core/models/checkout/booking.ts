export interface ICreateBookingRequest {
    serviceId: number;
    addressId: number;
    couponCode: string | null;
    slotDate: string;
    slotStartTime: string;
    slotEndTime: string;
    paymentMethod: number;
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
  }
   
  export interface IApiResponse<T> {
    success: boolean;
    message: string;
    data: T;
    errors?: string[];
    statusCode?: number; 
  }