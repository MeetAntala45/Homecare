import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { IOfferResponse } from '../../models/offers/offers.response';
import { PagedResult } from '../../models/paged-result';
import { IApiResponse } from '../../models/api-response/api-response';
import { IConditionTypeResponse } from '../../models/offers/condition-type.response';
import { ICreateConditionTypeRequest } from '../../models/offers/create-condition-type.request';
import { IServiceTypeGroup } from '../../models/offers/service-type-hierarchy.model';
import { API_BASE_URL } from '../../constants/environment-config';

@Injectable({
  providedIn: 'root',
})
export class OffersService {
  private readonly base = `${API_BASE_URL}/api/offer`;
  private readonly conditionTypeBase = `${API_BASE_URL}/api/offers/condition-types`;

  constructor(private http: HttpClient) {}

  getAll(params: HttpParams): Observable<IApiResponse<PagedResult<IOfferResponse>>> {
    return this.http.get<IApiResponse<PagedResult<IOfferResponse>>>(`${this.base}/get`, { params });
  }

  create(dto: any): Observable<IApiResponse<string>> {
    return this.http.post<IApiResponse<string>>(`${this.base}/create`, dto);
  }

  update(dto: any): Observable<IApiResponse<string>> {
    return this.http.put<IApiResponse<string>>(`${this.base}/edit`, dto);
  }

  delete(id: number): Observable<IApiResponse<string>> {
    return this.http.delete<IApiResponse<string>>(`${this.base}/delete/${id}`);
  }

  getActiveConditionTypes(): Observable<IApiResponse<IConditionTypeResponse[]>>{
    return this.http.get<IApiResponse<IConditionTypeResponse[]>>(`${this.conditionTypeBase}/active-conditions`)
  }

  createCondtitionType(dto: ICreateConditionTypeRequest) : Observable<IApiResponse<string>>{
    return this.http.post<IApiResponse<string>>(`${this.conditionTypeBase}/add-condition`, dto)
  }

  getContextKeys(): Observable<IApiResponse<string[]>> {
    return this.http.get<IApiResponse<string[]>>(`${this.conditionTypeBase}/context-keys`);
  }
  getAllowedInputTypes(contextKey: string): Observable<IApiResponse<string[]>> {
    return this.http.get<IApiResponse<string[]>>(
      `${this.conditionTypeBase}/allowed-input-types/${contextKey}`
    );
  }

  getServiceTypeHierarchy(): Observable<IApiResponse<IServiceTypeGroup[]>> {
    return this.http.get<IApiResponse<IServiceTypeGroup[]>>(
      `${this.conditionTypeBase}/service-type-hierarchy`
    );
  }
}
