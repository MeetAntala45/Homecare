import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ServiceType } from '../../../core/services/home/serviceType/service-type';
import { IApiResponse } from '../../../core/models/apiResponse/IApiResponse';
import { IServiceType } from '../../../core/models/serviceType/IServiceType';
import { GradientOverlay } from '../../../shared/components/gradient-overlay/gradient-overlay';
import { CountFormatPipe } from "../../../shared/pipes/count-formatter/count-format-pipe";
import { Toaster } from '../../../core/services/toaster/toaster';
import { MatTooltip } from '@angular/material/tooltip';
import { API_BASE_URL } from '../../../core/constants/environment-config';


@Component({
  selector: 'app-services-page',
  imports: [CommonModule, GradientOverlay, CountFormatPipe, MatTooltip],
  templateUrl: './servicespage.html',
  styleUrl: './servicespage.css',
})
export class ServicePage {
  loading: boolean = false;

  BASE_URL = API_BASE_URL;

  serviceTypes: IServiceType[] = [];
  bookingCounts: { [key: number]: number } = {};

  constructor(private router: Router,
    private serviceType: ServiceType,
    private toaster: Toaster
  ) { }

  ngOnInit(): void {
    this.loadServiceTypes();
  }

  loadServiceTypes() {
    this.loading = true;
    this.serviceType.getAll().subscribe({
      next: (res: IApiResponse<IServiceType[]>) => {
        this.serviceTypes = res.data;
        this.loadBookingCounts();
        this.loading = false;
      },
      error: (err) => {
        this.toaster.error(err?.error?.message);
        this.loading = false;
      }
    });
  }

  loadBookingCounts() {
    this.serviceTypes.forEach(service => {
      this.serviceType.getBookingCountByServiceType(service.id).subscribe({
        next: (res) => {
          if (res.success) {
            this.bookingCounts[service.id] = res.data;
          }
        },
        error: (err) => {
          this.toaster.error(err?.error?.message);
        }
      });
    });
  }

  onServiceTypeClick(serviceType: IServiceType) {
    this.router.navigate(['/customer/service-list'], {
      queryParams: { id: serviceType.id, name: serviceType.name }
    });
  }
}
