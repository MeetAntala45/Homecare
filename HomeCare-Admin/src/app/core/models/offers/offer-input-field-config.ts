import { IServiceTypeGroup } from "./service-type-hierarchy.model";

export interface IOfferDropdownOption {
    label: string;
    value: string | number;
  }

export interface IOfferInputFieldConfig {
    type?: 'text' | 'number' | 'select' | 'multi-select' | 'time' | 'date' | 'textarea' | 'subcategory-select';
    label?: string;
    placeholder?: string;
    suffixText?: string;
    options?: IOfferDropdownOption[];
    min?: number;
    max?: number;
    hint?: string;
    requiredMessage?: string;
    serviceTypeHierarchy?: IServiceTypeGroup[];
    disableMinDate?: boolean;
    maxLength?: number
  }