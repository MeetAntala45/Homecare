import { Component, inject, OnInit } from '@angular/core';
import { DatePipe, Location } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { PaymentTransactionService } from '../../../../core/services/payments-and-transactions/payment-transaction-service';
import { FilterPanel } from '../../../../shared/components/filter-panel/filter-panel';
import { GridLayout } from '../../../../shared/components/grid-layout/grid-layout';
import { IUserPaymentDetail } from '../../../../core/models/payments-and-transations/IUserPaymentDetail';
import { IUserPaymentList } from '../../../../core/models/payments-and-transations/IUserPaymentList';
import { IUserPaymentFilterRequest } from '../../../../core/models/payments-and-transations/IUserPaymentFilterRequest';
import { IGridColumn } from '../../../../core/models/shared-components/IDataGridModel';
import { IFilterPanelData } from '../../../../core/models/shared-components/IFilterPanel';
import { MatTooltip } from '@angular/material/tooltip';
import { PAYMENT_TRANSACTION_MESSAGES } from '../../../../core/constants/payment-transaction-messages';
import { TimezonePipe } from '../../../../shared/pipes/timezone/timezone-pipe';
import { IExportConfig } from '../../../../core/models/shared-components/IExportConfig';

@Component({
  selector: 'app-payment-detail',
  imports: [DatePipe, GridLayout, MatTooltip, TimezonePipe],
  templateUrl: './payment-detail.html',
  styleUrl: './payment-detail.css',
})
export class PaymentDetail implements OnInit {

  loading = false;
  isLoading = true;
  MESSAGES = PAYMENT_TRANSACTION_MESSAGES;

  userPayment: IUserPaymentDetail | null = null;
  data: IUserPaymentList[] = [];
  totalCount = 0;
  minAmount = 0;
  maxAmount = 0;
  activeFilters: Record<string, any> = {};

  filter: IUserPaymentFilterRequest = {
    pageNumber: 1,
    pageSize: 5,
    sortBy: '',
    sortOrder: '',
    userId: 0,
    currentPaymentId: 0,
    minAmount: null as number | null,
    maxAmount: null as number | null,
    paymentMethod: ''
  };

  columns: IGridColumn[] = [
    { header: 'Payment ID', field: 'id', type: 'text', width: '15%', sortable: true},
    { header: 'Transaction ID', field: 'transactionId', type: 'text', width: '28%', sortable: false },
    { header: 'Booking ID', field: 'bookingId', type: 'text', width: '15%', sortable: true },
    { header: 'Service', field: 'service', type: 'text', width: '25%', sortable: true },
    { header: 'Amount', field: 'amount', type: 'currency', width: '15%', sortable: true, isPositiveAmount: true },
    { header: 'Payment Method', field: 'paymentMethod', type: 'text', width: '12%', height: '59px', sortable: false },
  ];

  exportConfig!: IExportConfig;
  userId: number = 0;
  userName: string = "user";
  location = inject(Location);

