import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { SubCategoryService } from './../../../../../core/services/sub-category/sub-category-service';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { ISubCategory } from '../../../../../core/models/master-data/sub-category/sub-category';
import { Toaster } from '../../../../../core/services/toaster/toaster';

@Component({
  selector: 'app-add-subcategory-dialog',
  imports: [CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule],
  templateUrl: './add-subcategory-dialog.html',
  styleUrl: './add-subcategory-dialog.css',
})
export class AddSubCategoryDialog {
  subCategoryfield!: FormGroup;

  constructor(private fb: FormBuilder,
    private dialogRef: MatDialogRef<AddSubCategoryDialog>,
    private SubCategoryService: SubCategoryService,
    @Inject(MAT_DIALOG_DATA) public data: ISubCategory,
    private toaster : Toaster
  ) { }

  ngOnInit(): void {
    this.initForm();
  }

  initForm() {
    this.subCategoryfield = this.fb.group({
      categoryId: [this.data.categoryId],
      SubCategory: ['', Validators.required]
    });
  }

  onSave() {
    if (this.subCategoryfield.invalid) {
      this.subCategoryfield.markAllAsTouched();
      return;
    }

    const subcategory = {
      id: 0,
      name: this.subCategoryfield.value.SubCategory,
      categoryId: Number(this.subCategoryfield.value.categoryId)
    };

    this.dialogRef.close(subcategory);
  }

  onCancel() {
    this.dialogRef.close();
  }
}
