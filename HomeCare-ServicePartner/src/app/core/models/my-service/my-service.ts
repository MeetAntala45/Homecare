export interface IServiceTypeHierarchy {
    serviceTypeId: number;
    serviceTypeName: string;
    categories: ICategory[];
}

export interface ICategory {
    categoryId: number;
    categoryName: string;
    subCategories: ISubCategory[];
}

export interface ISubCategory {
    subCategoryId: number;
    subCategoryName: string;
    services: IServiceItem[];
}

export interface IServiceItem {
    serviceId: number;
    serviceName: string;
}