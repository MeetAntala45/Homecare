import { Observable } from 'rxjs';


export interface IDeleteConfirmation {
    message: string;
    apiCall: () => Observable<any>;
}