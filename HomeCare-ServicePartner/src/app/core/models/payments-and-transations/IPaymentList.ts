export interface IPaymentList {
    id: number;
    user: string;
    transactionId: string;
    mobileNumber: string;
    service: string;
    amount: number;
    paymentMethod: string;
}