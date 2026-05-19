export interface IUserPaymentDetail {
  userId: number;
  userName: string;
  mobileNumber?: string;
  transactionId: string;
  serviceName: string;
  serviceId: number;
  amount: number;
  paymentMethod: string;
  transactionDateTime: string;
}