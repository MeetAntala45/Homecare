export interface IUserPaymentList {
    id: number;
    transactionId: string;
    bookingId: string;
    service: string;
    amount: number;
    paymentMethod: string;
}
