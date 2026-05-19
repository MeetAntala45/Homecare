import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { InputComponent } from '../input-component/input-component';
import { IFormModalData } from '../../../core/models/shared-components/IFormModalData.model';

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

  form: FormGroup;
  selectedFile: File | null = null;
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

      if (field.type === 'password')
        validators.push(Validators.pattern(/^(?=.*[A-Za-z])(?=.*\d)(?=.*[@$!%*#?&]).{8,15}$/));

      if (field.type === 'number') {
        validators.push(Validators.min(1));
        validators.push(Validators.max(100));
      }

      const defaultValue = field.type === 'number' ? null : '';

      controls[field.name] = [defaultValue, validators];
    });

    this.form = this.fb.group(controls, { validators: this.passwordMatchValidator });

    if (modalData.initialData) {
      this.form.patchValue(modalData.initialData);
    }
  }

  passwordMatchValidator(group: FormGroup) {
    const password = group.get('password')?.value || group.get('newPassword')?.value;
    const confirmPassword = group.get('confirmPassword')?.value;
    if (!password || !confirmPassword) return null;
    return password === confirmPassword ? null : { passwordMismatch: true };
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
      'Change Password': 'Changing Password...',
      'Add':    'Adding...',
      'Update': 'Updating...',
      'Save':   'Saving...'
    };
    return overrides[label] ?? (label.endsWith('e') ? label.slice(0, -1) + 'ing...' : label + 'ing...');
  }

  onFileSelected(event: any) {
    this.selectedFile = event.target.files[0];
  }

  onSaveClick() {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    if (!this.modalData.onSubmit) {
      this.dialogRef.close({ formValue: this.form.value, file: this.selectedFile });
      return;
    }

    this.isLoading = true;
    this.form.disable();

    this.modalData.onSubmit(this.form.value).subscribe({
      next: () => {
        this.isLoading = false;
        this.dialogRef.close({ formValue: this.form.value, file: this.selectedFile });
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