export interface IPartnerRevenueChartDataPoint {
    label: string;
    value: number;
  }
  
  export interface IPartnerRevenueChartResponse {
    period: string;
    data: IPartnerRevenueChartDataPoint[];
  }