import { ISavedAddress } from './ISavedAddress';

export interface ICustomerProfile {
  email: string;
  mobileNumber: string | null;
  addresses: ISavedAddress[];
}