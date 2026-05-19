
import { Component, inject, OnInit } from '@angular/core';
import { Location, NgClass } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { MatTooltip } from '@angular/material/tooltip';
import { ServicePartnerService } from '../../../../core/services/service-partner/service-partner-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { ServicePartnerStatus } from '../../../../core/enums/service-partner/service-partner';
import { IServicePartnerDetail } from '../../../../core/models/service-partner/serivce-partner-detail';
import { SERVICE_PARTNER_MESSAGES } from '../../../../core/constants/service-partner-messages';
import { GridLayout } from '../../../../shared/components/grid-layout/grid-layout';
import { FilterPanel } from '../../../../shared/components/filter-panel/filter-panel';
import { IGridColumn } from '../../../../core/models/shared-components/IDataGridModel';
import { IFilterPanelData } from '../../../../core/models/shared-components/IFilterPanel';
import { IPartnerAssignedService, IPartnerAssignedServiceFilter } from '../../../../core/models/service-partner/partner-assigned-service';
import { API_BASE_URL } from '../../../../core/constants/environment-config';

@Component({
  selector: 'app-service-partner-detail',
  standalone: true,
  imports: [NgClass, MatTooltip, MatDialogModule, GridLayout],
  templateUrl: './service-partner-detail.html',
  styleUrl: './service-partner-detail.css',
})
export class ServicePartnerDetail implements OnInit {

  private route = inject(ActivatedRoute);
  private location = inject(Location);
  private servicePartnerService = inject(ServicePartnerService);
  private toaster = inject(Toaster);
  private dialog = inject(MatDialog);
  private router = inject(Router);



  partner: IServicePartnerDetail | null = null;
  isPageLoading = false;
  isLoading = true;
  actionLoading = false;
  selectedTags = new Set<string>();


  assignedServiceColumns: IGridColumn[] = [
    { header: 'Name', field: 'serviceName', type: 'text', width: '14%', sortable: true },
    { header: 'Customer', field: 'customerName', type: 'text', width: '14%', sortable: true },
    { header: 'Date & Time', field: 'dateTime', type: 'text', width: '14%', sortable: true },
    { header: 'Address', field: 'serviceAddress', type: 'text', width: '24%', sortable: false },
    { header: 'Status', field: 'status', type: 'booking-status', width: '7%', sortable: false },
  ];

  assignedServicesData: IPartnerAssignedService[] = [];
  assignedServiceTotalCount = 0;
  activeAssignedServiceFilters: Record<string, any> = {};

