import { Component, Inject, OnInit, ViewChild, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { ErrorStateMatcher } from '@angular/material/core';
import { ServiceManagementService } from '../../../../core/services/service-management/service-management-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { ServiceDialogData } from '../../../../core/models/service-management/service';
import { MESSAGES } from '../../../../core/constants/service-management-messages';
import { API_BASE_URL } from '../../../../core/constants/environment-config';

export class AlwaysShowMatcher implements ErrorStateMatcher {
  private _show = false;
  setShow(v: boolean) {
    this._show = v;
  }
  isErrorState(): boolean {
    return this._show;
  }
}

@Component({
  selector: 'app-service-add-edit-dialog',
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatSlideToggleModule,
    MatIconModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
  ],
  templateUrl: './service-add-edit-dialog.html',
  styleUrl: './service-add-edit-dialog.css',
})
export class ServiceAddEditDialog implements OnInit {
  @ViewChild('imageInput') imageInput!: ElementRef<HTMLInputElement>;

  BASE_URL = API_BASE_URL;
  isEditMode = false;
  saving = false;
  submitted = false;

  form = {
    name: '',
    description: '', 
    subCategoryId: null as number | null,
    durationMin: null as number | null,
    price: null as number | null,
    commissionPct: null as number | null,
    isAvailable: false,
  };

  matchers: Record<string, AlwaysShowMatcher> = {
    name: new AlwaysShowMatcher(),
    description: new AlwaysShowMatcher(),
    subCategoryId: new AlwaysShowMatcher(),
    durationMin: new AlwaysShowMatcher(),
    price: new AlwaysShowMatcher(),
    commissionPct: new AlwaysShowMatcher(),
  };

  includeTags: string[] = [];
  excludeTags: string[] = [];
  includeInput = '';
  excludeInput = '';
  showIncludes = false;
  showExcludes = false;
  uploadedImages: { preview: string; file: File | null; isExisting?: boolean; path?: string }[] =
    [];

  constructor(
    public dialogRef: MatDialogRef<ServiceAddEditDialog>,
    @Inject(MAT_DIALOG_DATA) public data: ServiceDialogData,
    private svc: ServiceManagementService,
    private toaster: Toaster
  ) {}

  ngOnInit(): void {
    if (this.data.service) {
      this.isEditMode = true;
      this.loadServiceForEdit(this.data.service.id);
    }
  }

  private loadServiceForEdit(serviceId: number): void {
    this.svc.getServiceById(serviceId).subscribe({
      next: (res) => {
        const s: any = res.data;
        this.form = {
          name: s.name,
          description: s.description,
          subCategoryId: s.subCategoryId ?? null,
          durationMin: s.durationMin ?? null,
          price: s.price ?? null,
          commissionPct: s.commissionPct ?? null,
          isAvailable: s.isAvailable ?? false,
        };
        this.includeTags = s.inclusions ? [...s.inclusions] : [];
        this.excludeTags = s.exclusions ? [...s.exclusions] : [];
        if (this.includeTags.length > 0) this.showIncludes = true;
        if (this.excludeTags.length > 0) this.showExcludes = true;
        this.uploadedImages = (s.imagePaths ?? []).map((p: string) => ({
          preview: this.BASE_URL + p,
          file: null,
          isExisting: true,
          path: p,
        }));
      },
      error: () => this.toaster.error(MESSAGES.SERVICE.LOAD_DETAIL_FAILED),
    });
  }

  private isNullOrEmpty = (value: any): boolean =>
    value === null || value === undefined || String(value).trim() === '';

  private fieldValid: Record<string, () => boolean> = {
    name: () => !!this.form.name?.trim(),
    description: () => !!this.form.description?.trim() && this.form.description.length <= 300,
    subCategoryId: () => !!this.form.subCategoryId,
    durationMin: () =>
      !this.isNullOrEmpty(this.form.durationMin) && Number(this.form.durationMin) > 0,
    price: () => !this.isNullOrEmpty(this.form.price) && Number(this.form.price) > 0,
    commissionPct: () =>
      !this.isNullOrEmpty(this.form.commissionPct) &&
      Number(this.form.commissionPct) >= 0 &&
      Number(this.form.commissionPct) <= 100,
  };

  errorMsg(field: string): string {
    const map: Record<string, string> = {
      name: MESSAGES.VALIDATION.SERVICE_NAME_REQUIRED,
      subCategoryId: MESSAGES.VALIDATION.SELECT_SUB_CATEGORY,
      durationMin: MESSAGES.VALIDATION.DURATION_INVALID,
      price: MESSAGES.VALIDATION.PRICE_INVALID,
      commissionPct: MESSAGES.VALIDATION.COMMISSION_INVALID,
      description: MESSAGES.VALIDATION.DESCRIPTION_REQUIRED
    };
    return map[field] ?? '';
  }

