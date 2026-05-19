import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { InputComponent } from '../input-component/input-component';
import { IFormModalData } from '../../../core/models/shared/IFormModalData.model';

@Component({
  selector: 'app-form-modal',
  standalone: true,
  imports: [
    ReactiveFormsModule,
    MatDialogModule,
    MatIconModule,
    InputComponent
  ],
  templateUrl: './form-modal.html',
  styleUrl: './form-modal.css'
})
export class FormModal {

  form!: FormGroup;
  isLoading = false;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<FormModal>,
    @Inject(MAT_DIALOG_DATA) public modalData: IFormModalData
  ) {
    const controls: any = {};

    modalData.fields.forEach(field => {
      const validators = [];

      if (field.required)
        validators.push(Validators.required);

      if (field.type === 'email')
        validators.push(Validators.email);

      if (field.type === 'text') {
        validators.push(Validators.maxLength(field.maxLength ?? 100));
      }

      if (field.type === 'tel' || field.name === 'mobileNumber')
        validators.push(Validators.pattern(/^[1-9]\d{9}$/));

      if (field.type === 'number') {
        validators.push(Validators.min(1));
        validators.push(Validators.max(100));
      }

      if (field.type === 'otp') {
        validators.push(Validators.required);
      }

      const defaultValue = field.type === 'number' ? null : '';

      controls[field.name] = [defaultValue, validators];
    });
    this.form = this.fb.group(controls);
    if (modalData.initialData) {
      this.form.patchValue(modalData.initialData);
    }
  }

  getControl(name: string) {
    return this.form.get(name) as any;
  }

  getOptions(name: string): { label: string; value: any }[] {
    return this.modalData.fields.find(f => f.name === name)?.options ?? [];
  }

  get loadingLabel(): string {
    const label = this.modalData.submitLabel ?? 'Save';
    const overrides: Record<string, string> = {
      'Add': 'Adding...',
      'Update': 'Updating...',
      'Save': 'Saving...',
      'Get OTP': 'Sending OTP...',
    };
    return overrides[label] ?? (label.endsWith('e') ? label.slice(0, -1) + 'ing...' : label + 'ing...');
  }

  onSaveClick() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.form.disable();

    this.modalData.onSubmit(this.form.value).subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res && res.success === false) {
          this.form.enable();
          this.form.markAllAsTouched();
        }
      },
      error: () => {
        this.isLoading = false;
        this.form.enable();
        if (this.modalData.initialData) {
          this.form.patchValue(this.modalData.initialData);
        }
      }
    });
  }

  onCancelClick() {
    this.dialogRef.close();
  }
}