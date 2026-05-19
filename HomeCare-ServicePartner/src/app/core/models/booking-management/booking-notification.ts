export interface IBookingNotification {
    bookingId: number;
    partnerId: number;
    customerId: number;
    customerName: string;
    serviceName: string;
    paymentMethod: string;
    paymentMethodValue: number;
    slotDate: string;
    slotTime: string;
    amount: number;
    message: string;
    createdAt: string;
  }