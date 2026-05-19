import { Observable } from 'rxjs';


export interface IBlockConfirmation {
    message: string;
    apiCall: () => Observable<any>;
}