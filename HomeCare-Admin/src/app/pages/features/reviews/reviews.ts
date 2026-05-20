import { Component, OnInit } from '@angular/core';
import { GridLayout } from '../../../shared/components/grid-layout/grid-layout';
import { IGridColumn } from '../../../core/models/shared-components/IDataGridModel';
import { IAdminReview, IAdminReviewFilterRequest } from '../../../core/models/reviews/IAdminReview';
import { Toaster } from '../../../core/services/toaster/toaster';
import { MatDialog } from '@angular/material/dialog';
import { FilterPanel } from '../../../shared/components/filter-panel/filter-panel';
import { IFilterPanelData } from '../../../core/models/shared-components/IFilterPanel';
import { REVIEW_MESSAGES } from '../../../core/constants/review-messages';
import { ActivatedRoute } from '@angular/router';
import { AdminReviewService } from '../../../core/services/reviews/admin-review';

@Component({
  selector: 'app-reviews',
  imports: [GridLayout],
  templateUrl: './reviews.html',
  styleUrl: './reviews.css',
})
export class Reviews implements OnInit {
  totalCount = 0;
  data: IAdminReview[] = [];
  activeFilters: Record<string, any> = {};
  isLoading = true;

  columns: IGridColumn[] = [
    {
      header: 'Booking ID',
      field: 'bookingId',
      type: 'id',
      width: '9%',
      sortable: true,
    },
    {
      header: 'Customer',
      field: 'customerName',
      type: 'text',
      width: '11%',
      sortable: true,
    },
    {
      header: 'Service Partner',
      field: 'partnerName',
      type: 'text',
      width: '11%',
      sortable: true,
    },
    {
      header: 'Service',
      field: 'serviceName',
      type: 'text',
      width: '18%',
      sortable: true,
    },
    {
      header: 'Rating',
      field: 'rating',
      type: 'rating',
      width: '10%',
      sortable: true,
    },
    {
      header: 'Review',
      field: 'reviewText',
      type: 'text',
      width: '25%',
      sortable: false,
    },
    {
      header: 'Date',
      field: 'createdAt',
      type: 'text',
      width: '11%',
      sortable: true,
    },
  ];

  filter: IAdminReviewFilterRequest = {
    pageNumber: 1,
    pageSize: 10,
    sortBy: '',
    sortOrder: '',
    customerName: '',
    partnerName: '',
    serviceName: '',
    minRating: null,
    maxRating: null,
  };

  constructor(
    private reviewService: AdminReviewService,
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
        this.filter.customerName = params['customerName'] || '';
        this.filter.partnerName = params['partnerName'] || '';
        this.filter.serviceName = params['serviceName'] || '';
        this.filter.minRating = params['minRating'] ? Number(params['minRating']) : null;
        this.filter.maxRating = params['maxRating'] ? Number(params['maxRating']) : null;

        this.activeFilters = { ...params };
        if (this.activeFilters['minRating'] || this.activeFilters['maxRating']) {
          this.activeFilters['rating'] = {
            min: this.filter.minRating,
            max: this.filter.maxRating,
          };
        }
      }
      this.loadReviews();
    });
  }

  loadReviews(): void {
    this.isLoading = true;

    const params: any = {};
    Object.keys(this.filter).forEach((key) => {
      const value = (this.filter as any)[key];
      if (value !== null && value !== '') {
        params[key] = value;
      }
    });

    this.reviewService.getReviews(params).subscribe({
      next: (res) => {
        this.totalCount = res.data?.totalCount ?? 0;

        this.data = (res.data?.data ?? []).map((r) => ({
          id:           r.id,
          bookingId:    r.bookingId,
          customerName: r.customerName,
          partnerName:  r.partnerName,
          serviceName:  r.serviceName,
          rating:       r.rating,
          reviewText:   r.reviewText ?? '—',
          createdAt:    this.formatDate(r.createdAt),
        }));
      },
      error: (err) => {
        this.isLoading = false;
        this.toaster.error(err?.error?.message || REVIEW_MESSAGES.FAILED_LOAD);
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }

  onPageChanged(event: { pageNumber: number; pageSize: number }): void {
    this.filter.pageNumber = event.pageNumber;
    this.filter.pageSize = event.pageSize;
    this.loadReviews();
  }

  onSortChanged(event: { sortBy: string; sortOrder: string }): void {
    this.filter.sortBy    = event.sortBy;
    this.filter.sortOrder = event.sortOrder;
    this.filter.pageNumber = 1;
    this.loadReviews();
  }
  openFilter(): void {
    const data: IFilterPanelData = {
      fields: [
        {
          key: 'customerName',
          label: 'Customer Name',
          type: 'input',
        },
        {
          key: 'partnerName',
          label: 'Service Partner Name',
          type: 'input',
        },
        {
          key: 'serviceName',
          label: 'Service Name',
          type: 'input',
        },
        {
          key: 'rating',
          label: 'Rating Range',
          type: 'range',
          min: 1,
          max: 5,
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
        this.filter.customerName = '';
        this.filter.partnerName = '';
        this.filter.serviceName = '';
        this.filter.minRating = null;
        this.filter.maxRating = null;
        this.filter.pageNumber = 1;
        this.loadReviews();
        return;
      }

      this.activeFilters = filters!;
      this.filter.customerName = filters!['customerName'] ?? '';
      this.filter.partnerName = filters!['partnerName'] ?? '';
      this.filter.serviceName = filters!['serviceName'] ?? '';
      this.filter.minRating = filters!['rating']?.min ?? null;
      this.filter.maxRating = filters!['rating']?.max ?? null;
      this.filter.pageNumber = 1;

      this.loadReviews();
    });
  }

  private formatDate(raw: string): string {
    if (!raw) return '—';
    const d = new Date(raw);
    const day = String(d.getDate()).padStart(2, '0');
    const month = d.toLocaleString('en', { month: 'short' });
    const year = d.getFullYear();
    return `${day} ${month} ${year}`;
  }

  getStars(rating: number): boolean[] {
    return Array.from({ length: 5 }, (_, i) => i < rating);
  }

  getRatingColor(rating: number): string {
    if (rating >= 4) return '#1ea31f';
    if (rating >= 3) return '#ea580c';
    return '#dc2626';
  }
}
