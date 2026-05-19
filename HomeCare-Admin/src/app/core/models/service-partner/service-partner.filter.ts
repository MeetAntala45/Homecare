export interface IServicePartnerFilterRequest {
  pageNumber: number;
  pageSize: number;
  sortBy: string;
  sortOrder: string;
  partnerName?: string;
  serviceTypeId?: number;
  statusId?: number;
  minJob?: number;
  maxJob?: number;
}