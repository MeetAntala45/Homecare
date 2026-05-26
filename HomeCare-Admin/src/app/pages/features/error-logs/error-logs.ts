import { Component, OnInit } from '@angular/core';
import { GridLayout } from '../../../shared/components/grid-layout/grid-layout';
import { IGridColumn } from '../../../core/models/shared-components/IDataGridModel';
import {
  IErrorLogDetail,
  IErrorLogFilter,
  IErrorLogList,
} from '../../../core/models/error-logs/IErrorLog';
import { Toaster } from '../../../../../../HomeCare-Public/src/app/core/services/toaster/toaster';
import { MatDialog } from '@angular/material/dialog';
import { FilterPanel } from '../../../shared/components/filter-panel/filter-panel';
import { IFilterPanelData } from '../../../core/models/shared-components/IFilterPanel';
import { ErrorLogDetailModal } from './error-log-detail-modal/error-log-detail-modal';
import { ActivatedRoute } from '@angular/router';
import { ErrorLogService } from '../../../core/services/error-logs/error-log-service';

@Component({
  selector: 'app-error-logs',
  imports: [GridLayout],
  templateUrl: './error-logs.html',
  styleUrl: './error-logs.css',
})
export class ErrorLogs implements OnInit {
  totalCount = 0;
  data: IErrorLogList[] = [];
  activeFilters: Record<string, any> = {};
  isLoading = true;

  columns: IGridColumn[] = [
    { header: 'ID', field: 'id', type: 'id', width: '6%', sortable: true },
    { header: 'Date & Time', field: 'occurredAt', type: 'text', width: '14%', sortable: true },
    { header: 'Status', field: 'statusCode', type: 'text', width: '7%', sortable: true },
    {
      header: 'Exception Type',
      field: 'exceptionType',
      type: 'text',
      width: '16%',
      sortable: true,
    },
    { header: 'Method', field: 'httpMethod', type: 'text', width: '7%', sortable: false },
    { header: 'Path', field: 'path', type: 'text', width: '22%', sortable: true },
    { header: 'Message', field: 'message', type: 'text', width: '22%', sortable: true },
  ];

  filter: IErrorLogFilter = {
    pageNumber: 1,
    pageSize: 10,
    sortBy: 'occurredAt',
    sortOrder: 'desc',
    fromDate: null,
    toDate: null,
    statusCode: null,
    exceptionType: '',
    search: '',
  };

  constructor(
    private errorLogService: ErrorLogService,
    private toaster: Toaster,
    private dialog: MatDialog,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      if (Object.keys(params).length > 0) {
        this.filter.pageNumber = Number(params['pageNumber']) || 1;
        this.filter.pageSize = Number(params['pageSize']) || 20;
        this.filter.sortBy = params['sortBy'] || 'occurredAt';
        this.filter.sortOrder = params['sortOrder'] || 'desc';
        this.filter.search = params['search'] || '';
        this.filter.exceptionType = params['exceptionType'] || '';
        this.filter.statusCode = params['statusCode'] ? Number(params['statusCode']) : null;
        this.filter.fromDate = params['fromDate'] || null;
        this.filter.toDate = params['toDate'] || null;
        this.activeFilters = { ...params };
      }
      this.loadLogs();
    });
  }

  loadLogs(): void {
    this.isLoading = true;

    const params: any = {};
    Object.keys(this.filter).forEach((key) => {
      const value = (this.filter as any)[key];
      if (value !== null && value !== '') params[key] = value;
    });

    this.errorLogService.getLogs(params).subscribe({
      next: (res) => {
        this.totalCount = res.data?.totalCount ?? 0;
        this.data = (res.data?.data ?? []).map((e) => ({
          ...e,
          occurredAt: this.formatDate(e.occurredAt),
          message: e.message.length > 80 ? e.message.slice(0, 80) + '…' : e.message,
        }));
      },
      error: (err) => {
        this.isLoading = false;
        this.toaster.error(err?.error?.message || 'Failed to load error logs.');
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }

  onPageChanged(e: { pageNumber: number; pageSize: number }): void {
    this.filter.pageNumber = e.pageNumber;
    this.filter.pageSize = e.pageSize;
    this.loadLogs();
  }

  onSortChanged(e: { sortBy: string; sortOrder: string }): void {
    this.filter.sortBy = e.sortBy;
    this.filter.sortOrder = e.sortOrder;
    this.filter.pageNumber = 1;
    this.loadLogs();
  }

  onRowClicked(row: IErrorLogList): void {
    this.errorLogService.getById(row.id).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.dialog.open(ErrorLogDetailModal, {
            width: '860px',
            maxWidth: '95vw',
            maxHeight: '90vh',
            data: res.data,
            panelClass: 'error-log-modal-panel',
          });
        }
      },
      error: () => this.toaster.error('Failed to load error details.'),
    });
  }

  openFilter(): void {
    const data: IFilterPanelData = {
      fields: [
        { key: 'search', label: 'Search (message / path)', type: 'input' },
        { key: 'exceptionType', label: 'Exception Type', type: 'input' },
        { key: 'statusCode', label: 'Status Code', type: 'input' },
        { key: 'fromDate', label: 'From Date', type: 'date', allowFuture: false },
        { key: 'toDate', label: 'To Date', type: 'date', allowFuture: false },
      ],
      initialValues: this.activeFilters,
    };

    const dialogRef = this.dialog.open(FilterPanel, {
      data: { ...data, useMatInput: true },
      position: { right: '0', top: '0' },
      height: '100vh',
      maxWidth: '360px',
      panelClass: 'filter-panel-dialog',
    });

    dialogRef.afterClosed().subscribe((filters: Record<string, any> | null | 'reset') => {
      if (filters === undefined) return;

      if (filters === 'reset') {
        this.activeFilters = {};
        this.filter.search = '';
        this.filter.exceptionType = '';
        this.filter.statusCode = null;
        this.filter.fromDate = null;
        this.filter.toDate = null;
        this.filter.pageNumber = 1;
        this.loadLogs();
        return;
      }

      this.activeFilters = filters!;
      this.filter.search = filters!['search'] ?? '';
      this.filter.exceptionType = filters!['exceptionType'] ?? '';
      this.filter.statusCode = filters!['statusCode'] ? Number(filters!['statusCode']) : null;

      const rawFrom = filters!['fromDate'];
      const rawTo = filters!['toDate'];
      this.filter.fromDate = rawFrom ? new Date(rawFrom).toISOString() : null;
      this.filter.toDate = rawTo ? new Date(rawTo).toISOString() : null;

      this.filter.pageNumber = 1;
      this.loadLogs();
    });
  }

  private formatDate(raw: string): string {
    if (!raw) return '—';
    const d = new Date(raw);
    const pad = (n: number) => String(n).padStart(2, '0');
    return (
      `${pad(d.getDate())} ${d.toLocaleString('en', { month: 'short' })} ${d.getFullYear()} ` +
      `${pad(d.getHours())}:${pad(d.getMinutes())}`
    );
  }
}
