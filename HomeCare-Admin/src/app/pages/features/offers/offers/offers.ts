import { Component, inject, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { GridLayout } from '../../../../shared/components/grid-layout/grid-layout';
import { DeleteConfirmation } from '../../../../shared/components/delete-confirmation/delete-confirmation';
import { OffersService } from '../../../../core/services/offers/offers-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { IGridColumn } from '../../../../core/models/shared-components/IDataGridModel';
import { IOfferFilterRequest } from '../../../../core/models/offers/offer-filter.request';
import { OfferStatus } from '../../../../core/enums/offers/offer-status';
import { IOfferResponse } from '../../../../core/models/offers/offers.response';
import { FilterPanel } from '../../../../shared/components/filter-panel/filter-panel';
import { IFilterPanelData } from '../../../../core/models/shared-components/IFilterPanel';
import { HttpParams } from '@angular/common/http';
import { OFFERS_MESSAGES } from '../../../../core/constants/offer-messages';
import { FAIL_BEHAVIOUR_OPTIONS, OPERATOR_OPTIONS } from '../../../../core/constants/offer-constants';
import { AddOfferModal } from '../add-offer-modal/add-offer-modal';
import { EditOfferModal } from '../edit-offer-modal/edit-offer-modal';

@Component({
  selector: 'app-offers',
  standalone: true,
  imports: [GridLayout],
  templateUrl: './offers.html',
  styleUrl: './offers.css',
})
export class OffersComponent implements OnInit {
  offersService = inject(OffersService);
  toaster = inject(Toaster);

  operatorOptions = OPERATOR_OPTIONS;
  failBehaviourOptions = FAIL_BEHAVIOUR_OPTIONS;

  constructor(private dialog: MatDialog) { }

  columns: IGridColumn[] = [
    { field: 'offerCode', header: 'Coupon Code', width: '17%', sortable: true },
    { field: 'description', header: 'Coupon Description', width: 'auto', sortable: false },
    { field: 'discount', header: 'Coupon Discount', width: '17%', sortable: true },
    { field: 'timesAppliedFormatted', header: 'Coupon Applied', width: '17%', sortable: true },
    { field: 'status', header: 'Status', type: 'status', width: '17%', sortable: false },
  ];

  data: any[] = [];
  totalCount = 0;
  isLoading = true;
  minUsage = 0;
  maxUsage = 999999;

  filter: IOfferFilterRequest = {
    pageNumber: 1,
    pageSize: 10,
    couponCode: '',
    sortBy: 'id',
    sortOrder: 'desc',
  };

  activeFilters: Partial<IOfferFilterRequest> = {};
  rawPanelFilters: Record<string, any> = {};

  ngOnInit(): void {
    this.loadOffers();
  }

  loadOffers(): void {
    this.isLoading = true;

    let params = new HttpParams()
      .set('pageNumber', this.filter.pageNumber)
      .set('pageSize', this.filter.pageSize)
      .set('sortBy', this.filter.sortBy)
      .set('sortOrder', this.filter.sortOrder);

    if (this.activeFilters.couponCode)
      params = params.set('CouponCode', this.activeFilters.couponCode);
    if (this.activeFilters.minDiscount != null)
      params = params.set('minDiscount', this.activeFilters.minDiscount);
    if (this.activeFilters.minUsage != null)
      params = params.set('minUsage', this.activeFilters.minUsage);
    if (this.activeFilters.maxUsage != null)
      params = params.set('maxUsage', this.activeFilters.maxUsage);
    if (this.activeFilters.status != null) params = params.set('status', this.activeFilters.status);
    
    this.offersService.getAll(params).subscribe({
      next: (res: any) => {
        if (!res.success || !res.data) {
          this.toaster.error(res.message ?? OFFERS_MESSAGES.LOAD_FAILED);
          return;
        }
        this.totalCount = res.data.totalCount;
        this.minUsage = res.data?.min ?? 0;
        this.maxUsage = res.data?.max ?? 0;
        this.data = res.data.data.map((o: IOfferResponse) => this.mapRow(o));
      },
      error: (err: any) => {
        this.isLoading = false;
        this.toaster.error(err?.error?.message ?? OFFERS_MESSAGES.LOAD_FAILED);
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }

  private mapRow(o: IOfferResponse): any {
    const isActive = o.status === OfferStatus.Active || o.status === (1 as any);

    return {
      ...o,
      status: isActive ? 'Active' : 'Inactive',
      statusRaw: isActive ? OfferStatus.Active : OfferStatus.Inactive,
      discountPct: this.parseDiscount(o.discount),
      timesAppliedFormatted: this.formatCount(o.timesApplied),
      actions: [
        { label: 'Edit', icon: 'bi-pencil', action: 'edit' },
        { label: 'Delete', icon: 'bi-trash', action: 'delete' },
      ],
    };
  }

  private parseDiscount(discount: string): number | null {
    if (!discount) return null;
    const match = discount.match(/[\d.]+/);
    return match ? parseFloat(match[0]) : null;
  }

  private formatCount(n: number): string {
    if (n == null) return '—';
    if (n >= 1_000_000) return (n / 1_000_000).toFixed(1).replace(/\.0$/, '') + 'M Times';
    if (n >= 1_000) return (n / 1_000).toFixed(1).replace(/\.0$/, '') + 'k Times';
    return n + ' Times';
  }

  onSortChanged(event: { sortBy: string; sortOrder: string }): void {
    this.filter.sortBy = event.sortBy;
    this.filter.sortOrder = event.sortOrder;
    this.filter.pageNumber = 1;
    this.loadOffers();
  }

  onPageChanged(event: { pageNumber: number; pageSize: number }): void {
    this.filter.pageNumber = event.pageNumber;
    this.filter.pageSize = event.pageSize;
    this.loadOffers();
  }

  handleAction(event: any): void {
    const { action, row } = event;
    switch (action) {
      case 'edit':
        this.editOffer(row);
        break;
      case 'delete':
        this.deleteOffer(row.id);
        break;
    }
  }

  openAddOffer(): void {
    this.dialog
      .open(AddOfferModal, { width: '650px', disableClose: true })
      .afterClosed()
      .subscribe((result) => {
        if (result?.created) {
          this.loadOffers();
        }
      });
  }

  editOffer(offer: any): void {
    this.dialog
      .open(EditOfferModal, {
        width: '650px',
        disableClose: true,
        data: {
          ...offer,
          status: offer.statusRaw  
        }
      })
      .afterClosed()
      .subscribe((result) => {
        if (result?.updated) {
          this.loadOffers();
        }
      });
  }

  deleteOffer(id: number): void {
    this.dialog
      .open(DeleteConfirmation, {
        width: '400px',
        disableClose: true,
        data: {
          message: OFFERS_MESSAGES.DELETE_CONFIRM,
          apiCall: () => this.offersService.delete(id),
        },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result) {
          this.totalCount--;
          this.loadOffers();
        }
      });
  }

  openFilter(): void {
    const data: IFilterPanelData = {
      fields: [
        {
          key: 'couponCode',
          label: 'Coupon Code',
          type: 'input',
        },
        {
          key: 'couponDiscount',
          label: 'Coupon Discount',
          type: 'input',
          inputType: 'number',
          suffix: '%',
        },
        { key: 'coupon', label: 'Coupon', type: 'range', min: this.minUsage, max: this.maxUsage },
        { key: 'availability', label: 'Availability', type: 'toggle' },
      ],
      initialValues: this.rawPanelFilters,
    };

    this.dialog
      .open(FilterPanel, {
        data,
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
          this.filter.pageNumber = 1;
          this.loadOffers();
          return;
        }

        this.rawPanelFilters = filters;
        this.activeFilters = {
          couponCode: filters['couponCode']?.trim() || undefined,
          minDiscount: filters['couponDiscount'] ? Number(filters['couponDiscount']) : undefined,
          minUsage: filters['coupon']?.min === 0 ? undefined : filters['coupon']?.min ?? undefined,
          maxUsage:
            filters['coupon']?.max === 99999 ? undefined : filters['coupon']?.max ?? undefined,
          status: filters['availability'] ? OfferStatus.Active : undefined,
        };

        this.filter.pageNumber = 1;
        this.loadOffers();
      });
  }
}