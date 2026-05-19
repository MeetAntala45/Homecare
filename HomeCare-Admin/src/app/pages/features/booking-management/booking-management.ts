import { Component, EventEmitter, Input, Output, OnInit, OnDestroy, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { Subscription } from 'rxjs';

import { GridLayout } from '../../../shared/components/grid-layout/grid-layout';
import { FilterPanel } from '../../../shared/components/filter-panel/filter-panel';
import { BookingDetailGrid } from './booking-detail-grid/booking-detail-grid';
import { DeleteConfirmation } from '../../../shared/components/delete-confirmation/delete-confirmation';

import { CustomerBookingService } from '../../../core/services/booking-management/booking-management-service';
import { Toaster } from '../../../core/services/toaster/toaster';

import { IGridColumn } from '../../../core/models/shared-components/IDataGridModel';
import { IFilterPanelData } from '../../../core/models/shared-components/IFilterPanel';
import { BOOKING_MANAGEMENT_MESSAGES } from '../../../core/constants/booking-management-messages';
import {
  IActiveBookingFilters,
  IBookingGridFilter,
  IDropdownOption,
} from '../../../core/models/booking-management/booking-managment-filter';
import { ICustomerBookingSummary } from '../../../core/models/booking-management/customer-booking-summary';
import { BookingSignalRService } from '../../../core/services/signalr/booking-signalr-service';
import { HttpParams } from '@angular/common/http';

@Component({
  selector: 'app-booking-management',
  standalone: true,
  imports: [GridLayout, BookingDetailGrid],
  templateUrl: './booking-management.html',
  styleUrl: './booking-management.css',
})
export class BookingManagement implements OnInit, OnDestroy {
  private readonly bookingService = inject(CustomerBookingService);
  private readonly signalR = inject(BookingSignalRService);
  private readonly toaster = inject(Toaster);
  private readonly dialog = inject(MatDialog);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  readonly MESSAGES = BOOKING_MANAGEMENT_MESSAGES;

  private signalRSub!: Subscription;
  private queryParamSub!: Subscription;

  isLoading = true;
  serviceTypeOptions: IDropdownOption[] = [];

  expandedCustomerId: number | null = null;
  expandedPaymentMethod: number | null = null;
  expandTriggered: boolean = false;

  highlightRowKey: string | null = null;
  highlightBookingId: number | null = null;
  forceReloadKey: number = 0;

  columns: IGridColumn[] = [
    { field: 'customerName', header: 'User', width: '11%', sortable: true },
    { field: 'mobileNumber', header: 'Mobile Number', width: '12%', sortable: false },
    { field: 'email', header: 'Email', width: '18%', sortable: false },
    { field: 'totalBookedServices', header: 'Booked Services', width: '12%', sortable: true },
    { field: 'address', header: 'Address', width: '18%', sortable: false },
    {
      field: 'totalBookingAmount',
      header: 'Amount',
      width: '10%',
      sortable: true,
      type: 'currency',
    },
    { field: 'paymentMethod', header: 'Payment Method', width: '11%', sortable: false },
  ];

  filter: IBookingGridFilter = {
    pageNumber: 1,
    pageSize: 10,
    userName: '',
    sortBy: 'customerName',
    sortOrder: 'asc',
    serviceType: '',
    fromDate: null,
    time: null,
    paymentMethod: null,
    bookingStatus: null,
    minBookings: null,
    maxBookings: null,
    minAmount: null,
    maxAmount: null,
  };

  sharedFilters: IActiveBookingFilters = {};
  activeFilters: IActiveBookingFilters = {};
  data: ICustomerBookingSummary[] = [];
  totalCount: number = 0;
  minBookedServices: number = 0;
  maxBookedServices: number = 0;
  minAmount: number = 0;
  maxAmount: number = 0;

  ngOnInit(): void {
    this.loadServiceTypes();
    this.listenToSignalR();
    this.handleQueryParams();
  }

  ngOnDestroy(): void {
    this.signalRSub?.unsubscribe();
    this.queryParamSub?.unsubscribe();
  }

  private navigateToCustomer(
    customerId: number,
    paymentMethod: number,
    bookingId: number | null = null
  ): void {
    this.expandedCustomerId = customerId;
    this.expandedPaymentMethod = paymentMethod;
    this.expandTriggered = false;
    this.highlightRowKey = null;
    this.highlightBookingId = null;

    this.bookingService
      .getCustomerPage({
        customerId,
        paymentMethod,
        pageSize: this.filter.pageSize,
      })
      .subscribe({
        next: (res) => {
          this.filter.pageNumber = res.data ?? 1;
          this.loadData(() => {
            this.forceReloadKey++;
            this.expandTriggered = true;
            this.highlightRowKey = `${customerId}_${paymentMethod}`;
            setTimeout(() => {
              this.highlightBookingId = bookingId;
            });

            setTimeout(() => {
              this.expandTriggered = false;
              setTimeout(() => {
                this.highlightRowKey = null;
                this.highlightBookingId = null;
              }, 2000);
            }, 500);
          });
        },
        error: () => {
          this.filter.pageNumber = 1;
          this.loadData(() => {
            this.expandTriggered = true;
            this.highlightRowKey = `${customerId}_${paymentMethod}`;
            this.highlightBookingId = bookingId;

            setTimeout(() => {
              this.expandTriggered = false;
              setTimeout(() => {
                this.highlightRowKey = null;
                this.highlightBookingId = null;
              }, 2500);
            }, 500);
          });
        },
      });
  }

  private handleQueryParams(): void {
    this.queryParamSub = this.route.queryParams.subscribe((params) => {

      this.filter.pageNumber = +params['pageNumber'] || 1;
      this.filter.pageSize = +params['pageSize'] || 10;
      const customerId = params['expandCustomerId'];
      const paymentMethod = params['expandPaymentMethod'];
      const resetFilters = params['resetFilters'];
      const bookingId = params['highlightBookingId'];

      if (resetFilters) {
        this.resetFilter();
      }
      if (!customerId || !paymentMethod) {
        this.loadData();
        return;
      }

      this.router.navigate([], {
        relativeTo: this.route,
        queryParams: {},
        replaceUrl: true,
      });

      this.navigateToCustomer(
        Number(customerId),
        Number(paymentMethod),
        bookingId ? Number(bookingId) : null
      );
    });
  }

  private listenToSignalR(): void {
    this.signalRSub = this.signalR.newBooking$.subscribe((notification) => {
      this.loadData();
    });
  }

  private loadServiceTypes(): void {
    this.isLoading = true;
    this.bookingService.getServiceTypes().subscribe({
      next: (res) => {
        this.serviceTypeOptions = res.data ?? [];
      },
      error: () => {
        this.isLoading = false;
        this.toaster.error(this.MESSAGES.LOAD_SERVICE_TYPES_FAILED);
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  loadData(callback?: () => void): void {
    this.isLoading = true;

    let params = new HttpParams()
      .set('pageNumber', this.filter.pageNumber)
      .set('pageSize', this.filter.pageSize)
      .set('sortBy', this.filter.sortBy)
      .set('sortOrder', this.filter.sortOrder);

    if (this.filter.userName) params = params.set('userName' , this.filter.userName);

    if (this.filter.serviceType) params = params.set('serviceType', this.filter.serviceType);

    if (this.filter.fromDate) params = params.set('fromDate', this.filter.fromDate);

    if (this.filter.time) params = params.set('fromTime', this.filter.time);

    if (this.filter.paymentMethod != null)
      params = params.set('paymentMethod', this.filter.paymentMethod);

    if (this.filter.bookingStatus != null)
      params = params.set('bookingStatus', this.filter.bookingStatus);

    if (this.filter.minBookings != null)
      params = params.set('minBookings', this.filter.minBookings);

    if (this.filter.maxBookings != null)
      params = params.set('maxBookings', this.filter.maxBookings);

    if (this.filter.minAmount != null) params = params.set('minAmount', this.filter.minAmount);

    if (this.filter.maxAmount != null) params = params.set('maxAmount', this.filter.maxAmount);

    this.bookingService.getCustomerSummaries(params).subscribe({
      next: (res) => {
        if (!res.success) {
          this.toaster.error(res.message);
          return;
        }
        this.totalCount = res.data!.totalCount;
        this.data = res.data!.data;
        this.minBookedServices = res.data!.minBookedServices;
        this.maxBookedServices = res.data!.maxBookedServices;
        this.minAmount = res.data!.minAmount;
        this.maxAmount = res.data!.maxAmount;

        setTimeout(() => {
          callback?.();
        });
      },
      error: (err: any) => {
        this.isLoading = false;
        this.toaster.error(err?.error?.message ?? this.MESSAGES.LOAD_FAILED);
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  onSortChanged(event: { sortBy: string; sortOrder: string }): void {
    this.filter.sortBy = event.sortBy;
    this.filter.sortOrder = event.sortOrder;
    this.filter.pageNumber = 1;
    this.loadData();
  }

  onPageChanged(event: { pageNumber: number; pageSize: number }): void {
    this.filter.pageNumber = event.pageNumber;
    this.filter.pageSize = event.pageSize;
  
    this.router.navigate([], {
      queryParams: {
        pageNumber: this.filter.pageNumber,
        pageSize: this.filter.pageSize
      },
      queryParamsHandling: 'merge'
    });
  
    this.loadData();
  }
  onDeleteCustomer(row: ICustomerBookingSummary): void {
    this.dialog
      .open(DeleteConfirmation, {
        width: '400px',
        disableClose: true,
        data: {
          message: `Are you sure you want to delete all cancelled bookings for <strong>${row.customerName}</strong>?`,
          apiCall: () => {
            const params = new HttpParams().set('paymentMethodValue', row.paymentMethodValue);
            return this.bookingService.deleteCustomerBookings(row.customerId, params);
          },
        },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result) {
          this.forceReloadKey++; 
          this.loadData();
        }
      });
  }

  openFilter(): void {
    const filterData: IFilterPanelData = {
      fields: [
        {
          key: 'userName',
          label: 'User',
          type: 'input',
        },
        {
          key: 'serviceType',
          label: 'Service Type',
          type: 'select',
          options: this.serviceTypeOptions,
        },
        { key: 'date', label: 'Date', type: 'date', allowFuture: true },
        { key: 'time', label: 'Time', type: 'time' },
        {
          key: 'bookedServices',
          label: 'Booked Services',
          type: 'range',
          min: this.minBookedServices,
          max: this.maxBookedServices,
        },
        { key: 'amount', label: 'Amount', type: 'range', min: this.minAmount, max: this.maxAmount },
        {
          key: 'paymentMethod',
          label: 'Payment Method',
          type: 'select',
          options: [
            { label: 'Cash', value: 1 },
            { label: 'Debit Card', value: 2 },
          ],
        },
        {
          key: 'bookingStatus',
          label: 'Status',
          type: 'select',
          options: [
            { label: 'Pending', value: 1 },
            { label: 'Completed', value: 2 },
            { label: 'Cancelled', value: 3 },
            { label: 'InProgress', value: 4 },
          ],
        },
      ],
      initialValues: this.activeFilters,
    };

    this.dialog
      .open(FilterPanel, {
        data: { ...filterData, useMatInput: true },
        position: { right: '0', top: '0' },
        height: '100vh',
        maxWidth: '360px',
        panelClass: 'filter-panel-dialog',
      })
      .afterClosed()
      .subscribe((result: Record<string, any> | null | 'reset') => {
        if (result === undefined) return;
        if (result === 'reset') {
          this.activeFilters = {};
          this.sharedFilters = {};
          this.resetFilter();
        } else {
          this.activeFilters = result!;
          this.applyFilter(result!);
        }
        this.filter.pageNumber = 1;
        this.loadData();
      });
  }

  private applyFilter(f: Record<string, any>): void {
    const rawDate = f['date'];
    let dateStr: string | null = null;
    if (rawDate) {
      const d = new Date(rawDate);
      dateStr = `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(
        d.getDate()
      ).padStart(2, '0')}`;
    }
    this.filter.userName = f['userName'] ?? '';
    this.filter.serviceType = f['serviceType'] ?? '';
    this.filter.fromDate = dateStr;
    this.filter.time = f['time'] ?? null;
    this.filter.paymentMethod = f['paymentMethod'] ?? null;
    this.filter.bookingStatus = f['bookingStatus'] ?? null;
    this.filter.minBookings = f['bookedServices']?.min ?? null;
    this.filter.maxBookings = f['bookedServices']?.max ?? null;
    this.filter.minAmount = f['amount']?.min ?? null;
    this.filter.maxAmount = f['amount']?.max ?? null;
    this.sharedFilters = {
      serviceType: f['serviceType'] ?? '',
      date: dateStr,
      time: f['time'] ?? null,
      bookingStatus: f['bookingStatus'] ?? null,
    };
  }

  private resetFilter(): void {
    this.filter.userName = '';
    this.filter.serviceType = '';
    this.filter.fromDate = null;
    this.filter.time = null;
    this.filter.paymentMethod = null;
    this.filter.bookingStatus = null;
    this.filter.minBookings = null;
    this.filter.maxBookings = null;
    this.filter.minAmount = null;
    this.filter.maxAmount = null;
  }
}
