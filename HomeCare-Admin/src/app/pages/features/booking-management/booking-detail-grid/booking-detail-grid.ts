import {
  Component,
  Input,
  OnInit,
  OnChanges,
  SimpleChanges,
  inject,
  Output,
  EventEmitter,
} from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';

import { DataGrid } from '../../../../shared/components/data-grid/data-grid';
import { ChangeExpert } from '../../../../shared/components/change-expert/change-expert';
import { CancelBooking } from '../../../../shared/components/cancel-booking/cancel-booking';
import { CompleteBooking } from '../../../../shared/components/complete-booking/complete-booking';
import { DeleteConfirmation } from '../../../../shared/components/delete-confirmation/delete-confirmation';

import { CustomerBookingService } from '../../../../core/services/booking-management/booking-management-service';
import { CustomerManagementService } from '../../../../core/services/customer-user-management/customer-management-service';
import { Toaster } from '../../../../core/services/toaster/toaster';

import { IGridColumn } from '../../../../core/models/shared-components/IDataGridModel';

import { IActionItem } from '../../../../core/models/shared-components/IActionDropDownModel';
import { BOOKING_MANAGEMENT_MESSAGES } from '../../../../core/constants/booking-management-messages';
import {
  IBookingDetailRow,
  ICustomerBookingDetail,
} from '../../../../core/models/booking-management/customer-booking-detail';
import {
  IActiveBookingFilters,
  IBookingDetailFilter,
} from '../../../../core/models/booking-management/booking-managment-filter';
import { HttpParams } from '@angular/common/http';
import { API_BASE_URL } from '../../../../core/constants/environment-config';

@Component({
  selector: 'app-booking-detail-grid',
  standalone: true,
  imports: [FormsModule, DataGrid],
  templateUrl: './booking-detail-grid.html',
  styleUrl: './booking-detail-grid.css',
})
export class BookingDetailGrid implements OnInit, OnChanges {
  @Input() customerId!: number;
  @Input() paymentMethodValue!: number;
  @Input() sharedFilters: IActiveBookingFilters = {};
  @Input() highlightBookingId: number | null = null;

  @Output() bookingDeleted = new EventEmitter<void>();
  @Output() bookingUpdated = new EventEmitter<void>();

  highlightedRowId: number | null = null;
  private pendingHighlightId: number | null = null;
  @Input() forceReloadKey: number = 0;


  private readonly bookingService = inject(CustomerBookingService);
  private readonly customerService = inject(CustomerManagementService);
  private readonly toaster = inject(Toaster);
  private readonly dialog = inject(MatDialog);
  private readonly router = inject(Router);

  readonly MESSAGES = BOOKING_MANAGEMENT_MESSAGES;

  columns: IGridColumn[] = [
    { field: 'bookingId', header: 'Booking ID', width: '110px', sortable: true, type: 'id' },
    { field: 'serviceName', header: 'Service', width: '17%', sortable: true },
    { field: 'serviceType', header: 'Service Type', width: '17%', sortable: false },
    { field: 'dateTime', header: 'Date & Time', width: '17%', sortable: true },
    {
      field: 'assignedExpert',
      header: 'Assigned Expert',
      width: '17%',
      sortable: false,
      type: 'expert',
    },
    { field: 'amount', header: 'Amount', width: '110px', sortable: false, type: 'currency' },
    {
      field: 'bookingStatus',
      header: 'Status',
      width: '110px',
      sortable: false,
      type: 'booking-status',
    },
  ];

  data: IBookingDetailRow[] = [];
  totalCount: number = 0;
  pageNumber: number = 1;
  pageSize: number = 5;
  sortBy: string = '';
  sortOrder: string = 'desc';
  pageSizeOptions: number[] = [5, 10, 25];
  isPageLoading: boolean = false;
  isLoading = true;

  ngOnInit(): void {
    this.load();
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['sharedFilters'] && !changes['sharedFilters'].firstChange) {
      this.pageNumber = 1;
      this.load();
    }

    if (changes['forceReloadKey'] && !changes['forceReloadKey'].firstChange) {
      this.load();
    }
    
