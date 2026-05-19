import { Component, EventEmitter, Input, Output, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatSlideToggleModule, MatSlideToggleChange } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatDialog } from '@angular/material/dialog';
import {
  ICategory,
  IServiceRow,
  ISubCategory,
  ServiceDialogData,
} from '../../../../core/models/service-management/service';
import { ServiceManagementService } from '../../../../core/services/service-management/service-management-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { ActionDropdown } from '../../../../shared/components/action-dropdown/action-dropdown';
import { IActionItem } from '../../../../core/models/shared-components/IActionDropDownModel';
import { IServiceFilterRequest } from '../../../../core/models/service-management/IServiceFilterRequest';
import { IFilterPanelData } from '../../../../core/models/shared-components/IFilterPanel';
import { FilterPanel } from '../../../../shared/components/filter-panel/filter-panel';
import { IdFormatPipe } from '../../../../shared/pipes/format/id-format-pipe';
import { ServiceAddEditDialog } from '../service-add-edit-dialog/service-add-edit-dialog';
import { DeleteConfirmation } from '../../../../shared/components/delete-confirmation/delete-confirmation';
import { Router } from '@angular/router';
import { TruncateTooltipDirective } from '../../../../shared/directives/truncate-tooltip';
import { MESSAGES } from '../../../../core/constants/service-management-messages';

@Component({
  selector: 'app-service-table',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatSlideToggleModule,
    MatIconModule,
    ActionDropdown,
    IdFormatPipe,
    TruncateTooltipDirective,
  ],
  templateUrl: './service-table.html',
  styleUrl: './service-table.css',
})
export class ServiceTable implements OnChanges {
  @Input() services: IServiceRow[] = [];
  @Input() categoryName: string | null = null;
  @Input() loadingServices = false;
  @Input() activeCategory: ICategory | null = null;
  @Input() subCategories: ISubCategory[] = [];
  @Input() serviceTypeId: number | null = null;
  @Input() serviceTypeName: string | null = null;

  @Output() filterChanged = new EventEmitter<Partial<IServiceFilterRequest>>();
  @Output() reload        = new EventEmitter<void>();
  @Output() availability  = new EventEmitter<{ service: IServiceRow; event: MatSlideToggleChange }>();

  sortBy    = '';
  sortOrder = 'desc';

  pageSizeOptions = [4, 10, 25, 50];
  pageSize   = 4;
  pageNumber = 1;

  get totalCount() { return this.services.length; }
  get totalPages()  { return Math.ceil(this.totalCount / this.pageSize); }
  get rangeStart()  { return this.totalCount === 0 ? 0 : (this.pageNumber - 1) * this.pageSize + 1; }
  get rangeEnd()    { return Math.min(this.pageNumber * this.pageSize, this.totalCount); }
  get isFirstPage() { return this.pageNumber <= 1; }
  get isLastPage()  { return this.pageNumber >= this.totalPages; }

  get pagedServices(): IServiceRow[] {
    const sorted = this.getSortedServices();
    const start  = (this.pageNumber - 1) * this.pageSize;
    return sorted.slice(start, start + this.pageSize);
  }

  private getSortedServices(): IServiceRow[] {
    if (!this.sortBy) return this.services;

    return [...this.services].sort((a, b) => {
      let valA: any = (a as any)[this.sortBy];
      let valB: any = (b as any)[this.sortBy];

      if (typeof valA === 'number' && typeof valB === 'number') {
        return this.sortOrder === 'asc' ? valA - valB : valB - valA;
      }

      valA = valA?.toString().toLowerCase() ?? '';
      valB = valB?.toString().toLowerCase() ?? '';

      if (valA < valB) return this.sortOrder === 'asc' ? -1 : 1;
      if (valA > valB) return this.sortOrder === 'asc' ?  1 : -1;
      return 0;
    });
  }

  onPageSizeChange(newSize: number) {
    this.pageSize   = Number(newSize);
    this.pageNumber = 1;
  }
  goToPrev() { if (!this.isFirstPage) this.pageNumber--; }
  goToNext()  { if (!this.isLastPage)  this.pageNumber++; }

  rowActions: IActionItem[] = [
    { label: 'Edit',   action: 'edit',   icon: 'bi-pencil' },
    { label: 'Delete', action: 'delete', icon: 'bi-trash'  },
  ];

  onRowAction(action: string, row: IServiceRow) {
    if (action === 'edit')   this.openEdit(row);
    if (action === 'delete') this.openDelete(row);
  }

  onRowClick(row: IServiceRow) {
    this.router.navigate(
      ['admin/service-management/services', row.id],
      {
        queryParams: {
          categoryId:    this.activeCategory?.id ?? 0,
          serviceTypeId: this.serviceTypeId ?? 0,
        },
      }
    );
  }

