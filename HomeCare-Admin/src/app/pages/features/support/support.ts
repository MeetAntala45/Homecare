import { Component, inject, OnInit } from '@angular/core';
import { GridLayout } from '../../../shared/components/grid-layout/grid-layout';
import { SupportService } from '../../../core/services/support/support';
import { Toaster } from '../../../core/services/toaster/toaster';
import { IGridColumn } from '../../../core/models/shared-components/IDataGridModel';
import { IFilterPanelData } from '../../../core/models/shared-components/IFilterPanel';
import { FilterPanel } from '../../../shared/components/filter-panel/filter-panel';
import { MatDialog } from '@angular/material/dialog';
import { ISupportFilterRequest } from '../../../core/models/support/filter';
import { ISupportResponse } from '../../../core/models/support/support-response';
import { IApiResponse } from '../../../core/models/api-response/api-response';
import { PagedResult } from '../../../core/models/paged-result';
import { SUPPORT_DEFAULTS, SUPPORT_MESSAGES } from '../../../core/constants/support-messages';
import { IExportConfig } from '../../../core/models/shared-components/IExportConfig';


@Component({
  selector: 'app-support',
  standalone: true,
  imports: [GridLayout],
  templateUrl: './support.html',
  styleUrl: './support.css',
})
export class Support implements OnInit {
  toaster = inject(Toaster);
  constructor(private dialog: MatDialog, private supportService: SupportService) { }

  columns: IGridColumn[] = [
    { field: 'id', header: 'Ticket ID', width: '8%', type: 'id',  sortable: true},
    { field: 'userName', header: 'User Name', width: '16%',  sortable: true },
    { field: 'mobile', header: 'Mobile', width: '14%' },
    { field: 'email', header: 'Email', width: '20%',  sortable: true },
    { field: 'description', header: 'Description', width: 'auto' },
    { field: 'createdAt', header: 'Created Date', width: '10%', height: '59px',  sortable: true },
  ];
  
  data : ISupportResponse[]= [] ;
  isLoading = true;
  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  sort = { sortBy: 'Id', sortOrder: 'desc' };
  
  activeFilters: Partial<ISupportFilterRequest> = {};
  
  exportConfig: IExportConfig = {
    csvUrl: 'export/support/csv',
    pdfUrl: 'export/support/pdf',
    filename: 'support-tickets',
    queryParams: () => ({
      userName: this.activeFilters.userName,
      createdDate: this.activeFilters.createdDate,
      sortBy: this.sort.sortBy,
      sortOrder: this.sort.sortOrder,
      pageNumber: this.pageNumber,
      pageSize: this.pageSize
    })
  };
  
  ngOnInit(): void {
    this.loadSupportData();   
  }

  loadSupportData(): void {
    this.isLoading = true;

    const params: any = {
      pageNumber: this.pageNumber,
      pageSize: this.pageSize,
      sortBy : this.sort.sortBy,
      sortOrder : this.sort.sortOrder,
    };

    if (this.activeFilters.userName) {
      params.userName = this.activeFilters.userName;
    }

    if (this.activeFilters.createdDate) {
      params.createdDate = this.activeFilters.createdDate;
    }

    this.supportService.getAll(params).subscribe({
      next: (res: IApiResponse<PagedResult<ISupportResponse>>) => {
        if (!res.success || !res.data) {
          this.toaster.error(res.message ?? SUPPORT_MESSAGES.error.LOAD_FAILED);
          return;
        }

        this.data = res.data.data.map((item: ISupportResponse) => this.mapRow(item));
        this.totalCount = res.data.totalCount;
      },
      error: (err: { error?: { message?: string } }) => {
        this.isLoading = false;
        this.toaster.error(err?.error?.message ?? SUPPORT_MESSAGES.error.LOAD_FAILED);
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }

  private mapRow(item: ISupportResponse): ISupportResponse {
    return {
      id: item.id,
      userName: item.userName ?? SUPPORT_DEFAULTS.FALLBACK_TEXT,
      mobile: item.mobile ?? SUPPORT_DEFAULTS.FALLBACK_TEXT,
      email: item.email ?? SUPPORT_DEFAULTS.FALLBACK_TEXT,
      description: item.description ?? SUPPORT_DEFAULTS.FALLBACK_TEXT,
      createdAt: this.formatDate(item.createdAt),
    };
  }

  private formatDate(date: string): string {
    if (!date) return SUPPORT_DEFAULTS.FALLBACK_TEXT;

    const d = new Date(date);
    if (isNaN(d.getTime())) return SUPPORT_DEFAULTS.FALLBACK_TEXT;

    return d.toLocaleDateString(SUPPORT_DEFAULTS.DATE_FORMAT_LOCALE, {
      day: '2-digit',
      month: 'short',
      year: 'numeric',
    });
  }

  openFilter() {
    const data: IFilterPanelData = {
      fields: [
        {
          key: 'userName',
          label: 'User Name',
          type: 'input',
          inputType: 'text',
        },
        {
          key: 'createdDate',
          label: 'Submission Date',
          type: 'date'
        }
      ],
      initialValues: this.activeFilters,
      useMatInput: true
    };

    const dialogRef = this.dialog.open(FilterPanel, {
      data: data,
      position: { right: '0', top: '0' },
      height: '100vh',
      maxWidth: '360px',
      panelClass: 'filter-panel-dialog'
    });

    dialogRef.afterClosed().subscribe((filters: Record<string, any> | null) => {
      if (filters === null) return;

      this.activeFilters = {
        userName: filters['userName']?.trim() || null,
        createdDate: filters['createdDate']
        ? new Date(filters['createdDate']).toLocaleDateString(SUPPORT_DEFAULTS.API_DATE_FORMAT)
        : null
      } as Partial<ISupportFilterRequest>;

      this.pageNumber = 1; 
      this.loadSupportData();
    });
  }

  onPageChanged(event: { pageNumber: number; pageSize: number }) {
    this.pageNumber = event.pageNumber;
    this.pageSize = event.pageSize;
    this.loadSupportData();
  }

  onSortChanged(event: { sortBy: string; sortOrder: string }) {
    this.sort.sortBy = event.sortBy;
    this.sort.sortOrder = event.sortOrder;
    this.pageNumber = 1;
    this.loadSupportData();
  }
}
