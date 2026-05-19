export type FilterFieldType = 'select' | 'input' | 'range' | 'toggle' | 'date' | 'time' | 'custom-time';


export interface IFilterOption {
  label: string;
  value: any;
}

export interface IFilterField {
  key: string;
  label: string;
  type?: FilterFieldType;
  options?: IFilterOption[];
  allowFuture?: boolean;

  suffix?: string; 
  inputType?: string;

  min?: number;
  max?: number;
  prefix?: string;

}

export interface IFilterExtras {
  discountPct?: boolean;
  rangeSlider?: {   
    label: string;
    key: string;
    max: number;
  };
  availabilityToggle?: {
    label: string;
    key: string;
  };
}

export interface IFilterPanelData {
    fields: IFilterField[];
    extras?: IFilterExtras;
    initialValues?: Record<string, any>;
    useMatInput?: boolean;
}