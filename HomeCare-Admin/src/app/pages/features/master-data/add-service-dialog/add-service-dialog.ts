import { Component } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { MasterDataService } from '../../../../core/services/master-data/master-data-service';
import { Toaster } from '../../../../core/services/toaster/toaster';

@Component({
  selector: 'app-add-service-dialog',
  templateUrl: './add-service-dialog.html',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ]
})
export class AddServiceDialog {

  serviceForm!: FormGroup;
  selectedFile: File | null = null;
  imagePreview: string | ArrayBuffer | null = null;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<AddServiceDialog>,
    private serviceTypeService: MasterDataService,
    private toaster : Toaster
  ) { }

  ngOnInit(): void {
    this.initForm();
  }

  initForm() {
    this.serviceForm = this.fb.group({
      name: ['', Validators.required]
    });

  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;

    if (input.files && input.files.length > 0) {
      this.selectedFile = input.files[0];

      const reader = new FileReader();
      reader.onload = () => {
        this.imagePreview = reader.result;
      };

      reader.readAsDataURL(this.selectedFile);
    }
  }

  onSave() {
    if (this.serviceForm.invalid) {
      this.serviceForm.markAllAsTouched();
      return;
    }

    const formData = new FormData();
    formData.append('name', this.serviceForm.value.name);
    if (this.selectedFile) {
      formData.append('image', this.selectedFile);
    }

    this.serviceTypeService.addService(formData).subscribe({
      next: (res) => {
        if (res.success) {
          this.dialogRef.close(true);
          this.toaster.success(res.message);
        }
      },
      error: (err) => {    
        if (err?.message) {
          this.toaster.error(err.message);
        }
      }
    });
  }

  onCancel() {
    this.dialogRef.close();
  }
}