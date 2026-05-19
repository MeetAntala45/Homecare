export interface IAdminRequest {
    id?: number;
    name: string;
    email: string;
    mobileNumber: string;
    password?: string;
    confirmPassword?: string;
}