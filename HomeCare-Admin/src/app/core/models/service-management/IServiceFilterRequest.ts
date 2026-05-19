export interface IServiceFilterRequest {
    pageNumber: number;
    pageSize: number;
    subCategoryId?: number;
    minPrice?: number;
    maxPrice?: number;
    isAvailable?: boolean;
    commissionPct?: number;
}