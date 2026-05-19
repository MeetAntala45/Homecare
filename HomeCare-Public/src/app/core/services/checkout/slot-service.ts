import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import {
  IApiResponse,
  IDateAvailability,
  IGetSlotsRequest,
  ISlotResponse,
} from '../../models/checkout/slot.js';
import { API_BASE_URL } from '../../constants/environment-config.js';

@Injectable({
  providedIn: 'root',
})
export class SlotService  {
  private readonly apiUrl = `${API_BASE_URL}/api/slot`;

  constructor(private http: HttpClient) {}

  
  getAvailableDates(serviceId: number): Observable<IApiResponse<IDateAvailability[]>> {
    return this.http.get<IApiResponse<IDateAvailability[]>>(
      `${this.apiUrl}/dates`,
      { params: { serviceId: serviceId.toString() } }
    );
  }

  getSlots(dto: IGetSlotsRequest): Observable<IApiResponse<ISlotResponse[]>> {
    return this.http.get<IApiResponse<ISlotResponse[]>>(
      `${this.apiUrl}/get`,
      {
        params: {
          serviceId: String(dto.serviceId),
          date: dto.date,
          session: dto.session,
          addressId: dto.addressId
        },
      }
    );
  }

  getServicePartnerAvailability(): Observable<IApiResponse<Record<number, boolean>>> {
    return this.http.get<IApiResponse<Record<number, boolean>>>(
        `${this.apiUrl}/partner-availability`
    );
}
}