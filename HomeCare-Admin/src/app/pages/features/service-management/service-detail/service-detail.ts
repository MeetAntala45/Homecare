import { Component, inject, OnInit } from '@angular/core';
import { Location, CurrencyPipe } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { ServiceManagementService } from '../../../../core/services/service-management/service-management-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { ServiceAddEditDialog } from '../service-add-edit-dialog/service-add-edit-dialog';
import {
  IService,
  ISubCategory,
  ServiceDialogData,
} from '../../../../core/models/service-management/service';
import { MESSAGES } from '../../../../core/constants/service-management-messages';
import { API_BASE_URL } from '../../../../core/constants/environment-config';

@Component({
  selector: 'app-service-detail',
  standalone: true,
  imports: [CurrencyPipe],
  templateUrl: './service-detail.html',
  styleUrl: './service-detail.css',
})
export class ServiceDetail implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private location = inject(Location);
  private dialog = inject(MatDialog);
  private serviceManagementService = inject(ServiceManagementService);
  private toaster = inject(Toaster);

  service: IService | null = null;
  subCategories: ISubCategory[] = [];
  isLoading = false;

  private categoryId: number = 0;
  private serviceTypeId: number = 0;

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    this.categoryId = Number(this.route.snapshot.queryParamMap.get('categoryId') ?? 0);
    this.serviceTypeId = Number(this.route.snapshot.queryParamMap.get('serviceTypeId') ?? 0);
    if (id) this.loadService(id);
  }

  loadService(id: number): void {
    this.isLoading = true;
    this.serviceManagementService.getServiceById(id).subscribe({
      next: (res: any) => {
        if (!res.success || !res.data) {
          this.toaster.error(res.message ?? MESSAGES.SERVICE.LOAD_DETAIL_FAILED);
          return;
        }
        this.service = res.data;
        this.loadSubCategories();
      },
      error: (err: any) => {
        this.toaster.error(err?.error?.message ?? MESSAGES.SERVICE.LOAD_DETAIL_FAILED);
        this.isLoading = false;
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }

  loadSubCategories(): void {
    if (!this.categoryId) return;
    this.serviceManagementService.getSubCategoriesByCategory(this.categoryId).subscribe({
      next: (res: any) => {
        if (res.success) this.subCategories = res.data ?? [];
      },
      error: () => {},
    });
  }

  openEdit(): void {
    if (!this.service) return;

    const data: ServiceDialogData = {
      categoryName: this.service.categoryName ?? '',
      categoryId: this.categoryId,
      subCategories: this.subCategories,
      serviceTypeName: this.service.serviceTypeName ?? '',
      service: this.service as any,
    };

    this.dialog
      .open(ServiceAddEditDialog, {
        width: '720px',
        maxWidth: '100vw',
        maxHeight: '90vh',
        data,
        disableClose: true,
      })
      .afterClosed()
      .subscribe((saved: boolean) => {
        if (saved && this.service) this.loadService(this.service.id);
      });
  }

  getImageUrl(path: string): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    return `${API_BASE_URL}${path}`;
  }

  onImageError(event: Event): void {
    const img = event.target as HTMLImageElement;
    img.src = 'assets/images/placeholder.png';
  }

  goBack(): void {
    this.router.navigate(['/admin/service-management'], {
      queryParams: {
        expandedServiceTypeId: this.serviceTypeId,
        activeCategoryId: this.categoryId,
      },
    });
  }
}
