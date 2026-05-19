export interface IFormField {
  name: string;
  label: string;
  type: 'text' | 'email' | 'password' | 'tel' | 'number' | 'select' | 'dropdown' | 'input'; 
  required?: boolean;
  icon?: string;
  options?: { label: string; value: any }[];
  maxLength?: number;
}