export interface IServiceType {
  id: number;
  name: string;
  imagePath: string;
  expanded?: boolean;
}

export interface ICategory {
  id: number;
  name: string;
  serviceCount?: number;
  subCategories: ISubCategory[];
}

export interface ISubCategory {
  id: number;
  name: string;
  categoryId: number;
}

export interface IService {
  id: number;
  name: string;
  discription: string;
  subCategoryId: number;       
  subCategoryName: string;
  categoryName: string;         
  serviceTypeName: string;
  price: number;
  commissionPct: number;
  isAvailable: boolean;
  durationMin: number;
  imagePaths: string[];
  inclusions: string[];
  exclusions: string[];
}
export interface IServiceRow extends IService {
  priceFormatted: string;
  commissionFormatted: string;
}


export interface ServiceDialogData {
  categoryName: string;
  categoryId: number;
  subCategories: ISubCategory[];
  serviceTypeName: string;
  service?: IService | null;
}
