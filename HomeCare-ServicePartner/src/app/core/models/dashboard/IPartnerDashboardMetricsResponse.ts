import { IMetricCardResponse } from "./IMetricCardResponse";


export interface IPartnerDashboardMetricsResponse {
  totalBookings: IMetricCardResponse;
  uniqueCustomers: IMetricCardResponse;
  averageRating: IMetricCardResponse;
  totalRevenue: IMetricCardResponse;
}