import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { MasterDataService } from '../../../../core/services/master-data/master-data-service';
import { IServiceType } from '../../../../core/models/master-data/service-type/service-type';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { API_BASE_URL } from '../../../../core/constants/environment-config';

@Component({
  selector: 'app-edit-service-dialog',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatFormFieldModule,
    MatInputModule,
    MatButtonModule,
    MatIconModule
  ],
  templateUrl: './edit-service-dialog.html',
  styleUrl: './edit-service-dialog.css',
})
export class EditServiceDialog {

  BASE_URL = API_BASE_URL;

  serviceForm!: FormGroup;
  selectedFile: File | null = null;
  imagePreview: string | ArrayBuffer | null = null;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<EditServiceDialog>,
    private serviceTypeService: MasterDataService,
    @Inject(MAT_DIALOG_DATA) public data: IServiceType,
    private toaster : Toaster
  ) { }

  ngOnInit(): void {
    this.initForm();
    if (this.data?.imagePath) {
      this.imagePreview = `${this.BASE_URL}/${this.data.imagePath}`;
    }
  }

  initForm() {
    this.serviceForm = this.fb.group({
      id: [this.data.id],
      name: [this.data.name, Validators.required],
      imagePath: [this.data.imagePath]
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

    this.serviceTypeService.editServiceType(this.serviceForm.value.id, formData).subscribe({
        next: (res) => {
          if (res.success) {
            this.dialogRef.close(true);
            this.toaster.success(res.message);
          }
        },
        error: (err) => {    
          if (err.message) {
            this.toaster.error(err.message);
          }
        }
      });
  }

  onCancel() {
    this.dialogRef.close();
  }
}
