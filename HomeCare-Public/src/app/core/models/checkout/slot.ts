export interface IDateAvailability {
  date: string;
  hasSlot: boolean;
  allBooked?: boolean;
}

export interface ISlotResponse {
  startTime: string;
  endTime: string;
  available: boolean;
}

export interface IGetSlotsRequest {
  serviceId: number;
  date: string;
  session: string;
  addressId: number ; 
  
}

export interface ISelectedSlot {
  date: string;
  session: string;
  startTime: string;
  endTime: string;
  displayDate: string;
  displayTime: string;
}

export interface IApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors?: string[];

}

export interface ISessionConfig {
  key: string;
  label: string;
  range: string;
  icon: string;
}