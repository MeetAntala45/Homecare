import { Injectable } from '@angular/core';
import { Router } from '@angular/router';

@Injectable({ providedIn: 'root' })
export class QueryParamService {
  constructor(private router: Router) {}

  getFilteredQueryParams(filter: any): any {
    const params: any = {};
    Object.keys(filter).forEach(key => {
      const value = filter[key];
      if (value !== null && value !== '' && value !== undefined) {
        params[key] = value;
      }
    });
    return params;
  }

  navigateToDetail(path: string, id: any, currentFilters: any) {
    const queryParams = this.getFilteredQueryParams(currentFilters);
    this.router.navigate([path, id], { queryParams });
  }
}