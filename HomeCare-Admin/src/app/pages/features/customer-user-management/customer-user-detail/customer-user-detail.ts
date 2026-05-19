import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { Location, NgClass } from '@angular/common';
import { MatTooltip } from '@angular/material/tooltip';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { CustomerManagementService } from '../../../../core/services/customer-user-management/customer-management-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { GridLayout } from '../../../../shared/components/grid-layout/grid-layout';
import { FilterPanel } from '../../../../shared/components/filter-panel/filter-panel';
import { ICustomerDetail } from '../../../../core/models/customer-user/ICustomerDetail';
import { ICustomerBookingList } from '../../../../core/models/customer-user/ICustomerBookingList';
import { ICustomerBookingFilter } from '../../../../core/models/customer-user/ICustomerBookingFilter';
import { IDropdownOption } from '../../../../core/models/customer-user/IDropdownOption';
import { ECustomerStatus } from '../../../../core/enums/customer-user-management/customer-status';
import {
  BOOKING_MESSAGES,
  CUSTOMER_MESSAGES,
} from '../../../../core/constants/customer-user-messages';
import { IGridColumn } from '../../../../core/models/shared-components/IDataGridModel';
import { IFilterPanelData } from '../../../../core/models/shared-components/IFilterPanel';
import { DeleteConfirmation } from '../../../../shared/components/delete-confirmation/delete-confirmation';
import { ChangeExpert } from '../../../../shared/components/change-expert/change-expert';
import { CancelBooking } from '../../../../shared/components/cancel-booking/cancel-booking';
import { CompleteBooking } from '../../../../shared/components/complete-booking/complete-booking';
import { environment } from '../../../../../environments/environment';

@Component({
  selector: 'app-customer-user-detail',
  imports: [NgClass, MatTooltip, MatDialogModule, GridLayout],
  templateUrl: './customer-user-detail.html',
  styleUrl: './customer-user-detail.css',
})
export class CustomerUserDetail implements OnInit {
  private readonly BASE_URL = environment.apiUrl;

  readonly MESSAGES = CUSTOMER_MESSAGES;
  readonly ECustomerStatus = ECustomerStatus;

  isPageLoading = false;
  isLoading = true;
  customer: ICustomerDetail | null = null;
  location = inject(Location);
  serviceTypeOptions: IDropdownOption[] = [];
  activeBookingFilters: Record<string, any> = {};

  columns: IGridColumn[] = [
    { header: 'Service ID', field: 'serviceId', width: '5%', type: 'text', sortable: true },
    { header: 'Service Name', field: 'serviceName', width: '11%', type: 'text', sortable: true },
    { header: 'Service Type', field: 'serviceType', width: '13%', type: 'text', sortable: true },
    {
      header: 'Assigned Expert',
      field: 'assignedExpert',
      width: '11%',
      type: 'expert',
      sortable: false,
    },
    { header: 'Address', field: 'address', type: 'text', width: '16%', sortable: false },
    { header: 'Date & Time', field: 'dateTime', type: 'text', width: '13%', sortable: true },
    { header: 'Amount', field: 'amount', type: 'currency', width: '9%', sortable: true },
    {
      header: 'Payment Method',
      field: 'paymentMethod',
      type: 'text',
      width: '9%',
      sortable: false,
    },
    { header: 'Status', field: 'status', type: 'booking-status', width: '11%', sortable: false },
  ];

  bookingData: ICustomerBookingList[] = [];
  bookingTotalCount = 0;
  minAmount: number = 0;
  maxAmount: number = 0;

  bookingFilter: ICustomerBookingFilter = {
    pageNumber: 1,
    pageSize: 5,
    sortBy: '',
    sortOrder: '',
    serviceTypeId: '',
    date: null,
    time: null,
    minAmount: null,
    maxAmount: null,
    paymentMethod: '',
    status: '',
  };

