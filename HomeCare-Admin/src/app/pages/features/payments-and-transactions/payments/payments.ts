import { Component, OnInit } from '@angular/core';
import { GridLayout } from '../../../../shared/components/grid-layout/grid-layout';
import { IGridColumn } from '../../../../core/models/shared-components/IDataGridModel';
import { IPaymentList } from '../../../../core/models/payments-and-transations/IPaymentList';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { ActivatedRoute } from '@angular/router';
import { PaymentTransactionService } from '../../../../core/services/payments-and-transactions/payment-transaction-service';
import { MatDialog } from '@angular/material/dialog';
import { FilterPanel } from '../../../../shared/components/filter-panel/filter-panel';
import { IFilterPanelData } from '../../../../core/models/shared-components/IFilterPanel';
import { IPaymentFilterRequest } from '../../../../core/models/payments-and-transations/IPaymentFilterRequest';
import { PAYMENT_TRANSACTION_MESSAGES } from '../../../../core/constants/payment-transaction-messages';
import { IExportConfig } from '../../../../core/models/shared-components/IExportConfig';

@Component({
  selector: 'app-payments',
  imports: [GridLayout],
  templateUrl: './payments.html',
  styleUrl: './payments.css',
})
export class Payments implements OnInit {
  totalCount = 0;
  minAmount = 0;
  maxAmount = 0;
  data: IPaymentList[] = [];
  activeFilters: Record<string, any> = {};
  isLoading = true;
  MESSAGES = PAYMENT_TRANSACTION_MESSAGES;

  columns: IGridColumn[] = [
    { header: 'Payment ID', field: 'id', type: 'id', width: '11%', sortable: true },
    { header: 'User', field: 'user', type: 'text', width: '12%', sortable: true },
    {
      header: 'Transaction ID',
      field: 'transactionId',
      type: 'text',
      width: '23%',
      sortable: false,
    },
    { header: 'Booking ID', field: 'bookingId', type: 'id', width: '10%', sortable: true },
    { header: 'Mobile Number', field: 'mobileNumber', type: 'text', width: '13%', sortable: false },
    { header: 'Service', field: 'service', type: 'text', width: '20%', sortable: true },
    {
      header: 'Amount',
      field: 'amount',
      type: 'currency',
      width: '9%',
      sortable: true,
      isPositiveAmount: true,
    },
    {
      header: 'Payment Method',
      field: 'paymentMethod',
      type: 'text',
      width: '12%',
      height: '59px',
      sortable: false,
    },
  ];

  filter: IPaymentFilterRequest = {
    pageNumber: 1,
    pageSize: 10,
    sortBy: '',
    sortOrder: '',
    userName: '',
    minAmount: null as number | null,
    maxAmount: null as number | null,
    paymentMethod: '',
  };

  exportConfig: IExportConfig = {
    csvUrl: 'export/payments/csv',
    pdfUrl: 'export/payments/pdf',
    filename: 'payments',
    queryParams: () => ({
      userName: this.filter.userName,
      minAmount: this.filter.minAmount,
      maxAmount: this.filter.maxAmount,
      paymentMethod: this.filter.paymentMethod,
      sortBy: this.filter.sortBy,
      sortOrder: this.filter.sortOrder,
      pageNumber: this.filter.pageNumber,
      pageSize: this.filter.pageSize,
    }),
  };

  constructor(
    private paymentTransactionService: PaymentTransactionService,
    private toaster: Toaster,
    private dialog: MatDialog,
    private route: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.route.queryParams.subscribe((params) => {
      if (Object.keys(params).length > 0) {
        this.filter.pageNumber = Number(params['pageNumber']) || 1;
        this.filter.pageSize = Number(params['pageSize']) || 10;
        this.filter.sortBy = params['sortBy'] || '';
        this.filter.sortOrder = params['sortOrder'] || '';
        this.filter.userName = params['userName'] || '';
        this.filter.minAmount = params['minAmount'] ? Number(params['minAmount']) : null;
        this.filter.maxAmount = params['maxAmount'] ? Number(params['maxAmount']) : null;
        this.filter.paymentMethod = params['paymentMethod'] || '';

        this.activeFilters = { ...params };
        if (this.activeFilters['minAmount'] || this.activeFilters['maxAmount']) {
          this.activeFilters['amount'] = {
            min: this.filter.minAmount,
            max: this.filter.maxAmount,
          };
        }
      }

      this.loadPayments();
    });
  }

  loadPayments() {
    this.isLoading = true;

    const params: any = {};

    Object.keys(this.filter).forEach((key) => {
      const value = (this.filter as any)[key];
      if (value !== null && value !== '') {
        params[key] = value;
      }
    });

    this.paymentTransactionService.getPayments(params).subscribe({
      next: (res) => {
        this.totalCount = res.data?.totalCount ?? 0;
        this.minAmount = res.data?.minAmount ?? 0;
        this.maxAmount = res.data?.maxAmount ?? 0;

        this.data = (res.data?.data ?? []).map((p: any) => ({
          id: p.id,
          user: p.userName,
          transactionId: p.transactionId,
          bookingId: p.bookingId,
          mobileNumber: p.mobileNumber ?? '-',
          service: p.serviceName,
          amount: p.amount,
          paymentMethod: p.paymentMethod,
        }));
      },
      error: (err) => {
        this.isLoading = false;
        this.toaster.error(err?.error?.message || this.MESSAGES.PAYMENTS.FAILED_PAYMENTS_LOAD);
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }

  onPageChanged(event: { pageNumber: number; pageSize: number }) {
    this.filter.pageNumber = event.pageNumber;
    this.filter.pageSize = event.pageSize;
    this.loadPayments();
  }

  onSortChanged(event: { sortBy: string; sortOrder: string }) {
    this.filter.sortBy = event.sortBy;
    this.filter.sortOrder = event.sortOrder;
    this.filter.pageNumber = 1;
    this.loadPayments();
  }

  openFilter() {
    const data: IFilterPanelData = {
      fields: [
        {
          key: 'userName',
          label: 'User',
          type: 'input',
        },
        {
          key: 'amount',
          label: 'Amount Range',
          type: 'range',
          min: this.minAmount,
          max: this.maxAmount,
          prefix: '$',
        },
        {
          key: 'paymentMethod',
          label: 'Payment Method',
          type: 'select',
          options: [
            { label: 'Cash', value: 'Cash' },
            { label: 'Debit Card', value: 'DebitCard' },
          ],
        },
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
        this.filter.userName = '';
        this.filter.minAmount = null;
        this.filter.maxAmount = null;
        this.filter.paymentMethod = '';
        this.filter.pageNumber = 1;
        this.loadPayments();
        return;
      }

      this.activeFilters = filters!;

      this.filter.userName = filters!['userName'] ?? '';
      this.filter.minAmount = filters!['amount']?.min ?? null;
      this.filter.maxAmount = filters!['amount']?.max ?? null;
      this.filter.paymentMethod = filters!['paymentMethod'] ?? '';
      this.filter.pageNumber = 1;

      this.loadPayments();
    });
  }
}
