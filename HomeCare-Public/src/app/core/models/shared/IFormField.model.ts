export interface IFormField {
  name: string;
  label: string;
  type: 'text' | 'email' | 'otp' | 'password' | 'tel' | 'number' | 'select' | 'dropdown'; 
  required?: boolean;
  icon?: string;
  options?: { label: string; value: any }[];
  maxLength?: number;
}