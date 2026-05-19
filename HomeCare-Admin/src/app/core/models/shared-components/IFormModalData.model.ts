import { Observable } from "rxjs";
import { IFormField } from "./IFormField.model";

export interface IFormModalData {
  title: string;
  fields: IFormField[];
  initialData?: any;
  enableFileUpload?: boolean;
  submitLabel?: string;
  onSubmit?: (formValue: any) => Observable<any>;
}