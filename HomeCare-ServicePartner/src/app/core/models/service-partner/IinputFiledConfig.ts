import { IDropdownOption } from "./dropdown-option";

export interface IInputFieldConfig {
    type?: 'text' | 'email' | 'tel' | 'date' | 'select' | 'number' | 'textarea';
    label?: string;
    placeholder?: string;
    prefixIcon?: string;
    suffixIcon?: string;
    suffixText?: string;
    options?: IDropdownOption[];
    rows?: number;
    suffixBsIcon?: string;
    prefixBsIcon?: string;
    requiredMessage?: string;
   
  }