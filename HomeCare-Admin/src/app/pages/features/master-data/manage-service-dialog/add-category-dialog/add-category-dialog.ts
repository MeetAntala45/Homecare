import { CommonModule } from '@angular/common';
import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogModule, MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { CategoryService } from './../../../../../core/services/category/category-service';
import { ICategory } from '../../../../../core/models/master-data/category/category';
import { Toaster } from '../../../../../core/services/toaster/toaster';

@Component({
  selector: 'app-add-category-dialog',
  imports: [CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule],
  templateUrl: './add-category-dialog.html',
  styleUrl: './add-category-dialog.css',
})
export class AddCategoryDialog {
  categoryfield!: FormGroup;

  constructor(private fb: FormBuilder,
    private dialogRef: MatDialogRef<AddCategoryDialog>,
    private CategoryService: CategoryService,
    @Inject(MAT_DIALOG_DATA) public data: ICategory,
    private toaster: Toaster
  ) { }

  ngOnInit(): void {
    this.initForm();
  }

  initForm() {
    this.categoryfield = this.fb.group({
      serviceTypeId: [this.data.serviceTypeId],
      category: ['', Validators.required]
    });
  }

  onSave() {
    if (this.categoryfield.invalid) {
      this.categoryfield.markAllAsTouched();
      return;
    }

    const category = {
      id: 0,
      name: this.categoryfield.value.category,
      serviceTypeId: Number(this.categoryfield.value.serviceTypeId)
    };

    this.dialogRef.close(category);
  }

  onCancel() {
    this.dialogRef.close();
  }
}
