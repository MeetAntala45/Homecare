import { ECustomerStatus } from "../../enums/customer-user-management/customer-status";

export interface ICustomerList{
    id: number;
    name: string;
    email: string;
    mobileNumber: string;
    totalBookings: number;
    pendingBookings: number;
    status: ECustomerStatus;
}