import { ECustomerStatus } from "../../enums/homepage/customer-status";

export interface ICustomerList{
    id: number;
    name: string;
    email: string;
    mobileNumber: string;
    totalBookings: number;
    pendingBookings: number;
    status: ECustomerStatus;
}