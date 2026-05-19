export interface ISubcategoryOption {
    id: number;
    name: string;
}

export interface ICategoryGroup {
    categoryId: number;
    categoryName: string;
    subcategories: ISubcategoryOption[];
}

export interface IServiceTypeGroup {
    serviceTypeId: number;
    serviceTypeName: string;
    categories: ICategoryGroup[];
}