  constructor(
    private customerService: CustomerManagementService,
    private toaster: Toaster,
    private route: ActivatedRoute,
    private dialog: MatDialog,
    private router: Router
  ) {}

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.loadCustomerDetails(id);
      this.loadBookings(id);
      this.loadServiceTypes();
    }
  }

  private loadServiceTypes(): void {
    this.isLoading = true;
    this.customerService.getServiceTypes().subscribe({
      next: (res: any) => {
        if (res.success && res.data) {
          this.serviceTypeOptions = res.data.map((d: any) => ({
            label: d.label,
            value: d.value,
          }));
        }
      },
      error: () => {
        this.isLoading = false;
        this.toaster.error(this.MESSAGES.LOAD_TO_FAILED_SERVICETYPE);
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }

  loadCustomerDetails(id: number) {
    this.isPageLoading = true;
    this.customerService.getCustomerById(id).subscribe({
      next: (res: any) => {
        if (!res.success || !res.data) {
          this.toaster.error(res.message);
          return;
        }
        this.customer = res.data;
      },
      error: (err: any) => {
        this.isPageLoading = false;
        this.toaster.error(err?.error?.message ?? this.MESSAGES.LOAD_FAILED);
      },
      complete: () => {
        this.isPageLoading = false;
      },
    });
  }

  loadBookings(customerId: number) {
    this.isLoading = true;
    const params: any = {};
    const f = this.bookingFilter;

    if (f.pageNumber) params['pageNumber'] = f.pageNumber;
    if (f.pageSize) params['pageSize'] = f.pageSize;
    if (f.sortBy) params['sortBy'] = f.sortBy;
    if (f.sortOrder) params['sortOrder'] = f.sortOrder;
    if (f.serviceTypeId) params['serviceTypeId'] = f.serviceTypeId;
    if (f.date) params['date'] = f.date;
    if (f.time) params['time'] = f.time;
    if (f.minAmount != null && f.minAmount > 0) params['minAmount'] = f.minAmount;
    if (f.maxAmount != null && f.maxAmount < 99999) params['maxAmount'] = f.maxAmount;
    if (f.paymentMethod) params['paymentMethod'] = f.paymentMethod;
    if (f.status) params['status'] = f.status;

    this.customerService.getCustomerBookings(customerId, params).subscribe({
      next: (res: any) => {
        this.bookingData = (res.data?.data ?? []).map(
          (b: any) =>
            ({
              bookingId: b.bookingId,
              serviceId: b.serviceId,
              serviceName: b.serviceName,
              serviceType: b.serviceType,
              assignedExpert: b.assignedExpert,
              expertImage: b.partnerImage
                ? `${this.BASE_URL}${b.partnerImage
                    .split('/')
                    .map((seg: string, i: number) => (i === 0 ? seg : encodeURIComponent(seg)))
                    .join('/')}`
                : null,
              partnerId: b.partnerId,
              partnerPhone: b.partnerPhone,
              address: b.address,
              dateTime: b.dateTime,
              amount: b.amount,
              paymentMethod: b.paymentMethod,
              status: b.status,
              isPartnerDeleted: b.isPartnerDeleted ?? false,

              actions:
                b.status === 'Completed' || b.status === 'Cancelled' || b.status === 'Failed'
                  ? [{ label: 'Delete', icon: 'bi-trash', action: 'delete' }]
                  : b.status === 'InProgress'
                  ? [
                      {
                        label: 'Complete Booking',
                        icon: 'bi-check-circle',
                        action: 'complete-booking',
                      },
                      { label: 'Cancel Booking', icon: 'bi-x-circle', action: 'cancel-booking' },
                    ]
                  : [
                      {
                        label: 'Change Expert',
                        icon: 'bi-person-fill-gear',
                        action: 'change-expert',
                      },
                      {
                        label: 'Complete Booking',
                        icon: 'bi-check-circle',
                        action: 'complete-booking',
                      },
                      { label: 'Cancel Booking', icon: 'bi-x-circle', action: 'cancel-booking' },
                    ],
              reason: b.reason,
            } as ICustomerBookingList)
        );

        this.bookingTotalCount = res.data?.totalCount ?? 0;
        this.minAmount = res.data?.minAmount ?? 0;
        this.maxAmount = res.data?.maxAmount ?? 0;
      },
      error: () => {
        this.toaster.error(this.MESSAGES.LOAD_FAILED), (this.isLoading = false);
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }

  goBack(): void {
    const returnTo = this.route.snapshot.queryParams['returnTo'];

    if (returnTo) {
      this.router.navigateByUrl(returnTo);
    } else {
      const { returnTo: _, ...queryParams } = this.route.snapshot.queryParams;
      this.router.navigate(['/admin/customer-users'], {
        queryParams,
      });
    }
  }

  openBookingFilter() {
    const data: IFilterPanelData = {
      fields: [
        {
          key: 'serviceType',
          label: 'Service Type',
          type: 'select',
          options: this.serviceTypeOptions,
        },
        { key: 'date', label: 'Date', type: 'date', allowFuture: true },
        { key: 'time', label: 'Time', type: 'time' },
        { key: 'amount', label: 'Amount', type: 'range', min: this.minAmount, max: this.maxAmount },
        {
          key: 'paymentMethod',
          label: 'Payment Method',
          type: 'select',
          options: [
            { label: 'DebitCard', value: 'DebitCard' },
            { label: 'Cash', value: 'Cash' },
          ],
        },
        {
          key: 'status',
          label: 'Status',
          type: 'select',
          options: [
            { label: 'Pending', value: 'Pending' },
            { label: 'Completed', value: 'Completed' },
            { label: 'Cancelled', value: 'Cancelled' },
            { label: 'InProgress', value: 'InProgress' },
          ],
        },
      ],
      initialValues: this.activeBookingFilters,
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
        this.activeBookingFilters = {};
        this.bookingFilter.serviceTypeId = '';
        this.bookingFilter.date = null;
        this.bookingFilter.time = null;
        this.bookingFilter.minAmount = null;
        this.bookingFilter.maxAmount = null;
        this.bookingFilter.paymentMethod = '';
        this.bookingFilter.status = '';
        this.bookingFilter.pageNumber = 1;
      } else {
        this.activeBookingFilters = filters!;
        this.bookingFilter.serviceTypeId = filters!['serviceType'] ?? '';

        const rawDate = filters!['date'];
        if (rawDate) {
          const d = new Date(rawDate);
          const yyyy = d.getFullYear();
          const mm = String(d.getMonth() + 1).padStart(2, '0');
          const dd = String(d.getDate()).padStart(2, '0');
          this.bookingFilter.date = `${yyyy}-${mm}-${dd}`;
        } else {
          this.bookingFilter.date = null;
        }

        this.bookingFilter.time = filters!['time'] ?? null;
        this.bookingFilter.minAmount = filters!['amount']?.min ?? null;
        this.bookingFilter.maxAmount = filters!['amount']?.max ?? null;
        this.bookingFilter.paymentMethod = filters!['paymentMethod'] ?? '';
        this.bookingFilter.status = filters!['status'] ?? '';
        this.bookingFilter.pageNumber = 1;
      }

      this.loadBookings(this.customer!.id);
    });
  }

  onBookingPageChanged(e: { pageNumber: number; pageSize: number }) {
    this.bookingFilter.pageNumber = e.pageNumber;
    this.bookingFilter.pageSize = e.pageSize;
    this.loadBookings(this.customer!.id);
  }

  onBookingSortChanged(e: { sortBy: string; sortOrder: string }) {
    this.bookingFilter.sortBy = e.sortBy;
    this.bookingFilter.sortOrder = e.sortOrder;
    this.bookingFilter.pageNumber = 1;
    this.loadBookings(this.customer!.id);
  }
  handleBookingAction(event: { action: string; row: ICustomerBookingList }) {
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

  openChangeExpertModal(row: ICustomerBookingList) {
    this.customerService.getAvailablePartners(row.bookingId).subscribe({
      next: (res: any) => {
        if (!res.success) {
          this.toaster.error(res.message);
          return;
        }

        const dialogRef = this.dialog.open(ChangeExpert, {
          width: '500px',
          disableClose: true,
          data: {
            bookingId: row.bookingId,
            currentPartner: row.assignedExpert,
            serviceType: row.serviceType,
            expertImage: row.expertImage,
            partners: res.data
              .filter((p: any) => !p.isCurrentlyAssigned)
              .map((p: any) => ({
                ...p,
                profileImage: p.profileImage
                  ? `${this.BASE_URL}${p.profileImage
                      .split('/')
                      .map((seg: string, i: number) => (i === 0 ? seg : encodeURIComponent(seg)))
                      .join('/')}`
                  : null,
              })),

            apiCall: (partnerId: number) =>
              this.customerService.changeExpert(row.bookingId, partnerId),
          },
        });

        dialogRef.afterClosed().subscribe((result) => {
          if (result) {
            this.loadBookings(this.customer!.id);
          }
        });
      },
      error: (err: any) =>
        this.toaster.error(err?.error?.message ?? CUSTOMER_MESSAGES.FAILED_TO_LOAD_PARTNER),
    });
  }

  completeBooking(row: ICustomerBookingList) {
    const dialogRef = this.dialog.open(CompleteBooking, {
      width: '400px',
      disableClose: true,
      data: {
        message: BOOKING_MESSAGES.COMPLETE_BOOKING_DIALOG,
        apiCall: () => this.customerService.completeBooking(row.bookingId),
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) this.loadBookings(this.customer!.id);
    });
  }

  cancelBooking(row: ICustomerBookingList) {
    const dialogRef = this.dialog.open(CancelBooking, {
      width: '400px',
      disableClose: true,
      data: {
        bookingId: row.bookingId,
        apiCall: (reason: string) => this.customerService.cancelBooking(row.bookingId, reason),
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) this.loadBookings(this.customer!.id);
    });
  }
  deleteBooking(row: ICustomerBookingList) {
    const dialogRef = this.dialog.open(DeleteConfirmation, {
      width: '400px',
      disableClose: true,
      data: {
        message: BOOKING_MESSAGES.DELETE_BOOKING_DIALOG,
        apiCall: () => this.customerService.deleteBooking(row.bookingId),
      },
    });
    dialogRef.afterClosed().subscribe((result) => {
      if (result) this.loadBookings(this.customer!.id);
    });
  }
  onExpertDetailClicked(row: any): void {
    if (!row.partnerId) {
      this.toaster.error(BOOKING_MESSAGES.NO_EXPERT_AVAILABLE);
      return;
    }
    this.router.navigate(['/admin/service-partners', row.partnerId], {
      queryParams: { returnTo: `/admin/customer-users/${this.customer!.id}` },
    });
  }
}
