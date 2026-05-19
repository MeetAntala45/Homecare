import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { IApiResponse } from '../../models/api-response/api-response';
import { IChangePasswordRequest } from '../../models/admin-users/IChangePasswordRequest';
import { IAdminRequest } from '../../models/admin-users/IAdminRequest';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class AdminManagementService {
  private readonly baseUrl = `${API_BASE_URL}/api/admin`;

  constructor(private http: HttpClient) { }

  getAllAdminList(params: any) {
    return this.http.get<any>(`${this.baseUrl}/admin-list`, { params });
  }

  saveAdminUser(req: IAdminRequest) {
    return this.http.post<IApiResponse<number>>(`${this.baseUrl}/save`, req);
  }

  deleteAdminUser(id: number){
    return this.http.delete<IApiResponse<boolean>>(
      `${this.baseUrl}/delete-admin/${id}`
    );
  }

  changePassword(dto: IChangePasswordRequest){
    return this.http.put<IApiResponse<boolean>>(
      `${this.baseUrl}/change-password`,
      dto
    );
  }
}
