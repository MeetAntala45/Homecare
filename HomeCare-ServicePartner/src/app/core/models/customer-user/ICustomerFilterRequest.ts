import { ECustomerStatus } from "../../enums/customer-user-management/customer-status";

export interface ICustomerFilterRequest{
    pageNumber: number;
    pageSize: number; 
    sortBy: string; 
    sortOrder: string; 
    userName?: string;
    minBookings? : number | null;
    maxBookings? : number | null; 
    status : ECustomerStatus | '';
    name?:string;
}