import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { IApiResponse } from '../../models/apiResponse/IApiResponse';
import { Observable } from 'rxjs';
import { IContact } from '../../models/contact-us/IContact';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class ContactUsService {
  constructor(private http: HttpClient) { }

  addContact(service : IContact){
    return this.http.post<IApiResponse<IContact[]>>(`${API_BASE_URL}/api/support/add`,service);
  }
}
