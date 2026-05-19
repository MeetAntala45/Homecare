import { EAdminRole } from "../../enums/auth/admin-role";

export interface IAdminFilterRequest{
    pageNumber: number;
    pageSize: number;
    role: EAdminRole;
    isActive: boolean;
    sortBy: string; 
    sortOrder: string; 
}