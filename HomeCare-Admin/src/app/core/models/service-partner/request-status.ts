import { ServicePartnerStatus } from "../../enums/service-partner/service-partner";

export interface IApproveRejectRequest {
  id: number;
  status: ServicePartnerStatus.Active | ServicePartnerStatus.Rejected;
}