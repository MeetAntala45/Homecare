import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { ServiceManagementService } from '../../../core/services/service-management/service-management-service';
import { IServiceType } from '../../../core/models/service-management/service';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { AddServiceDialog } from '../master-data/add-service-dialog/add-service-dialog';
import { ManageServiceDialog } from '../master-data/manage-service-dialog/manage-service-dialog';
import { ServiceTypeDetail } from './service-type-detail/service-type-detail';
import { GridLayout } from '../../../shared/components/grid-layout/grid-layout';
import { ActivatedRoute, Router } from '@angular/router';
import { Toaster } from '../../../core/services/toaster/toaster';
import { API_BASE_URL } from '../../../core/constants/environment-config';

@Component({
  selector: 'app-service-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatCardModule,
    MatButtonModule,
    MatTooltipModule,
    MatDividerModule,
    ServiceTypeDetail,
    GridLayout,
  ],
  templateUrl: './service-management.html',
  styleUrl: './service-management.css',
})
export class ServiceManagement implements OnInit {
  BASE_URL = API_BASE_URL;
  serviceTypes: IServiceType[] = [];
  loading = false;

  expandedServiceTypeId: number | null = null;
  activeCategoryId: number | null = null;
  constructor(
    private dialog: MatDialog,
    private svc: ServiceManagementService,
    private route: ActivatedRoute,
    private router: Router,
    private toaster: Toaster,
  ) { }

  ngOnInit(): void {
    this.expandedServiceTypeId =
      Number(this.route.snapshot.queryParamMap.get('expandedServiceTypeId')) || null;
    this.activeCategoryId =
      Number(this.route.snapshot.queryParamMap.get('activeCategoryId')) || null;

    if (this.expandedServiceTypeId || this.activeCategoryId) {
      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: {},
        replaceUrl: true,
      });
    }
    this.loadServiceTypes();
  }

  loadServiceTypes(): void {
    this.loading = true;
    this.svc.getServiceTypes().subscribe({
      next: (response) => {
        const fresh = response.data ?? [];
        this.serviceTypes = fresh.map(s => ({
          ...s,
          expanded: this.serviceTypes.find(old => old.id === s.id)?.expanded ?? false
        }));

      },
      error: (err) => {
        this.toaster.error(err?.error?.message);
        this.loading = false;
      },
      complete: () => {
        setTimeout(() => {
          this.loading = false;
        }, 400);
      }
    });
  }

  addServiceDialog(): void {
    this.dialog
      .open(AddServiceDialog, { width: '400px' })
      .afterClosed()
      .subscribe((result) => {
        if (result) this.loadServiceTypes();
      });
  }

  toggleService(service: IServiceType & { expanded?: boolean }): void {
    service.expanded = !service.expanded;
    if (!service.expanded) this.expandedServiceTypeId = null;
  }

  manageServiceDialog(service: IServiceType): void {
    this.dialog.open(ManageServiceDialog, {
      maxWidth: '50rem',
      maxHeight: '520px',
      data: service,
      panelClass: 'manage-service-dialog',
    }).afterClosed().subscribe((result) => {
      if (result) {
        this.expandedServiceTypeId = service.id;
        this.activeCategoryId = result.categoryId ?? null;
        this.loadServiceTypes()
      }
    })
  }
}
