import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatCardModule } from '@angular/material/card';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { MatDividerModule } from '@angular/material/divider';
import { ActivatedRoute, Router } from '@angular/router';

import { Toaster } from '../../../core/services/toaster/toaster';
import { ManageServiceDialog } from './manage-service-dialog/manage-service-dialog';
import { PartnerMyService } from '../../../core/services/my-service/my-service';

import {
  IServiceTypeHierarchy,
  ICategory,
  ISubCategory,
  IServiceItem
} from '../../../core/models/my-service/my-service';
import { API_BASE_URL } from '../../../core/constants/environment-config';

@Component({
  selector: 'app-service-management',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatIconModule,
    MatCardModule,
    MatButtonModule,
    MatTooltipModule,
    MatDividerModule
  ],
  templateUrl: './my-service.html',
  styleUrl: './my-service.css',
})
export class MyService implements OnInit {
  BASE_URL = API_BASE_URL;
  loading = false;

  partnerId: number | null = null;

  partnerDetail: IServiceTypeHierarchy[] = [];
  data: IServiceTypeHierarchy[] = [];

  currentServiceTypeId: number | null = null;

  openedCategoryIndex: number | null = 0;
  selectedCategoryIndex: number | null = null;
  selectedSubCategoryIndex: number | null = null;

  constructor(
    private dialog: MatDialog,
    private route: ActivatedRoute,
    private router: Router,
    private toaster: Toaster,
    private myService: PartnerMyService
  ) { }

  ngOnInit(): void {
    this.partnerId = Number(this.route.snapshot.paramMap.get('id')) || null;
    this.loadPartnerDetail();
  }

  get selectedCategory(): ICategory | null {
    if (this.selectedCategoryIndex === null) return null;
    return this.data[0]?.categories?.[this.selectedCategoryIndex] ?? null;
  }

  get selectedSubCategory(): ISubCategory | null {
    if (this.selectedCategoryIndex === null || this.selectedSubCategoryIndex === null) {
      return null;
    }

    return this.data[0]?.categories?.[this.selectedCategoryIndex]?.subCategories?.[this.selectedSubCategoryIndex] ?? null;
  }

  get selectedServices(): IServiceItem[] {
    return this.selectedSubCategory?.services ?? [];
  }

  toggleCategory(index: number): void {
    this.openedCategoryIndex =
      this.openedCategoryIndex === index ? null : index;
  }

  selectSubCategory(categoryIndex: number, subCategoryIndex: number): void {
    this.selectedCategoryIndex = categoryIndex;
    this.selectedSubCategoryIndex = subCategoryIndex;
    this.openedCategoryIndex = categoryIndex;
  }

  loadPartnerDetail(): void {
    this.loading = true;

    setTimeout(() => {

      this.myService.getPartnerServiceHierarchy().subscribe({
        next: (res) => {
          const response = res.data ?? [];

          this.partnerDetail = response;
          this.data = response;

          this.currentServiceTypeId = response[0]?.serviceTypeId ?? null;

          this.resetSelection();
        },
        error: (err) => {
          this.partnerDetail = [];
          this.data = [];
          this.toaster.error(err?.error?.message || 'Failed to load services');
        },
        complete: () => {
          this.loading = false;
        }
      });
    }, 600)
  }

  private resetSelection(): void {
    this.openedCategoryIndex = null;
    this.selectedCategoryIndex = null;
    this.selectedSubCategoryIndex = null;

    if (this.data.length > 0 && this.data[0].categories?.length > 0) {
      this.openedCategoryIndex = 0;

      if (this.data[0].categories[0].subCategories?.length > 0) {
        this.selectedCategoryIndex = 0;
        this.selectedSubCategoryIndex = 0;
      }
    }
  }

  addSubCategoryDialog(): void {
    const dialogRef = this.dialog.open(ManageServiceDialog, {
      width: '700px',
      data: {
        serviceTypeId: this.currentServiceTypeId
      }
    });

    dialogRef.afterClosed().subscribe(() => {
      this.loadPartnerDetail();
    });
  }

  goToServiceDetail(serviceId: number): void {
    this.router.navigate(['/service-partner/service-detail', serviceId]);
  }
}