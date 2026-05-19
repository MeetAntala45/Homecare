import { ECustomerStatus } from "../../enums/homepage/customer-status";

export interface ICustomerFilterRequest{
    pageNumber: number;
    pageSize: number; 
    sortBy: string; 
    sortOrder: string; 
    minBookings? : number | null;
    maxBookings? : number | null; 
    status : ECustomerStatus | '';
}