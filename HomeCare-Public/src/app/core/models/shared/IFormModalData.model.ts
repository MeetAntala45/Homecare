import { Observable } from "rxjs";
import { IFormField } from "./IFormField.model";

export interface IFormModalData {
  title: string;
  fields: IFormField[];
  subtitle?: string;
  initialData?: any;
  enableFileUpload?: boolean;
  submitLabel?: string;
  onSubmit: (formValue: any) => Observable<any>;
}