import { CommonModule } from '@angular/common';
import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialog, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatSelectModule } from '@angular/material/select';

import { Toaster } from '../../../../core/services/toaster/toaster';
import { IApiResponse } from '../../../../core/models/api-response/api-response';
import { ICategory, ISubCategory } from '../../../../core/models/service-management/service';
import { IService } from '../../../../core/models/service/IService';
import { Services } from '../../../../core/services/services/services';
import { PartnerMyService } from '../../../../core/services/my-service/my-service';
import { IServiceTypeHierarchy } from '../../../../core/models/my-service/my-service';
import { MatTooltipModule } from '@angular/material/tooltip';
import { ConfirmDialogComponent } from './confirm-dialog/confirm-dialog';

@Component({
  selector: 'app-add-subcategory-dialog',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule,
    MatSelectModule,
    MatTooltipModule
  ],
  templateUrl: './manage-service-dialog.html',
  styleUrl: './manage-service-dialog.css',
})
export class ManageServiceDialog implements OnInit {
  subCategoryForm!: FormGroup;

  activeCategoryId: number | null = null;

  categories: ICategory[] = [];
  subCategories: ISubCategory[] = [];
  services: IService[] = [];
  selectedSubCategories: ISubCategory[] = [];
  alreadyAssignedSubCategoryIds: number[] = [];

  loadingCategories = false;
  loadingSubCategories = false;
  loadingServices = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<ManageServiceDialog>,
    private servcie: Services,
    private myService: PartnerMyService,
    private toaster: Toaster,
    private dialog: MatDialog,
    @Inject(MAT_DIALOG_DATA) public data: { serviceTypeId?: number }
  ) { }

  ngOnInit(): void {
    this.subCategoryForm = this.fb.group({
      subCategoryIds: [[], Validators.required]
    });

    this.loadCategories();
    this.loadAssigned();

    this.subCategoryForm.get('categoryId')?.valueChanges.subscribe((categoryId) => {
      this.subCategoryForm.patchValue({ subCategoryId: null }, { emitEvent: false });
      this.subCategories = [];
      this.services = [];

      if (categoryId) {
        this.loadSubCategories(categoryId);
      }
    });

    this.subCategoryForm.get('subCategoryId')?.valueChanges.subscribe(() => {
      this.services = [];
    });
  }

  loadAssigned() {
    this.myService.getPartnerServiceHierarchy().subscribe(res => {
      const data: IServiceTypeHierarchy[] = res.data ?? [];

      this.alreadyAssignedSubCategoryIds =
        data.flatMap(serviceType =>
          (serviceType.categories ?? []).flatMap(category =>
            (category.subCategories ?? []).map(sub => sub.subCategoryId)
          )
        );

    });
  }

  loadCategories(): void {
    if (!this.data?.serviceTypeId) {
      this.toaster.error('Service type id not found');
      return;
    }

    this.loadingCategories = true;

    this.servcie.getCategoryById(this.data.serviceTypeId!).subscribe({
      next: (res: IApiResponse<ICategory[]>) => {
        this.categories = res.data || [];
        this.loadingCategories = false;

        if (!this.categories.length) {
          this.subCategories = [];
          return;
        }

        const firstCategory = this.categories[0];

        this.activeCategoryId = firstCategory.id;

        this.subCategoryForm.patchValue({
          categoryId: firstCategory.id
        });

        this.loadSubCategories(firstCategory.id);
      }
    });
  }

  loadSubCategories(categoryId: number): void {
    this.loadingSubCategories = true;

    this.servcie.getSubCategories(categoryId).subscribe({
      next: (res) => {
        this.subCategories = (res.data || []).map(sub => ({
          id: sub.id,
          name: sub.name,
          categoryId: categoryId
        }));

        this.loadingSubCategories = false;
      },
      error: (err) => {
        this.loadingSubCategories = false;
        this.toaster.error(err?.error?.message || 'Failed to load subcategories');
      }
    });
  }

  onSave(): void {
    if (this.selectedSubCategories.length === 0) {
      this.toaster.error('Please select at least one subcategory');
      return;
    }

    const payload = {
      categoryIds: [...new Set(this.selectedSubCategories.map(x => x.categoryId))],
      subCategoryIds: this.selectedSubCategories.map(x => x.id)
    };

    this.servcie.addSkillService(payload).subscribe({
      next: (res) => {
        this.toaster.success(res.message || 'Service added successfully');
        this.dialogRef.close(true);
      },
      error: (err) => {
        this.toaster.error(err?.error?.message || 'Failed to add service');
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }

  toggleSubCategory(sub: any) {
    const id = sub.id;

    const isAlreadyAssigned = this.alreadyAssignedSubCategoryIds.includes(id);
    const isNewSelection = this.selectedSubCategories.find(x => x.id === id);

    if (isAlreadyAssigned && !isNewSelection) {
      this.removeSubCategory(id);
      return;
    }

    if (isNewSelection) {
      this.selectedSubCategories =
        this.selectedSubCategories.filter(x => x.id !== id);
    } else {
      this.selectedSubCategories.push(sub);
    }

    this.subCategoryForm.patchValue({
      subCategoryIds: this.selectedSubCategories.map(x => x.id)
    });
  }

  removeSubCategory(subCategoryId: number) {
    this.myService.removeSkillService(subCategoryId).subscribe({
      next: () => {
        this.toaster.success('Removed successfully');

        this.alreadyAssignedSubCategoryIds =
          this.alreadyAssignedSubCategoryIds.filter(x => x !== subCategoryId);
      },
      error: (err) => {
        this.toaster.error(err?.error?.message || 'Failed to remove');
      }
    });
  }

  confirmRemove(subCategoryId: number) {
    const dialogRef = this.dialog.open(ConfirmDialogComponent, {
      width: '400px',
      data: {
        message: 'Are you sure you want to remove this subcategory? It will no longer be part of your services.'
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.removeSubCategory(subCategoryId);
      }
    });
  }

  onCategoryClick(categoryId: number): void {
    this.activeCategoryId = categoryId;

    this.subCategoryForm.patchValue({
      categoryId: categoryId
    });

    this.loadSubCategories(categoryId);
  }
}