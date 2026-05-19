import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { ReactiveFormsModule } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import {
  MAT_DIALOG_DATA,
  MatDialog,
  MatDialogModule,
  MatDialogRef
} from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { CategoryService } from '../../../../core/services/category/category-service';
import { SubCategoryService } from '../../../../core/services/sub-category/sub-category-service';
import { AddCategoryDialog } from './add-category-dialog/add-category-dialog';
import { AddSubCategoryDialog } from './add-subcategory-dialog/add-subcategory-dialog';
import { DeleteConfirmation } from '../../../../shared/components/delete-confirmation/delete-confirmation';
import { ICategory } from '../../../../core/models/master-data/category/category';
import { ISubCategory } from '../../../../core/models/master-data/sub-category/sub-category';
import { IApiResponse } from '../../../../core/models/api-response/api-response';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { MASTER_DATA_MESSAGES } from '../../../../core/constants/master-data-messages';

@Component({
  selector: 'app-manage-service-dialog',
  imports: [
    MatDialogModule,
    MatIconModule,
    MatButtonModule,
    MatFormFieldModule,
    MatInputModule,
    ReactiveFormsModule,
    CommonModule
  ],
  templateUrl: './manage-service-dialog.html',
  styleUrl: './manage-service-dialog.css',
})
export class ManageServiceDialog {
  categories: ICategory[] = [];
  subCategories: ISubCategory[] = [];
  subCategoryMap: Record<number, ISubCategory[]> = {};
  selectedCategoryId!: number;

  constructor(
    private dialogRef: MatDialogRef<ManageServiceDialog>,
    private category: CategoryService,
    private subcategories: SubCategoryService,
    @Inject(MAT_DIALOG_DATA) public data: { id: number; name: string },
    private dialog: MatDialog,
    private toaster: Toaster
  ) { }

  ngOnInit() {
    this.loadCategories();
  }

  private normalizeName(name: string | undefined | null): string {
    return (name || '').trim().toLowerCase();
  }

  private isDuplicateCategoryName(name: string): boolean {
    const normalizedName = this.normalizeName(name);

    return this.categories.some(
      category => this.normalizeName(category.name) === normalizedName
    );
  }

  private isDuplicateSubCategoryName(categoryId: number, name: string): boolean {
    const normalizedName = this.normalizeName(name);
    const subCategories = this.subCategoryMap[categoryId] || [];

    return subCategories.some(
      subCategory => this.normalizeName(subCategory.name) === normalizedName
    );
  }

  loadCategories() {
    this.category.getCategoryById(this.data.id).subscribe({
      next: (res: IApiResponse<ICategory[]>) => {
        this.categories = res.data || [];

        if (this.categories.length > 0) {
          this.selectedCategoryId = this.categories[0].id!;
          this.loadSubCategories(this.selectedCategoryId);
        }
      },
      error: (err) => {
        this.toaster.error(err?.error?.message);
      }
    });
  }

  selectCategory(categoryId: number) {
    this.selectedCategoryId = categoryId;
    this.loadSubCategories(categoryId);
  }

  loadSubCategories(categoryId: number) {
    if (this.subCategoryMap[categoryId]) {
      this.subCategories = [...this.subCategoryMap[categoryId]];
      return;
    }

    this.subcategories.getSubCategories(categoryId).subscribe({
      next: (res: IApiResponse<ISubCategory[]>) => {
        const data = res.data || [];
        this.subCategoryMap[categoryId] = [...data];
        this.subCategories = [...data];
      },
      error: (err) => {
        this.toaster.error(err?.error?.message);
      }
    });
  }

