import { ServicePartnerStatus } from "../../enums/service-partner/service-partner";

export interface IUpdateServicePartnerStatusRequest {
  id: number;
  status: ServicePartnerStatus;
}