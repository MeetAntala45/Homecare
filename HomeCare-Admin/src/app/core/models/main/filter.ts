export interface FilterField {
    key: string;
    label: string;
    type: 'range' | 'toggle' | 'select' | 'text';
    rangeConfig?: RangeConfig;
    options?: SelectOption[];
    suffix?: string;
  }
  
  export interface RangeConfig {
    min: number;
    max: number;
    step?: number;
  }
  
  export interface SelectOption {
    label: string;
    value: string | number | boolean;
  }
  
  export interface FilterConfig {
    fields: FilterField[];
  }
  
  export interface FilterValues {
    [key: string]: any;
  }