  isFieldInvalid(field: string): boolean {
    return this.submitted && !this.fieldValid[field]();
  }

  onFieldChange(field: string): void {
    if (this.submitted && this.fieldValid[field]()) {
      this.matchers[field].setShow(false);
    }
  }

  private updateMatchers(): void {
    for (const field of Object.keys(this.matchers)) {
      this.matchers[field].setShow(!this.fieldValid[field]());
    }
  }

  isFormValid(): boolean {
    return Object.keys(this.fieldValid).every((f) => this.fieldValid[f]());
  }

  triggerImageUpload(): void {
    this.imageInput?.nativeElement?.click();
  }

  onImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files) return;
    const allowed = ['image/jpeg', 'image/png', 'image/webp', 'image/gif'];

    Array.from(input.files).forEach((file) => {
      if (!allowed.includes(file.type)) {
        this.toaster.error(MESSAGES.IMAGE.UNSUPPORTED_TYPE(file.name));
        return;
      }
      if (file.size > 5 * 1024 * 1024) {
        this.toaster.error(MESSAGES.IMAGE.SIZE_EXCEEDED(file.name));
        return;
      }
      const reader = new FileReader();
      reader.onload = (e) =>
        this.uploadedImages.push({ preview: e.target?.result as string, file, isExisting: false });
      reader.readAsDataURL(file);
    });
    input.value = '';
  }

  removeImage(i: number): void {
    this.uploadedImages.splice(i, 1);
  }

  addIncludeTag(): void {
    const v = this.includeInput.trim();
    if (v && !this.includeTags.includes(v)) this.includeTags.push(v);
    this.includeInput = '';
  }
  onIncludeKeydown(e: KeyboardEvent): void {
    if (e.key === 'Enter') {
      e.preventDefault();
      this.addIncludeTag();
    }
  }
  removeIncludeTag(i: number): void {
    this.includeTags.splice(i, 1);
  }

  addExcludeTag(): void {
    const v = this.excludeInput.trim();
    if (v && !this.excludeTags.includes(v)) this.excludeTags.push(v);
    this.excludeInput = '';
  }
  onExcludeKeydown(e: KeyboardEvent): void {
    if (e.key === 'Enter') {
      e.preventDefault();
      this.addExcludeTag();
    }
  }
  removeExcludeTag(i: number): void {
    this.excludeTags.splice(i, 1);
  }

  save(): void {
    this.submitted = true;
    this.updateMatchers();
    if (!this.isFormValid()) return;
    this.saving = true;
    this.submitUpsert();
  }

  private buildFormData(): FormData {
    const fd = new FormData();
    fd.append('name', this.form.name);
    fd.append('description', this.form.description);
    fd.append('subCategoryId', String(this.form.subCategoryId ?? ''));
    fd.append('durationMin', String(this.form.durationMin));
    fd.append('price', String(this.form.price));
    fd.append('commissionPct', String(this.form.commissionPct));
    fd.append('isAvailable', String(this.form.isAvailable));
    this.includeTags.forEach((t) => fd.append('Inclusions', t));
    this.excludeTags.forEach((t) => fd.append('Exclusions', t));
    return fd;
  }

  private submitUpsert(): void {
    const fd = this.buildFormData();

    if (this.isEditMode) {
      fd.append('Id', String(this.data.service!.id));
      this.uploadedImages
        .filter((i) => i.isExisting)
        .forEach((i) => fd.append('ExistingImagePaths', i.path!));
    }

    this.uploadedImages
      .filter((i) => !i.isExisting && i.file)
      .forEach((i) => fd.append('Images', i.file!));

    this.svc.upsertService(fd).subscribe({
      next: (res) => {
        this.saving = false;
        if (!res.success) {
          this.toaster.error(res.message ?? MESSAGES.SERVICE.OPERATION_FAILED);
          return;
        }
        this.toaster.success(res.message);
        this.dialogRef.close(true);
      },
      error: (err) => {
        this.saving = false;
        this.toaster.error(err?.error?.message ?? MESSAGES.SERVICE.OPERATION_FAILED);
        if (err?.error?.errors?.length) {
          err.error.errors.forEach((e: string) => this.toaster.error(e));
        }
      },
    });
  }

  cancel(): void {
    this.dialogRef.close(false);
  }
}
