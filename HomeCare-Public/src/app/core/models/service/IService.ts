export interface IService {
  id: number;
  name: string;
  description?: string;
  subCategoryId?: number;
  subCategoryName?: string;
  serviceTypeName?: string;
  price: number;
  bookingCount? : number;
  commissionPct?: number;
  durationMin?: number;
  isAvailable?: boolean;
  imagePaths: string[];
  inclusions?: string[];
  exclusions?: string[];
  hasPartner?: Boolean;
}