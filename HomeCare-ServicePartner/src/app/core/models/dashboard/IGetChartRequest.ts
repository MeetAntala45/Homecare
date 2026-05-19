export interface IGetChartRequest {
    period: 'week' | 'month' | 'year';
    week?: 'this' | 'last';
}
