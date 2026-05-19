import { Component, input, Input, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatSlideToggleChange } from '@angular/material/slide-toggle';
import { MatDialog } from '@angular/material/dialog';
import {
  ICategory,
  IService,
  IServiceRow,
  IServiceType,
  ISubCategory,
} from '../../../../core/models/service-management/service';
import { ServiceManagementService } from '../../../../core/services/service-management/service-management-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { CategoryList } from '../category-list/category-list';
import { ServiceTable } from '../service-table/service-table';
import { IServiceFilterRequest } from '../../../../core/models/service-management/IServiceFilterRequest';
import { HttpParams } from '@angular/common/http';
import { MESSAGES } from '../../../../core/constants/service-management-messages';
import { AvailabilityConfirmation, IAvailabilityConfirmation } from '../../../../shared/components/availability-confirmation/availability-confirmation';

@Component({
  selector: 'app-service-type-detail',
  standalone: true,
  imports: [CommonModule, CategoryList, ServiceTable],
  templateUrl: './service-type-detail.html',
  styleUrl: './service-type-detail.css',
})
export class ServiceTypeDetail implements OnChanges {
  @Input() serviceType!: IServiceType;
  @Input() expanded = false;

  categories: ICategory[] = [];
  serviceRows: IServiceRow[] = [];
  subCategories: ISubCategory[] = [];

  @Input() restoreCategoryId: number | null = null;
  activeCategory: ICategory | null = null;

  loadingCategories = false;
  loadingServices = true;

  activeFilter: Partial<IServiceFilterRequest> = {};

  pendingService: IServiceRow | null = null;
  pendingAvailability = false;

  constructor(
    private svc: ServiceManagementService,
    private dialog: MatDialog,
    private toaster: Toaster
  ) { }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['serviceType'] && this.serviceType) {
      this.loadCategories();
    }
    if (changes['expanded'] && this.expanded) {
      this.loadCategories();
    }
  }

  loadCategories(): void {
    this.loadingCategories = true;
    this.categories = [];
    this.activeCategory = null;
    this.serviceRows = [];
    
    this.svc.getCategoriesByServiceType(this.serviceType.id).subscribe({
      next: (res) => {
        this.categories = res.data ?? [];
        this.categories.forEach((cat) => {
          if (this.activeCategory?.id === cat.id) return;
          this.svc.getServicesByCategory(cat.id).subscribe({
            next: (r) => (cat.serviceCount = (r.data ?? []).length),
            error: () => (cat.serviceCount = 0),
          });
        });

        if (this.categories.length) {
          const toRestore = this.restoreCategoryId
            ? this.categories.find((c) => c.id === this.restoreCategoryId) ?? this.categories[0]
            : this.categories[0];

            this.restoreCategoryId = null;
          this.selectCategory(toRestore);
        }
      },
      error: () => {
        this.loadingCategories = false;
      },
      complete: () => {
        setTimeout(() => {
          this.loadingCategories = false;
        }, 400);
      }
    });
  }

  selectCategory(cat: ICategory): void {
    if (this.activeCategory?.id === cat.id) return;
    this.activeCategory = cat;
    this.activeFilter = {};
    this.loadServices(cat.id);

    this.svc.getSubCategoriesByCategory(cat.id).subscribe({
      next: (res) => (this.subCategories = res.data ?? []),
      error: () => (this.subCategories = []),
    });
  }

  loadServices(categoryId: number): void {
    this.loadingServices = true;

    let params = new HttpParams();

    if (this.activeFilter.subCategoryId != null)
      params = params.set('subCategoryId', this.activeFilter.subCategoryId);
    if (this.activeFilter.minPrice != null)
      params = params.set('priceMin', this.activeFilter.minPrice);
    if (this.activeFilter.maxPrice != null)
      params = params.set('priceMax', this.activeFilter.maxPrice);
    if (this.activeFilter.isAvailable != null)
      params = params.set('isAvailable', this.activeFilter.isAvailable);
    if (this.activeFilter.commissionPct != null)
      params = params.set('commissionPct', this.activeFilter.commissionPct);

    this.svc.getServicesByCategory(categoryId, params).subscribe({
      next: (res: any) => {
        if (!res.success || !res.data) {
          this.toaster.error(res.message ?? MESSAGES.SERVICE.LOAD_FAILED);
          return;
        }

        this.serviceRows = res.data.map((s: IService) => this.mapRow(s));

        if (this.activeCategory) {
          this.activeCategory.serviceCount = res.data.length;
        }
      },
      error: (err: any) => {
        this.loadingServices = false;
        this.toaster.error(err?.error?.message ?? MESSAGES.SERVICE.LOAD_FAILED);
      },
      complete: () => {
        setTimeout(() => {
          this.loadingServices = false;
        }, 400);
      },
    });
  }

  private mapRow(s: IService): IServiceRow {
    return {
      ...s,
      priceFormatted: `$${s.price.toFixed(2)}`,
      commissionFormatted: `${s.commissionPct}%`,
    };
  }

  onFilterChanged(filters: Partial<IServiceFilterRequest>): void {
    this.activeFilter = filters;
    if (this.activeCategory) this.loadServices(this.activeCategory.id);
  }

  onAvailabilityChange(service: IServiceRow, event: MatSlideToggleChange): void {
    event.source.checked = !event.checked;

    const isActivating = event.checked;

    this.dialog
      .open(AvailabilityConfirmation, {
        width: '420px',
        data: {
          serviceName: service.name,
          isActivating,
          apiCall: () => this.svc.toggleAvailability(service.id, isActivating),
        } satisfies IAvailabilityConfirmation,
      })
      .afterClosed()
      .subscribe((confirmed: boolean) => {
        if (confirmed) {
          service.isAvailable = isActivating;
          event.source.checked = isActivating;
        }
      });
  }
}