  onSort(field: string) {
    if (this.sortBy === field) {
      if (this.sortOrder === 'desc') {
        this.sortOrder = 'asc';
      } else {
        this.sortBy    = '';
        this.sortOrder = 'desc';
      }
    } else {
      this.sortBy    = field;
      this.sortOrder = 'desc';
    }
    this.pageNumber = 1;
  }

  getSortIcon(field: string): string {
    if (this.sortBy !== field) return 'bi-arrow-down-up';
    return this.sortOrder === 'asc' ? 'bi-arrow-up' : 'bi-arrow-down';
  }

  constructor(
    private svc: ServiceManagementService,
    private dialog: MatDialog,
    private toaster: Toaster,
    private router: Router,
  ) {}

  ngOnChanges(changes: SimpleChanges) {
    if (changes['services']) {
      const newTotalPages = Math.ceil(this.services.length / this.pageSize);
      if (this.pageNumber > newTotalPages) this.pageNumber = newTotalPages || 1;
    }
  }

  openAdd() {
    if (!this.activeCategory?.id) {
      this.toaster.error(MESSAGES.VALIDATION.SELECT_CATEGORY);
      return;
    }
  
    if (!this.subCategories || this.subCategories.length === 0) {
      this.toaster.error(MESSAGES.VALIDATION.SUBCATEGORY_REQUIRED);
      return;
    }
  
    const data: ServiceDialogData = {
      categoryName:    this.categoryName ?? '',
      categoryId:      this.activeCategory.id,
      subCategories:   this.subCategories,
      serviceTypeName: this.serviceTypeName ?? '',
      service:         null,
    };
  
    this.dialog
      .open(ServiceAddEditDialog, {
        width: '720px',
        maxWidth: '100vw',
        maxHeight: '90vh',
        data,
        disableClose: true
      })
      .afterClosed()
      .subscribe((saved) => {
        if (saved) this.reload.emit();
      });
  }

  openEdit(row: IServiceRow) {
    const data: ServiceDialogData = {
      categoryName:    this.categoryName ?? '',
      categoryId:      this.activeCategory?.id ?? 0,
      subCategories:   this.subCategories,
      serviceTypeName: this.serviceTypeName ?? '',
      service:         row,
    };
    this.dialog
      .open(ServiceAddEditDialog, { width: '720px', maxWidth: '100vw', maxHeight: '90vh', data, disableClose: true })
      .afterClosed()
      .subscribe((saved) => { if (saved) this.reload.emit(); });
  }

  openDelete(row: IServiceRow) {
    this.dialog
      .open(DeleteConfirmation, {
        width: '400px',
        disableClose: true,
        data: {
          message: MESSAGES.DELETE.body(row.name),
          apiCall: () => this.svc.deleteService(row.id),
        },
      })
      .afterClosed()
      .subscribe((result) => {
        if (result) {
          this.toaster.success(MESSAGES.SERVICE.DELETED);
          this.reload.emit();
        }
      });
  }

  onAvailability(service: IServiceRow, event: MatSlideToggleChange) {
    this.availability.emit({ service, event });
  }

  activeFilters: Record<string, any> = {};

  openFilter() {
    const data: IFilterPanelData = {
      fields: [
        {
          key:     'subCategoryId',
          label:   'Sub Category',
          type:    'select',
          options: this.subCategories.map((s) => ({ label: s.name, value: s.id })),
        },
        { key: 'price', label: 'Price', type: 'range', min: 5.5, max: 1550, prefix: '$' },
        {
          key:     'isAvailable',
          label:   'Availability',
          type:    'select',
          options: [
            { label: 'Yes', value: true },
            { label: 'No',  value: false },
          ],
        },
        { key: 'commissionPct', label: 'Commission', type: 'input', inputType: 'number', suffix: '%' },
      ],
      initialValues: this.activeFilters,
    };

    this.dialog
      .open(FilterPanel, {
        data,
        position:   { right: '0', top: '0' },
        height:     '100vh',
        maxWidth:   '360px',
        panelClass: 'filter-panel-dialog',
      })
      .afterClosed()
      .subscribe((filters: Record<string, any> | null) => {
        if (filters === null) {
          this.activeFilters = {};
          this.filterChanged.emit({});
          return;
        }
        this.activeFilters = filters;
        const mapped: Partial<IServiceFilterRequest> = {
          subCategoryId: filters['subCategoryId'] ?? undefined,
          minPrice:      filters['price']?.min === 0     ? undefined : filters['price']?.min ?? undefined,
          maxPrice:      filters['price']?.max === 99999 ? undefined : filters['price']?.max ?? undefined,
          isAvailable:
            filters['isAvailable'] != null && filters['isAvailable'] !== ''
              ? filters['isAvailable'] === true || filters['isAvailable'] === 'true'
              : undefined,
          commissionPct: filters['commissionPct'] ? Number(filters['commissionPct']) : undefined,
        };
        this.filterChanged.emit(mapped);
      });
  }
}