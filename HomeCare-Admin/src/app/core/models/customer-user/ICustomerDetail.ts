import { ECustomerStatus } from "../../enums/customer-user-management/customer-status";

export interface ICustomerDetail{
    id: number;
    name: string;
    email: string;
    mobileNumber: string;
    status: ECustomerStatus;
}