  addCategoryDialog() {
    const dialogRef = this.dialog.open(AddCategoryDialog, {
      width: '400px',
      data: { serviceTypeId: this.data.id }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        if (this.isDuplicateCategoryName(result.name)) {
          this.toaster.error(MASTER_DATA_MESSAGES.category.EXISTS);
          return;
        }

        const tempId = Date.now();

        const newCategory: ICategory = {
          ...result,
          id: tempId,
          isNew: true
        };

        this.categories.push(newCategory);
        this.subCategoryMap[tempId] = [];
        this.selectedCategoryId = tempId;
        this.subCategories = [];
        this.toaster.success(MASTER_DATA_MESSAGES.category.ADDED);
      }
    });
  }

  addSubCategoryDialog() {
    if (!this.selectedCategoryId) {
      this.toaster.error(MASTER_DATA_MESSAGES.common.SELECT_CATEGORY_FIRST);
      return;
    }

    const dialogRef = this.dialog.open(AddSubCategoryDialog, {
      width: '400px',
      data: { categoryId: this.selectedCategoryId }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        if (this.isDuplicateSubCategoryName(this.selectedCategoryId, result.name)) {
          this.toaster.error(MASTER_DATA_MESSAGES.subcategory.EXISTS);
          return;
        }

        const newSubCategory: ISubCategory = {
          ...result,
          id: Date.now(),
          categoryId: this.selectedCategoryId,
          isNew: true
        };

        if (!this.subCategoryMap[this.selectedCategoryId]) {
          this.subCategoryMap[this.selectedCategoryId] = [];
        }

        this.subCategoryMap[this.selectedCategoryId].push(newSubCategory);
        this.subCategories = [...this.subCategoryMap[this.selectedCategoryId]];

        this.toaster.success(MASTER_DATA_MESSAGES.subcategory.ADDED);
      }
    });
  }

  deleteCategory(categoryId: number, name: string) {
    const selectedCategory = this.categories.find(c => c.id === categoryId);

    if (selectedCategory?.isNew) {
      if ((this.subCategoryMap[categoryId]?.length ?? 0) > 0) {
        this.toaster.error(MASTER_DATA_MESSAGES.category.HAS_SUBCATEGORY);
        return;
      }

      this.categories = this.categories.filter(c => c.id !== categoryId);
      delete this.subCategoryMap[categoryId];
      this.toaster.success(MASTER_DATA_MESSAGES.category.DELETED);
      return;
    }

    if (this.subCategoryMap[categoryId] === undefined) {
      this.subcategories.getSubCategories(categoryId).subscribe({
        next: (res) => {
          this.subCategoryMap[categoryId] = res.data || [];
          this.handleCategoryDelete(categoryId, name);
        },
        error: () => {
          this.toaster.error('Failed to check subcategories.');
        }
      });
      return;
    }

    this.handleCategoryDelete(categoryId, name);
  }

  private handleCategoryDelete(categoryId: number, name: string) {
    if ((this.subCategoryMap[categoryId]?.length ?? 0) > 0) {
      this.toaster.error(MASTER_DATA_MESSAGES.category.HAS_SUBCATEGORY);
      return;
    }

    const dialogRef = this.dialog.open(DeleteConfirmation, {
      width: '400px',
      data: {
        message: MASTER_DATA_MESSAGES.conformation.CATEGORY_DELETED(name),
        apiCall: () => this.category.deleteCategory(categoryId)
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.categories = this.categories.filter(c => c.id !== categoryId);
        delete this.subCategoryMap[categoryId];

        if (this.selectedCategoryId === categoryId) {
          if (this.categories.length > 0) {
            this.selectedCategoryId = this.categories[0].id!;
            this.loadSubCategories(this.selectedCategoryId);
          } else {
            this.selectedCategoryId = 0;
            this.subCategories = [];
          }
        }
      }
    });
  }

  deleteSubCategory(subCategoryId: number, name: string) {
    const selectedSubCategory = this.subCategories.find(s => s.id === subCategoryId);

    if (selectedSubCategory?.isNew) {
      this.subCategoryMap[this.selectedCategoryId] =
        (this.subCategoryMap[this.selectedCategoryId] || []).filter(s => s.id !== subCategoryId);

      this.toaster.success(MASTER_DATA_MESSAGES.subcategory.DELETED);
      this.subCategories = [...this.subCategoryMap[this.selectedCategoryId]];
      return;
    }

    const dialogRef = this.dialog.open(DeleteConfirmation, {
      width: '400px',
      data: {
        message: MASTER_DATA_MESSAGES.conformation.SUBCATEGORY_DELETED(name),
        apiCall: () => this.subcategories.deleteSubCategory(subCategoryId)
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.subCategoryMap[this.selectedCategoryId] =
          (this.subCategoryMap[this.selectedCategoryId] || []).filter(s => s.id !== subCategoryId);

        this.subCategories = [...this.subCategoryMap[this.selectedCategoryId]];
      }
    });
  }

  onSave() {
    const newCategories = this.categories.filter(c => c.isNew);
    const newSubCategories = Object.values(this.subCategoryMap)
      .flat()
      .filter(s => s.isNew);

    const hasNewCategories = newCategories.length > 0;
    const hasNewSubCategories = newSubCategories.length > 0;
    const isBothAdded = hasNewCategories && hasNewSubCategories;

    if (!hasNewCategories && !hasNewSubCategories) {
      this.dialogRef.close(true);
      return;
    }

    if (hasNewCategories) {
      let savedCategoriesCount = 0;
      let hasCategoryError = false;

      newCategories.forEach(category => {
        this.category.addCategory({
          name: category.name,
          serviceTypeId: category.serviceTypeId
        }).subscribe({
          next: (res) => {
            if (!res.success) {
              hasCategoryError = true;
              this.toaster.error(res.message || MASTER_DATA_MESSAGES.category.SAVE_FAILED);
              return;
            }

            if (!isBothAdded) {
              this.toaster.success(res.message);
            }

            savedCategoriesCount++;

            if (savedCategoriesCount === newCategories.length && !hasCategoryError) {
              this.mapAndSaveSubCategories(newCategories, newSubCategories, isBothAdded);
            }
          },
          error: (err) => {
            hasCategoryError = true;
            this.toaster.error(err.error?.message || MASTER_DATA_MESSAGES.category.SAVE_FAILED);
          }
        });
      });
    } else {
      this.mapAndSaveSubCategories([], newSubCategories, isBothAdded);
    }
  }

  mapAndSaveSubCategories(
    newCategories: ICategory[],
    newSubCategories: ISubCategory[],
    isBothAdded: boolean
  ) {
    if (newSubCategories.length === 0) {
      this.category.getCategoryById(this.data.id).subscribe({
        next: (res: IApiResponse<ICategory[]>) => {
          const savedCategories = res.data || [];
          const matched = savedCategories.find(
            x => this.normalizeName(x.name) === this.normalizeName(
              this.categories.find(c => c.id === this.selectedCategoryId)?.name
            )
          );
          const realCategoryId = matched?.id ?? savedCategories[0]?.id ?? null;
          this.dialogRef.close({ success: true, categoryId: realCategoryId });
        },
        error: () => {
          this.dialogRef.close({ success: true, categoryId: null });
        }
      });
      return;
    }

    this.category.getCategoryById(this.data.id).subscribe({
      next: (res: IApiResponse<ICategory[]>) => {
        const savedCategories = res.data || [];
        const categoryIdMap = new Map<number, number>();

        newCategories.forEach(localCategory => {
          const matchedCategory = savedCategories.find(
            x => this.normalizeName(x.name) === this.normalizeName(localCategory.name)
          );

          if (matchedCategory?.id) {
            categoryIdMap.set(localCategory.id!, matchedCategory.id);
          }
        });

        this.saveSubCategories(newSubCategories, categoryIdMap, isBothAdded);
      },
      error: (err) => {
        this.toaster.error(MASTER_DATA_MESSAGES.common.RELOAD_FAILED);
      }
    });
  }

  saveSubCategories(
    newSubCategories: ISubCategory[],
    categoryIdMap: Map<number, number>,
    isBothAdded: boolean
  ) {
    if (newSubCategories.length === 0) {
      this.dialogRef.close(true);
      return;
    }

    let savedSubCategoriesCount = 0;
    let hasSubCategoryError = false;

    newSubCategories.forEach(subCategory => {
      const realCategoryId =
        categoryIdMap.get(subCategory.categoryId!) || subCategory.categoryId;

      this.subcategories.addSubCategory({
        name: subCategory.name,
        categoryId: realCategoryId
      }).subscribe({
        next: (res) => {
          if (!res.success) {
            hasSubCategoryError = true;
            this.toaster.error(res.message || MASTER_DATA_MESSAGES.subcategory.SAVE_FAILED);
            return;
          }

          savedSubCategoriesCount++;

          if (savedSubCategoriesCount === newSubCategories.length && !hasSubCategoryError) {
            if (isBothAdded) {
              this.toaster.success(MASTER_DATA_MESSAGES.common.DATA_ADDED);
            } else {
              this.toaster.success(res.message);
            }

            const realSelectedId = categoryIdMap.get(this.selectedCategoryId) ?? this.selectedCategoryId;
            this.dialogRef.close({ success: true, categoryId: realSelectedId });
          }
        },
        error: (err) => {
          hasSubCategoryError = true;
          this.toaster.error(err.error?.message || MASTER_DATA_MESSAGES.subcategory.SAVE_FAILED);
        }
      });
    });
  }

  onClose() {
    this.dialogRef.close(null);
  }
}