export interface IGridColumn{
    field: string;
    header: string;
    type?: 'text' | 'status' | 'rating' | 'currency' | 'id' | 'partner-status'|'expert'|'booking-status';
    width?: string;
    height?: string;
    tooltip?: boolean;
    sortable?: boolean; 
    isPositiveAmount?: boolean;
}