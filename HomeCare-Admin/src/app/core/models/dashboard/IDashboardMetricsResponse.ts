import { IMetricCardResponse } from "./IMetricCardResponse";

export interface IDashboardMetricsResponse{
    totalBookings: IMetricCardResponse;
    activeCustomers: IMetricCardResponse;
    activePartners: IMetricCardResponse;
    totalRevenue: IMetricCardResponse;
}