    if (changes['highlightBookingId']) {
      if (this.highlightBookingId != null) {
        this.pendingHighlightId = this.highlightBookingId;

        if (this.data.length > 0) {
          setTimeout(() => this.applyHighlight(), 100);
        }
      } else {
        this.pendingHighlightId = null;
        this.highlightedRowId = null;
      }
    }
  }

  private applyHighlight(): void {
    const idToHighlight = this.pendingHighlightId;
    if (idToHighlight == null) return;
    const found = this.data.find((d) => d.bookingId === idToHighlight);
    if (!found) return;

    this.highlightedRowId = idToHighlight;
    this.pendingHighlightId = null;

    setTimeout(() => {
      const el = document.getElementById(`booking-${idToHighlight}`);
      el?.scrollIntoView({ behavior: 'smooth', block: 'center' });
    }, 150);

    setTimeout(() => {
      this.highlightedRowId = null;
    }, 3200);
  }
  private buildDetailParams(): HttpParams {
    let params = new HttpParams()
      .set('paymentMethod', this.paymentMethodValue)
      .set('pageNumber', this.pageNumber)
      .set('pageSize', this.pageSize);

    if (this.sortBy) params = params.set('sortBy', this.sortBy);

    if (this.sortOrder) params = params.set('sortOrder', this.sortOrder);

    if (this.sharedFilters.serviceType)
      params = params.set('serviceType', this.sharedFilters.serviceType);

    if (this.sharedFilters.date) params = params.set('fromDate', this.sharedFilters.date);

    if (this.sharedFilters.time) params = params.set('fromTime', this.sharedFilters.time);

    if (this.sharedFilters.bookingStatus != null)
      params = params.set('bookingStatus', this.sharedFilters.bookingStatus);

    return params;
  }

  load(): void {
    this.isPageLoading = true;
    this.isLoading = true;
    this.highlightedRowId = null;

    this.bookingService
      .getCustomerBookingDetails(this.customerId, this.buildDetailParams())
      .subscribe({
        next: (res) => {
          if (!res.success) {
            this.toaster.error(res.message);
            return;
          }

          this.totalCount = res.data!.totalCount;
          this.data = res.data!.items.map((item: ICustomerBookingDetail) => this.mapRow(item));

          if (this.pendingHighlightId != null) {
            setTimeout(() => this.applyHighlight(), 200);
          }
        },
        error: (err: any) => {
          this.toaster.error(err?.error?.message ?? this.MESSAGES.LOAD_DETAILS_FAILED);
          this.isPageLoading = false;
          this.isLoading = false;
        },
        complete: () => {
          this.isPageLoading = false;
          setTimeout(() => {
            this.isLoading = false;
          }, 400);
        }
      });
  }

  private mapRow(item: ICustomerBookingDetail): IBookingDetailRow {
    const status = item.bookingStatus ?? '';

    const expertImage = item.expertPhoto
      ? `${API_BASE_URL}${item.expertPhoto
          .split('/')
          .map((seg, i) => (i === 0 ? seg : encodeURIComponent(seg)))
          .join('/')}`
      : null;

    const actions: IActionItem[] =
      status === 'Completed' || status === 'Cancelled' || status === 'Failed'
        ? [{ label: 'Delete', icon: 'bi-trash', action: 'delete' }]
        : status === 'InProgress'
        ? [
            { label: 'Complete Booking', icon: 'bi-check-circle', action: 'complete-booking' },
            { label: 'Cancel Booking', icon: 'bi-x-circle', action: 'cancel-booking' },
          ]
        : [
            { label: 'Change Expert', icon: 'bi-person-fill-gear', action: 'change-expert' },
            { label: 'Complete Booking', icon: 'bi-check-circle', action: 'complete-booking' },
            { label: 'Cancel Booking', icon: 'bi-x-circle', action: 'cancel-booking' },
          ];

    return { ...item, assignedExpert: item.expertName ?? '-', expertImage, actions };
  }

  onSortChanged(event: { sortBy: string; sortOrder: string }): void {
    this.sortBy = event.sortBy;
    this.sortOrder = event.sortOrder;
    this.pageNumber = 1;
    this.load();
  }

  handleAction(event: { action: string; row: IBookingDetailRow }): void {
    const { action, row } = event;
    switch (action) {
      case 'change-expert':
        this.openChangeExpertModal(row);
        break;
      case 'complete-booking':
        this.completeBooking(row);
        break;
      case 'cancel-booking':
        this.cancelBooking(row);
        break;
      case 'delete':
        this.deleteBooking(row);
        break;
    }
  }

  onExpertDetailClicked(row: IBookingDetailRow): void {
    if (!row.partnerId) {
      this.toaster.error(this.MESSAGES.NO_EXPERT);
      return;
    }
this.router.navigate(['/admin/service-partners', row.partnerId], {
  queryParams: { returnTo: this.router.url ,}
});

  }

  private openChangeExpertModal(row: IBookingDetailRow): void {
    this.customerService.getAvailablePartners(row.bookingId).subscribe({
      next: (res: any) => {
        if (!res.success) {
          this.toaster.error(res.message);
          return;
        }
        this.dialog
          .open(ChangeExpert, {
            width: '500px',
            disableClose: true,
            data: {
              bookingId: row.bookingId,
              currentPartner: row.assignedExpert,
              serviceType: row.serviceType,
              expertImage: row.expertImage,
              partners: res.data.filter((p: any) => !p.isCurrentlyAssigned),
              apiCall: (partnerId: number) =>
                this.customerService.changeExpert(row.bookingId, partnerId),
            },
          })
          .afterClosed()
          .subscribe((r) => {
            if (r) this.load();
          });
      },
      error: (err: any) =>
        this.toaster.error(err?.error?.message ?? this.MESSAGES.LOAD_PARTNERS_FAILED),
    });
  }

  private completeBooking(row: IBookingDetailRow): void {
    this.dialog
      .open(CompleteBooking, {
        width: '400px',
        disableClose: true,
        data: {
          message: this.MESSAGES.COMPLETE_CONFIRMATION,
          apiCall: () => this.customerService.completeBooking(row.bookingId),
        },
      })
      .afterClosed()
      .subscribe((r) => {
        if (r) {
          this.load();
          this.bookingUpdated.emit();
        }
      });
  }

  private cancelBooking(row: IBookingDetailRow): void {
    this.dialog
      .open(CancelBooking, {
        width: '400px',
        disableClose: true,
        data: {
          bookingId: row.bookingId,
          apiCall: (reason: string) => this.customerService.cancelBooking(row.bookingId, reason),
        },
      })
      .afterClosed()
      .subscribe((r) => {
        if (r) this.load();
      });
  }

  private deleteBooking(row: IBookingDetailRow): void {
    this.dialog
      .open(DeleteConfirmation, {
        width: '400px',
        disableClose: true,
        data: {
          message: this.MESSAGES.DELETE_CONFIRMATION,
          apiCall: () => this.customerService.deleteBooking(row.bookingId),
        },
      })
      .afterClosed()
      .subscribe((r) => {
        if (r) {
          this.load();
          this.bookingDeleted.emit();
        }
      });
  }

  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }
  get rangeStart(): number {
    return this.totalCount === 0 ? 0 : (this.pageNumber - 1) * this.pageSize + 1;
  }
  get rangeEnd(): number {
    return Math.min(this.pageNumber * this.pageSize, this.totalCount);
  }
  get isFirstPage(): boolean {
    return this.pageNumber <= 1;
  }
  get isLastPage(): boolean {
    return this.pageNumber >= this.totalPages;
  }

  onPageSizeChange(newSize: number): void {
    this.pageSize = Number(newSize);
    this.pageNumber = 1;
    this.load();
  }
  goToPrev(): void {
    if (!this.isFirstPage) {
      this.pageNumber--;
      this.load();
    }
  }
  goToNext(): void {
    if (!this.isLastPage) {
      this.pageNumber++;
      this.load();
    }
  }
}