  assignedServiceFilter: IPartnerAssignedServiceFilter = {
    pageNumber: 1,
    pageSize: 5,
    sortBy: '',
    sortOrder: 'desc',
    date: null,
    time: null,
    status: '',
  };


  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.loadPartner(id);
      this.loadAssignedServices(id);
    }
  }


  loadPartner(id: number): void {
    this.isPageLoading = true;
    this.servicePartnerService.getById(id).subscribe({
      next: (res: any) => {
        if (!res.success || !res.data) {
          this.toaster.error(res.message ?? SERVICE_PARTNER_MESSAGES.LOAD_DETAIL_FAILED);
          return;
        }
        this.partner = res.data;
      },
      error: (err: any) => {
        this.isPageLoading = false;
        this.toaster.error(err?.error?.message ?? SERVICE_PARTNER_MESSAGES.LOAD_DETAIL_FAILED);
      },
      complete: () => { this.isPageLoading = false; },
    });
  }


  loadAssignedServices(partnerId: number): void {
    this.isLoading = true;
    const f = this.assignedServiceFilter;
    const params: Record<string, any> = {
      pageNumber: f.pageNumber,
      pageSize: f.pageSize,
    };

    if (f.sortBy) params['sortBy'] = f.sortBy;
    if (f.sortOrder) params['sortOrder'] = f.sortOrder;
    if (f.date) params['date'] = f.date;
    if (f.time) params['time'] = f.time;
    if (f.status) params['status'] = f.status;

    this.servicePartnerService.getAssignedServices(partnerId, params).subscribe({
      next: (res: any) => {
        this.assignedServicesData = res.data?.data ?? [];
        this.assignedServiceTotalCount = res.data?.totalCount ?? 0;
      },
      error: () => {
        this.isLoading = false;
        this.toaster.error(SERVICE_PARTNER_MESSAGES.LOAD_DETAIL_FAILED);
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  onAssignedServicePageChanged(e: { pageNumber: number; pageSize: number }): void {
    this.assignedServiceFilter.pageNumber = e.pageNumber;
    this.assignedServiceFilter.pageSize = e.pageSize;
    this.loadAssignedServices(this.partner!.id);
  }

  onAssignedServiceSortChanged(e: { sortBy: string; sortOrder: string }): void {
    this.assignedServiceFilter.sortBy = e.sortBy;
    this.assignedServiceFilter.sortOrder = e.sortOrder;
    this.assignedServiceFilter.pageNumber = 1;
    this.loadAssignedServices(this.partner!.id);
  }

  openAssignedServiceFilter(): void {
    const data: IFilterPanelData = {
      fields: [
        { key: 'date', label: 'Date', type: 'date', allowFuture: true },
        { key: 'time', label: 'Time', type: 'time' },
        {
          key: 'status', label: 'Status', type: 'select',
          options: [
            { label: 'Pending', value: 'Pending' },
            { label: 'Completed', value: 'Completed' },
            { label: 'Cancelled', value: 'Cancelled' },
            { label: 'InProgress', value: 'InProgress' },
            { label: 'Onleave', value:'Onleave' }
          ],
        },
      ],
      initialValues: this.activeAssignedServiceFilters,
    };

    const dialogRef = this.dialog.open(FilterPanel, {
      data,
      position: { right: '0', top: '0' },
      height: '100vh',
      maxWidth: '360px',
      panelClass: 'filter-panel-dialog',
    });

    dialogRef.afterClosed().subscribe((filters: Record<string, any> | null | 'reset') => {
      if (filters === undefined) return;

      if (filters === 'reset') {
        this.activeAssignedServiceFilters = {};
        this.assignedServiceFilter.date = null;
        this.assignedServiceFilter.time = null;
        this.assignedServiceFilter.status = '';
        this.assignedServiceFilter.pageNumber = 1;
      } else {
        this.activeAssignedServiceFilters = filters!;

        const rawDate = filters!['date'];
        if (rawDate) {
          const d = new Date(rawDate);
          const yyyy = d.getFullYear();
          const mm = String(d.getMonth() + 1).padStart(2, '0');
          const dd = String(d.getDate()).padStart(2, '0');
          this.assignedServiceFilter.date = `${yyyy}-${mm}-${dd}`;
        } else {
          this.assignedServiceFilter.date = null;
        }

        this.assignedServiceFilter.time = filters!['time'] ?? null;
        this.assignedServiceFilter.status = filters!['status'] ?? '';
        this.assignedServiceFilter.pageNumber = 1;
      }

      this.loadAssignedServices(this.partner!.id);
    });
  }


  approve(): void {
    if (!this.partner) return;
    this.actionLoading = true;
    this.servicePartnerService.approve(this.partner.id).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.toaster.success(res.message ?? SERVICE_PARTNER_MESSAGES.APPROVED);
          this.partner!.statusId = ServicePartnerStatus.Active;
        } else {
          this.toaster.error(res.message ?? SERVICE_PARTNER_MESSAGES.APPROVE_FAILED);
        }
      },
      error: (err: any) => {
        this.toaster.error(err?.error?.message ?? SERVICE_PARTNER_MESSAGES.GENERIC_ERROR);
      },
      complete: () => { this.actionLoading = false; },
    });
  }

  reject(): void {
    if (!this.partner) return;
    this.actionLoading = true;
    this.servicePartnerService.reject(this.partner.id).subscribe({
      next: (res: any) => {
        if (res.success) {
          this.toaster.success(res.message ?? SERVICE_PARTNER_MESSAGES.REJECTED);
          this.partner!.statusId = ServicePartnerStatus.Rejected;
        } else {
          this.toaster.error(res.message ?? SERVICE_PARTNER_MESSAGES.REJECT_FAILED);
        }
      },
      error: (err: any) => {
        this.toaster.error(err?.error?.message ?? SERVICE_PARTNER_MESSAGES.GENERIC_ERROR);
      },
      complete: () => { this.actionLoading = false; },
    });
  }


  toggleTag(tag: string): void {
    if (this.selectedTags.has(tag)) this.selectedTags.delete(tag);
    else this.selectedTags.add(tag);
  }

  goBack(): void {
    const returnTo = this.route.snapshot.queryParams['returnTo'];
  
    if (returnTo) {
      this.router.navigateByUrl(returnTo);
    } else {
      const currentQueryParams = this.route.snapshot.queryParams;
      this.router.navigate(['/admin/service-partners'], {
        queryParams: currentQueryParams
      });
    }
  }
  formatFileSize(kb: number): string {
    if (!kb) return '';
    return kb >= 1024 ? (kb / 1024).toFixed(1) + ' MB' : kb + ' KB';
  }

  getStatusClass(statusId: number): string {
    const map: Record<number, string> = {
      [ServicePartnerStatus.Pending]: 'status--pending',
      [ServicePartnerStatus.Active]: 'status--active',
      [ServicePartnerStatus.Inactive]: 'status--inactive',
      [ServicePartnerStatus.Rejected]: 'status--rejected',
      [ServicePartnerStatus.Onleave]: 'status--onleave'
    };
    return map[statusId] ?? '';
  }

  getStatusLabel(statusId: number): string {
    const map: Record<number, string> = {
      [ServicePartnerStatus.Pending]: 'Pending',
      [ServicePartnerStatus.Active]: 'Active',
      [ServicePartnerStatus.Inactive]: 'Inactive',
      [ServicePartnerStatus.Rejected]: 'Rejected',
      [ServicePartnerStatus.Onleave]: 'Onleave'
    };
    return map[statusId] ?? '';
  }

  async downloadFile(filePath: string, documentName: string, fileType: string): Promise<void> {
    try {
      const fullUrl = `${API_BASE_URL}${filePath}`;
      const response = await fetch(fullUrl);
      const blob = await response.blob();
      const url = window.URL.createObjectURL(blob);
      const ext = fileType ?? filePath.split('.').pop() ?? '';
      const fileName = `${documentName}.${ext}`;
      const a = document.createElement('a');
      a.href = url;
      a.download = fileName;
      document.body.appendChild(a);
      a.click();
      document.body.removeChild(a);
      window.URL.revokeObjectURL(url);
    } catch {
      this.toaster.error(SERVICE_PARTNER_MESSAGES.DOWNLOAD_FAILED);
    }
  }
  onRowClicked(row: IPartnerAssignedService): void {
    if (!row.customerId) return;
    this.router.navigate(['/admin/customer-users', row.customerId], {
      queryParams: { returnTo: `/admin/service-partners/${this.partner!.id}` }
    });
  }
}