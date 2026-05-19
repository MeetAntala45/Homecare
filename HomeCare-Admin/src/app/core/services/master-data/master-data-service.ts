import { HttpClient } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { IServiceType } from "../../models/master-data/service-type/service-type";
import { IApiResponse } from "../../models/api-response/api-response";
import { Observable } from "rxjs";
import { API_BASE_URL } from "../../constants/environment-config";

@Injectable({
  providedIn: 'root',
})
export class MasterDataService {

  private apiUrl = `${API_BASE_URL}/api/admin/service-types`;

  constructor(private http: HttpClient) { }

  getAll(): Observable<IApiResponse<IServiceType[]>> {
    return this.http.get<IApiResponse<IServiceType[]>>(`${this.apiUrl}/all`);
  }

  addService(formData: FormData) {
    return this.http.post<IApiResponse<string>>(`${this.apiUrl}/add`, formData);
  }

  deleteServiceType(id: number) {
    return this.http.delete(`${this.apiUrl}/delete/${id}`);
  }

  editServiceType(id: number, formData: FormData) {
    return this.http.put<IApiResponse<string>>(`${this.apiUrl}/edit/${id}`, formData);
  }
}