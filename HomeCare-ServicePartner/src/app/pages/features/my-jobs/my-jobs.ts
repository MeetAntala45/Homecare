import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { GridLayout } from '../../../shared/components/grid-layout/grid-layout';
import { IGridColumn } from '../../../core/models/shared-components/IDataGridModel';
import { MyJobsService } from '../../../core/services/my-jobs/my-jobs';
import { Toaster } from '../../../core/services/toaster/toaster';
import { IMyJobs, IMyJobCalendarItem } from '../../../core/models/my-jobs/my-jobs';
import { JobsCalendar } from '../../../shared/components/jobs-calendar/jobs-calendar';
import { MatDialog } from '@angular/material/dialog';
import { FilterPanel } from '../../../shared/components/filter-panel/filter-panel';
import { ActivatedRoute, Router } from '@angular/router';
import { Subscription } from 'rxjs';
import { PartnerSignalRService } from '../../../core/services/notifications/partner-signalr-service';
import { LocationSenderService } from '../../../core/services/tracking-partner/location-sender-service';
import { MY_JOBS_VIEW_MODE } from '../../../core/constants/environment-config';

@Component({
  selector: 'app-my-jobs',
  standalone: true,
  imports: [CommonModule, GridLayout, JobsCalendar],
  templateUrl: './my-jobs.html',
  styleUrl: './my-jobs.css',
})
export class MyJobs implements OnInit {

  isLoading = false;
  isCalendarLoading = false;
  activeTrackingBookingId: number | null = null;
  private readonly partnerSignalR = inject(PartnerSignalRService);
  private signalRSub!: Subscription;

  get partnerId(): number {
    return Number(localStorage.getItem('partner_id'));
  }
  data: IMyJobs[] = [];
  highlightBookingId: number | null = null;
  originalMinAmount: number = 0;
  originalMaxAmount: number = 10000;

  filter = {
    pageNumber: 1,
    pageSize: 10,
    sortBy: '',
    sortOrder: '',
    serviceName: '',
    customerName: '',
    bookingDate: '',
    paymentMethod: '',
    minAmount: null as number | null,
    maxAmount: null as number | null
  };

  activeFilters: Record<string, any> = {};
  minAmount: number = 0;
  maxAmount: number = 10000;

  serviceOptions: { label: string; value: string }[] = [];

  status: 'pending' | 'completed' = 'pending';
  sort = { sortBy: 'Id', sortOrder: 'desc' };

  totalCount = 0;
  pageNumber = 1;
  pageSize = 10;
  scrollTrigger = 0;
  activeStatus: string = 'pending';

  calendarJobs: IMyJobCalendarItem[] = [];

  calendarYear = new Date().getFullYear();
  calendarMonth = new Date().getMonth() + 1;
  highlightStatus: 'pending' | 'completed' | null = null;

