import { IRevenueChartDataPointResponse } from "./IRevenueChartDataPointResponse";

export interface IRevenueChartResponse{
    period: string;
    data: IRevenueChartDataPointResponse[];
}