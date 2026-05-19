
export interface ApplyLeaveRequest {
    fromDate: string;
    toDate: string;
    reason: string;
  }
  
  export interface LeaveResponse {
    id: number;
    fromDate: string;
    toDate: string;
    totalDays: number;
    reason: string;
    statusId: number;
    status: string;
    adminRemarks?: string;
    appliedOn: string;
    reviewedAt?: string;
  }
  
  export interface AdminLeaveItem extends LeaveResponse {
    partnerId: number;
    partnerName: string;
    partnerEmail: string;
    profileImage?: string;
  }
  
  export interface ReviewLeaveDto {
    isApproved: boolean;
    adminRemarks?: string;
  }
  
  export interface LeaveFilter {
    pageNumber?: number;
    pageSize?: number;
    statusId?: number | null;
    partnerName?: string;
    fromDate?: string;
    toDate?: string;
  }
  
  export interface PagedResult<T> {
    data: T[];
    totalCount: number;
    pageNumber: number;
    pageSize: number;
  }
  
  export interface ApiResponse<T> {
    success: boolean;
    message: string;
    data: T;
  }
  
  export interface PartnerSystemNotif {
    id: number;
    title: string;
    message: string;
    typeId: number;
    type: string;
    isRead: boolean;
    referenceId?: number;
    referenceType?: string;
    createdAt: string;
    timeAgo: string;
  }
  
  export interface AdminSystemNotif {
    id: number;
    title: string;
    message: string;
    typeId: number;
    type: string;
    isRead: boolean;
    referenceId?: number;
    referenceType?: string;
    fromPartnerId?: number;
    fromPartnerName?: string;
    createdAt: string;
    timeAgo: string;
  }