  constructor(
    private my_jobs: MyJobsService,
    private toaster: Toaster,
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private router: Router,
    private locationSender: LocationSenderService
  ) { }

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (params['highlightBookingId']) {
        const bookingId = +params['highlightBookingId'];
        const status = (params['status'] ?? 'pending') as 'pending' | 'completed';

        this.status = status;

        this.router.navigate([], {
          relativeTo: this.route,
          queryParams: {},
          replaceUrl: true
        });

        this.my_jobs.getBookingPage(
          bookingId,
          this.filter.pageSize,
          status
        ).subscribe({
          next: (res) => {
            this.filter.pageNumber = res.data ?? 1;

            this.loadJobs(() => {
              const currentStatus = this.status;
              setTimeout(() => {
                this.highlightBookingId = bookingId;
                this.highlightStatus = currentStatus;
                setTimeout(() => {
                  this.highlightBookingId = null;
                  this.highlightStatus = null;
                }, 3500);
              }, 100);
            });
          },
          error: () => {
            this.filter.pageNumber = 1;
            this.loadJobs(() => {
              setTimeout(() => {
                this.highlightBookingId = bookingId;
                this.highlightStatus = status;
                setTimeout(() => {
                  this.highlightBookingId = null;
                  this.highlightStatus = null;
                }, 3500);
              }, 100);
            });
          }
        });

      } else {
        if (this.initialViewMode === 'calendar') {
          this.loadCalendarJobs();
        } else {
          this.loadJobs();
        }
      }
      this.loadServiceOptions();
    });

    this.listenToSignalR();
  }

  ngOnDestroy(): void {
    this.signalRSub?.unsubscribe();
  }

  private listenToSignalR(): void {
    this.signalRSub = this.partnerSignalR.newBookingAssigned$.subscribe(
      (notification) => {
        const targetStatus = 'pending';

        if (this.status !== targetStatus) {
          this.status = targetStatus;
        }

        this.my_jobs.getBookingPage(
          notification.bookingId,
          this.filter.pageSize,
          targetStatus
        ).subscribe({
          next: (res) => {
            this.filter.pageNumber = res.data ?? 1;

            this.loadJobs(() => {
              const currentStatus = this.status;
              setTimeout(() => {
                this.highlightBookingId = notification.bookingId;
                this.highlightStatus = currentStatus;
                setTimeout(() => {
                  this.highlightBookingId = null;
                  this.highlightStatus = null;
                }, 3500);
              }, 100);
            });
          },
          error: () => {
            this.filter.pageNumber = 1;
            this.loadJobs();
          }
        });
      }
    );
  }

  onListClicked() {
    this.loadJobs();
  }

  initialViewMode: 'list' | 'calendar' =
    (sessionStorage.getItem(MY_JOBS_VIEW_MODE) as 'list' | 'calendar') ?? 'list';

  columns: IGridColumn[] = [
    { header: 'Service Name', field: 'serviceName', width: '180px', type: 'text', sortable: true },
    { header: 'Booking Date', field: 'bookingDate', width: '140px', type: 'text', sortable: true },
    { header: 'Slot Time', field: 'slotTime', width: '130px', type: 'text' },
    { header: 'Customer Name', field: 'customerName', width: '180px', type: 'text', sortable: true },
    { header: 'Address', field: 'address', width: '240px', type: 'text' },
    { header: 'Amount', field: 'amount', width: '120px', type: 'currency', sortable: true },
    { header: 'Payment Method', field: 'paymentMethod', width: '120px', height: '59px', type: 'text' },
  ];

  onViewModeChanged(mode: 'list' | 'calendar') {
    sessionStorage.setItem(MY_JOBS_VIEW_MODE, mode);
    if (mode === 'calendar') {
      this.loadCalendarJobs();
    }
  }

  loadJobs(callback?: () => void): void {
    this.isLoading = true;

    const params: any = {};
    Object.keys(this.filter).forEach(key => {
      const value = (this.filter as any)[key];
      if (value !== null && value !== '') {
        params[key] = value;
      }
    });

    params.status = this.status;
    params.partnerId = this.partnerId;

    this.my_jobs.getBookingsByPartnerId(params).subscribe({
      next: (res) => {
        this.data = (res.data.data ?? []).map((item: IMyJobs) => ({
          ...item,
          actions: (item.bookingStatus === 'Pending' || item.bookingStatus === 'InProgress')
            ? [{
              label: this.activeTrackingBookingId === item.bookingId ? 'Stop Sharing' : 'Share Location',
              action: 'track',
              icon: this.activeTrackingBookingId === item.bookingId ? 'bi-broadcast-pin' : 'bi-broadcast'
            }]
            : []
        }));
        this.totalCount = res.data.totalCount;
        this.pageNumber = res.data.pageNumber;
        this.pageSize = res.data.pageSize;
        this.maxAmount = res.data.maxAmount;
        this.minAmount = res.data.minAmount;

        if (this.originalMinAmount === 0 && this.originalMaxAmount === 10000) {
          this.originalMinAmount = res.data.minAmount;
          this.originalMaxAmount = res.data.maxAmount;
        }

        this.isLoading = false;
        callback?.();
      },
      error: (err) => {
        this.toaster.error(err?.error?.message || 'Failed to load jobs');
        this.isLoading = false;
      }
    });
  }


  loadServiceOptions() {
    this.my_jobs.getAllServices().subscribe(res => {
      this.serviceOptions = (res.data || []).map((s: string) => ({
        label: s,
        value: s
      }));
    });


  }

  loadCalendarJobs(): void {
    this.isCalendarLoading = true;
    this.my_jobs.getCalendarJobs({ year: this.calendarYear, month: this.calendarMonth }).subscribe({
      next: (res) => {
        this.calendarJobs = res.data ?? [];
        this.isCalendarLoading = false;
      },
      error: (err) => {
        this.toaster.error(err?.error?.message || 'Failed to load calendar');
        this.isCalendarLoading = false;
      }
    });
  }

  prevMonth(): void {
    if (this.calendarMonth === 1) { this.calendarMonth = 12; this.calendarYear--; }
    else { this.calendarMonth--; }
    this.loadCalendarJobs();
  }

  nextMonth(): void {
    if (this.calendarMonth === 12) { this.calendarMonth = 1; this.calendarYear++; }
    else { this.calendarMonth++; }
    this.loadCalendarJobs();
  }

  goToToday() {
    const today = new Date();

    this.calendarYear = today.getFullYear();
    this.calendarMonth = today.getMonth() + 1;

    this.triggerScrollToToday();
    this.loadCalendarJobs();
  }

  triggerScrollToToday() {
    this.scrollTrigger++;
  }

  onPageChanged(event: { pageNumber: number; pageSize: number }) {
    this.filter.pageNumber = event.pageNumber;
    this.filter.pageSize = event.pageSize;
    this.loadJobs();
  }

  onSortChanged(event: { sortBy: string; sortOrder: string }) {
    this.filter.sortBy = event.sortBy;
    this.filter.sortOrder = event.sortOrder;
    this.filter.pageNumber = 1;
    this.loadJobs();
  }

  onStatusChange(status: 'pending' | 'completed') {
    if (this.status === status) return;
    this.status = status;
    this.activeFilters = {};
    this.originalMinAmount = 0;
    this.originalMaxAmount = 10000;

    this.filter = {
      pageNumber: 1,
      pageSize: this.pageSize,
      sortBy: this.filter.sortBy,
      sortOrder: this.filter.sortOrder,
      serviceName: '',
      customerName: '',
      bookingDate: '',
      paymentMethod: '',
      minAmount: null,
      maxAmount: null,

    };

    this.loadJobs();
  }

  openFilter() {
    const data = {
      fields: [
        {
          key: 'customerName',
          label: 'Customer Name',
          type: 'input',
        },
        {
          key: 'serviceName',
          label: 'Service Name',
          type: 'select',
          options: this.serviceOptions,
        },
        {
          key: 'bookingDate',
          label: 'Booking Date',
          type: 'date',
        },
        {
          key: 'amount',
          label: 'Amount Range',
          type: 'range',
          min: this.originalMinAmount,
          max: this.originalMaxAmount,
          prefix: '$'
        }, {
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
        this.filter.serviceName = '';
        this.filter.customerName = '';
        this.filter.bookingDate = '';
        this.filter.minAmount = null;
        this.filter.maxAmount = null;
        this.filter.pageNumber = 1;
        this.filter.paymentMethod = '';
        this.loadJobs();
        return;
      }
      this.activeFilters = filters!;

      this.filter.serviceName = filters!['serviceName'] ?? '';
      this.filter.customerName = filters!['customerName'] ?? '';
      this.filter.bookingDate = filters!['bookingDate'] ?? '';
      this.filter.minAmount = filters!['amount']?.min ?? null;
      this.filter.maxAmount = filters!['amount']?.max ?? null;
      this.filter.paymentMethod = filters!['paymentMethod'] ?? '';
      this.filter.pageNumber = 1;

      this.loadJobs();
    });
  }

  private trackingToggleLock = false;

  toggleTracking(bookingId: number): void {
    if (this.trackingToggleLock) return; 
    this.trackingToggleLock = true;
    setTimeout(() => this.trackingToggleLock = false, 500);
      
    if (this.locationSender.isTracking() && this.activeTrackingBookingId === bookingId) {
      this.locationSender.stopTracking();
      this.activeTrackingBookingId = null;
      this.toaster.success('Location sharing stopped.');
    } else {
      if (this.locationSender.isTracking()) {
        this.locationSender.stopTracking();
      }
      this.locationSender.startTracking(bookingId);
      this.activeTrackingBookingId = bookingId;
      this.toaster.success('Sharing your location with customer.');
    }

    this.data = this.data.map(item => ({
      ...item,
      actions: (item.bookingStatus === 'Pending' || item.bookingStatus === 'InProgress')
        ? [{
          label: this.activeTrackingBookingId === item.bookingId ? 'Stop Sharing' : 'Share Location',
          action: 'track',
          icon: this.activeTrackingBookingId === item.bookingId ? 'bi-broadcast-pin' : 'bi-broadcast'
        }]
        : []
    }));
  }

  onActionClicked(event: { action: string; row: any }): void {
    if (event.action === 'track') {
      this.toggleTracking(event.row.bookingId);
    }
  }
}