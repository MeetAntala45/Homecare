import { Component, inject, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { GridLayout } from '../../../shared/components/grid-layout/grid-layout';
import { DeleteConfirmation } from '../../../shared/components/delete-confirmation/delete-confirmation';
import { FilterPanel } from '../../../shared/components/filter-panel/filter-panel';
import { Toaster } from '../../../core/services/toaster/toaster';
import { IGridColumn } from '../../../core/models/shared-components/IDataGridModel';
import { IFilterPanelData } from '../../../core/models/shared-components/IFilterPanel';
import { IServicePartnerFilterRequest } from '../../../core/models/service-partner/service-partner.filter';
import { IServicePartnerListItem } from '../../../core/models/service-partner/service-partner.response';
import { ServicePartnerService } from '../../../core/services/service-partner/service-partner-service';
import { IDropdownOption } from '../../../core/models/service-partner/dropdown-option';
import { ServicePartnerStatus } from '../../../core/enums/service-partner/service-partner';
import { ConfirmationDialog } from '../../../shared/components/confirmation-dialog/confirmation-dialog';
import { HttpParams } from '@angular/common/http';
import { SERVICE_PARTNER_MESSAGES } from '../../../core/constants/service-partner-messages';

@Component({
  selector: 'app-service-partner',
  standalone: true,
  imports: [GridLayout],
  templateUrl: './service-partner.html',
  styleUrl: './service-partner.css',
})
export class ServicePartner implements OnInit {
  private servicePartnerService = inject(ServicePartnerService);
  private toaster = inject(Toaster);
  private dialog = inject(MatDialog);
  private route = inject(ActivatedRoute);
  private router = inject(Router);

  columns: IGridColumn[] = [
    { field: 'servicePartnerId', header: 'ID', width: '5%', sortable: true },
    { field: 'fullName', header: 'Name', width: '11%', sortable: true },
    { field: 'mobileNumber', header: 'Mobile Number', width: '11%', sortable: false },
    { field: 'email', header: 'Email', width: '16%', sortable: true },
    { field: 'residentialAddress', header: 'Address', width: '16%', sortable: false },
    { field: 'serviceType', header: 'Job', width: '15%', sortable: false },
    { field: 'jobsDone', header: 'Jobs Completed', width: '13%', sortable: true },
    { field: 'statusBadge', header: 'Status', type: 'partner-status', width: '15%', sortable: false },
  ];

  filter: IServicePartnerFilterRequest = {
    pageNumber: 1,
    pageSize: 10,
    partnerName: '',
    sortBy: 'id',
    sortOrder: 'desc',
  };

  data: any[] = [];
  totalCount = 0;
  isLoading = true;
  minJobCompleted = 0;
  maxJobCompleted = 9999;

  activeFilters: Partial<IServicePartnerFilterRequest> = {};
  rawPanelFilters: Record<string, any> = {};
  serviceTypeOptions: { label: string; value: number }[] = [];

  statusOptions = [
    { label: 'Pending', value: ServicePartnerStatus.Pending },
    { label: 'Active', value: ServicePartnerStatus.Active },
    { label: 'Inactive', value: ServicePartnerStatus.Inactive },
    { label: 'Rejected', value: ServicePartnerStatus.Rejected },
    { label: 'Onleave', value: ServicePartnerStatus.Onleave }
  ];

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (Object.keys(params).length > 0) {
        this.filter.pageNumber = Number(params['pageNumber']) || 1;
        this.filter.pageSize = Number(params['pageSize']) || 10;
        this.filter.sortBy = params['sortBy'] || 'id';
        this.filter.sortOrder = params['sortOrder'] || 'desc';

        if (params['partnerName']) this.activeFilters.partnerName = params['partnerName'];
        if (params['serviceTypeId']) this.activeFilters.serviceTypeId = Number(params['serviceTypeId']);
        if (params['statusId']) this.activeFilters.statusId = Number(params['statusId']);
        if (params['minJob']) this.activeFilters.minJob = Number(params['minJob']);
        if (params['maxJob']) this.activeFilters.maxJob = Number(params['maxJob']);

        this.rawPanelFilters = {};

        if (params['partnerName'])
          this.rawPanelFilters['partnerName'] = params['partnerName'];

        if (params['serviceTypeId'])
          this.rawPanelFilters['serviceTypeId'] = Number(params['serviceTypeId']);

        if (params['statusId'])
          this.rawPanelFilters['statusId'] = Number(params['statusId']);

        if (params['minJob'] || params['maxJob']) {
          this.rawPanelFilters['jobsCompleted'] = {
            min: params['minJob'] ? Number(params['minJob']) : this.minJobCompleted,
            max: params['maxJob'] ? Number(params['maxJob']) : this.maxJobCompleted
          };
        }
      }

      this.loadPartners();
      this.loadServiceTypes();
    });
  }

  private loadServiceTypes(): void {
    this.servicePartnerService.getServiceTypes().subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          this.serviceTypeOptions = res.data.map((d: IDropdownOption) => ({
            label: d.label,
            value: Number(d.value),
          }));
        }
      },
      error: () => { },
    });
  }

  loadPartners(): void {
    this.isLoading = true;

    let params = new HttpParams()
      .set('pageNumber', this.filter.pageNumber)
      .set('pageSize', this.filter.pageSize)
      .set('sortBy', this.filter.sortBy)
      .set('sortOrder', this.filter.sortOrder);

    if (this.activeFilters.partnerName != null)
      params = params.set('partnerName', String(this.activeFilters.partnerName));
    if (this.activeFilters.serviceTypeId != null)
      params = params.set('serviceTypeId', this.activeFilters.serviceTypeId);
    if (this.activeFilters.statusId != null)
      params = params.set('statusId', this.activeFilters.statusId);
    if (this.activeFilters.minJob != null)
      params = params.set('minJob', this.activeFilters.minJob);
    if (this.activeFilters.maxJob != null)
      params = params.set('maxJob', this.activeFilters.maxJob);

    this.servicePartnerService.getAll(params).subscribe({
      next: (res: any) => {
        if (!res.success || !res.data) {
          this.toaster.error(res.message ?? SERVICE_PARTNER_MESSAGES.LOAD_FAILED);
          return;
        }
        this.totalCount = res.data.totalCount;
        this.minJobCompleted = res.data?.min ?? 0;
        this.maxJobCompleted = res.data?.max ?? 0;
        this.data = res.data.data.map((p: IServicePartnerListItem) => this.mapRow(p));
      },
      error: (err: any) => {
        this.isLoading = false;
        this.toaster.error(err?.error?.message ?? SERVICE_PARTNER_MESSAGES.LOAD_FAILED);
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }

  private mapRow(p: IServicePartnerListItem): any {
    const isActive = p.statusId === ServicePartnerStatus.Active;

    return {
      ...p,
      residentialAddress: p.residentialAddress?.trim() ? p.residentialAddress.trim() : '—',
      jobsDone: p.jobsDone === 0 ? '—' : p.jobsDone,
      statusBadgeClass: this.getStatusClass(p.statusId),
      statusBadgeLabel: this.getStatusLabel(p.statusId),
      actions: [
        ...(isActive || p.statusId === ServicePartnerStatus.Inactive
          ? [
            isActive
              ? { label: 'Inactive', icon: 'bi-person-dash', action: 'toggleStatus' }
              : { label: 'Active', icon: 'bi-person-check', action: 'toggleStatus' },
          ]
          : []),
        { label: 'Delete', icon: 'bi-trash', action: 'delete' },
      ],
    };
  }

  private getStatusClass(statusId: number): string {
    const map: Record<number, string> = {
      [ServicePartnerStatus.Pending]: 'badge-pending',
      [ServicePartnerStatus.Active]: 'badge-active',
      [ServicePartnerStatus.Inactive]: 'badge-sp-inactive',
      [ServicePartnerStatus.Rejected]: 'badge-rejected',
      [ServicePartnerStatus.Onleave]: 'badge-onleave'
    };
    return map[statusId] ?? '';
  }

  private getStatusLabel(statusId: number): string {
    const map: Record<number, string> = {
      [ServicePartnerStatus.Pending]: 'Pending',
      [ServicePartnerStatus.Active]: 'Active',
      [ServicePartnerStatus.Inactive]: 'Inactive',
      [ServicePartnerStatus.Rejected]: 'Rejected',
      [ServicePartnerStatus.Onleave]: 'Onleave'
    };
    return map[statusId] ?? '';
  }

  onSortChanged(event: { sortBy: string; sortOrder: string }): void {
    this.filter.sortBy = event.sortBy;
    this.filter.sortOrder = event.sortOrder;
    this.filter.pageNumber = 1;
    this.loadPartners();
  }

  onPageChanged(event: { pageNumber: number; pageSize: number }): void {
    this.filter.pageNumber = event.pageNumber;
    this.filter.pageSize = event.pageSize;
    this.loadPartners();
  }

  handleAction(event: any): void {
    const { action, row } = event;
    switch (action) {
      case 'toggleStatus': this.confirmToggleStatus(row); break;
      case 'delete': this.deletePartner(row.id); break;
    }
  }

  private confirmToggleStatus(row: any): void {
    const isActive = row.statusId === ServicePartnerStatus.Active;

    this.dialog
      .open(ConfirmationDialog, {
        width: '400px',
        disableClose: true,
        data: {
          title: SERVICE_PARTNER_MESSAGES.TOGGLE_STATUS_DIALOG.title(isActive),
          message: SERVICE_PARTNER_MESSAGES.TOGGLE_STATUS_DIALOG.body(isActive, row.fullName),
          confirmLabel: SERVICE_PARTNER_MESSAGES.TOGGLE_STATUS_DIALOG.confirmLabel(isActive),
          confirmColor: 'primary',
          apiCall: () => this.servicePartnerService.updateStatus(row.id),
        },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result) this.loadPartners();
      });
  }

  deletePartner(id: number): void {
    this.dialog
      .open(DeleteConfirmation, {
        width: '400px',
        disableClose: true,
        data: {
          message: SERVICE_PARTNER_MESSAGES.DELETE_CONFIRM,
          apiCall: () => this.servicePartnerService.delete(id),
        },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result) {
          this.totalCount--;
          this.loadPartners();
        }
      });
  }

  openFilter(): void {
    const data: IFilterPanelData = {
      fields: [
        {
          key: 'partnerName',
          label: 'Name',
          type: 'input',
        },
        {
          key: 'serviceTypeId',
          label: 'Job',
          type: 'select',
          options: this.serviceTypeOptions,
        },
        {
          key: 'jobsCompleted',
          label: 'Jobs Completed',
          type: 'range',
          min: this.minJobCompleted,
          max: this.maxJobCompleted,
        },
        {
          key: 'statusId',
          label: 'Status',
          type: 'select',
          options: this.statusOptions,
        },
      ],
      initialValues: this.rawPanelFilters,
    };

    this.dialog
      .open(FilterPanel, {
        data: { ...data, useMatInput: true },
        position: { right: '0', top: '0' },
        height: '100vh',
        maxWidth: '360px',
        panelClass: 'filter-panel-dialog',
      })
      .afterClosed()
      .subscribe((filters: Record<string, any> | null) => {
        if (filters === null) {
          this.rawPanelFilters = {};
          this.activeFilters = {};
          this.filter.partnerName = '';
          this.filter.pageNumber = 1;
          this.loadPartners();
          return;
        }

        this.rawPanelFilters = filters;
        this.activeFilters = {
          partnerName: filters['partnerName']?.trim() || undefined,
          serviceTypeId: filters['serviceTypeId'] ?? undefined,
          minJob: filters['jobsCompleted']?.min === 0 ? undefined : filters['jobsCompleted']?.min,
          maxJob: filters['jobsCompleted']?.max === 9999 ? undefined : filters['jobsCompleted']?.max,
          statusId: filters['statusId'] ?? undefined,
        };

        this.filter.partnerName = filters['partnerName']?.trim() || '';
        this.filter.pageNumber = 1;
        this.loadPartners();
      });
  }

  onRowClicked(row: any): void {
    const params: any = {
      pageNumber: this.filter.pageNumber,
      pageSize: this.filter.pageSize,
      sortBy: this.filter.sortBy,
      sortOrder: this.filter.sortOrder,
    };

    if (this.activeFilters.partnerName) params['partnerName'] = this.activeFilters.partnerName;
    if (this.activeFilters.serviceTypeId != null) params['serviceTypeId'] = this.activeFilters.serviceTypeId;
    if (this.activeFilters.statusId != null) params['statusId'] = this.activeFilters.statusId;
    if (this.activeFilters.minJob != null) params['minJob'] = this.activeFilters.minJob;
    if (this.activeFilters.maxJob != null) params['maxJob'] = this.activeFilters.maxJob;

    this.router.navigate(['/admin/service-partners', row.id], {
      queryParams: params
    });
  }
}