  constructor(
    private paymentTransactionService: PaymentTransactionService,
    private toaster: Toaster,
    private route: ActivatedRoute,
    private dialog: MatDialog,
    private router: Router,
  ) {}
  
  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      
      if (id) {
        this.filter.currentPaymentId = id;
        this.loadUserPaymentDetail(id);
      }
    });
  }
  
  loadUserPaymentDetail(id: number) {
    this.loading = true;
    
    this.paymentTransactionService.getUserPaymentDetail(id).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.userPayment = res.data;
          this.userName = this.userPayment.userName.toLowerCase();

          this.exportConfig = {
            csvUrl: 'export/user-payments/csv',
            pdfUrl: 'export/user-payments/pdf',
            filename: `${this.userName} payments`,
            queryParams: () => ({
              userId: this.filter.userId,
              currentPaymentId: this.filter.currentPaymentId,
              minAmount: this.filter.minAmount,
              maxAmount: this.filter.maxAmount,
              paymentMethod: this.filter.paymentMethod,
              sortBy: this.filter.sortBy,
              sortOrder: this.filter.sortOrder, 
              pageNumber: this.filter.pageNumber,
              pageSize: this.filter.pageSize,
            })
          };

          this.filter.userId = res.data.userId;
          this.loadUserPayments();
        } else {
          this.toaster.error(res.message);
        }
      },
      error: (err) => {
        this.toaster.error(err?.error?.message || this.MESSAGES.PAYMENT_DETAIL.FAILED_USER_DETAILS_LOAD);
      },
      complete: () => {
        this.loading = false;
      }
    });
  }
  
  loadUserPayments() { 
    this.isLoading = true;

    const params: any = {};
    
    Object.keys(this.filter).forEach(key => {
      const value = (this.filter as any)[key];
      if (value !== null && value !== '') {
        params[key] = value;
      }
    });
    
    this.paymentTransactionService.getUserPayments(params).subscribe({
      next: (res) => {
        this.totalCount = res.data?.totalCount ?? 0;
        this.minAmount = res.data?.minAmount ?? 0;
        this.maxAmount = res.data?.maxAmount ?? 0;
        
        this.data = (res.data?.data ?? []).map((p: any) => ({
          id: p.id,
          transactionId: p.transactionId,
          bookingId: p.bookingId,
          service: p.serviceName,
          amount: p.amount,
          paymentMethod: p.paymentMethod
        }));
      },
      error: (err) => {
        this.isLoading = false;
        this.toaster.error(err?.error?.message || this.MESSAGES.PAYMENT_DETAIL.FAILED_TRANSACTION_LOAD);
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }
  
  goBack(): void {
    const currentQueryParams = this.route.snapshot.queryParams;

    this.router.navigate(['/admin/payments'], { 
      queryParams: currentQueryParams 
    });
  }

  onRowClicked(row: any): void {
    const currentQueryParams = this.route.snapshot.queryParams;

    this.filter.currentPaymentId = row.id;
    this.router.navigate(['/admin/payments', row.id], {
      queryParams: currentQueryParams
    });
  }

  onPageChanged(event: { pageNumber: number; pageSize: number }) {
    this.filter.pageNumber = event.pageNumber;
    this.filter.pageSize = event.pageSize;
    this.loadUserPayments();
  }

  onSortChanged(event: { sortBy: string; sortOrder: string }) {
    this.filter.sortBy = event.sortBy;
    this.filter.sortOrder = event.sortOrder;
    this.filter.pageNumber = 1;
    this.loadUserPayments();
  }

  openFilter() {
    const data: IFilterPanelData = {
      fields: [
        {
          key: 'amount',
          label: 'Amount Range',
          type: 'range',
          min: this.minAmount,
          max: this.maxAmount || 1,
          prefix: '$'
        },
        {
          key: 'paymentMethod',
          label: 'Payment Method',
          type: 'select',
          options: [
            { label: 'Cash', value: 'Cash' },
            { label: 'Debit Card', value: 'DebitCard' }
          ]
        }
      ],
      initialValues: this.activeFilters
    };

    const dialogRef = this.dialog.open(FilterPanel, {
      data: { ...data, useMatInput: true },
      position: { right: '0', top: '0' },
      height: '100vh',
      maxWidth: '360px',
      panelClass: 'filter-panel-dialog'
    });

    dialogRef.afterClosed().subscribe((filters: Record<string, any> | null | 'reset') => {
      if (filters === undefined) return;

      if (filters === 'reset') {
        this.activeFilters = {};
        this.filter.minAmount = null;
        this.filter.maxAmount = null;
        this.filter.paymentMethod = '';
        this.filter.pageNumber = 1;
        this.loadUserPayments();
        return;
      }

      this.activeFilters = filters!;
      this.filter.minAmount = filters!['amount']?.min ?? null;
      this.filter.maxAmount = filters!['amount']?.max ?? null;
      this.filter.paymentMethod = filters!['paymentMethod'] ?? '';
      this.filter.pageNumber = 1;
      this.loadUserPayments();
    });
  }

}