export type FilterFieldType = 'select' | 'input' | 'range' | 'toggle' | 'date' | 'time' | 'custom-time';


export interface IFilterOption {
  label: string;
  value: any;
}

export interface IFilterField {
  key: string;
  label: string;
  type?: FilterFieldType;
  //for select only
  options?: IFilterOption[];
  allowFuture?: boolean;

  //input only
  suffix?: string; // for an ex. '%'
  inputType?: string;

  //range only 
  min?: number;
  max?: number;
  prefix?: string;  //ex. $

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