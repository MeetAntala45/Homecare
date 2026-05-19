export interface IGetPartnerChartRequest {
    period: 'week' | 'month' | 'year';
    week?: 'this' | 'last';
  }