import { Component, inject, OnInit } from '@angular/core';
import { CONTEXT_KEY_INPUT_TYPES, FAIL_BEHAVIOUR_OPTIONS, INPUT_TYPE_OPERATORS, INPUT_TYPE_OPTIONS, OPERATOR_LABEL_MAP, OPERATOR_OPTIONS } from '../../../../core/constants/offer-constants';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogModule, MatDialogRef } from '@angular/material/dialog';
import { OffersService } from '../../../../core/services/offers/offers-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { ICreateConditionTypeRequest } from '../../../../core/models/offers/create-condition-type.request';
import { OFFERS_MESSAGES } from '../../../../core/constants/offer-messages';
import { MatIcon, MatIconModule } from '@angular/material/icon';
import { CommonModule } from '@angular/common';
import { OfferInputFields } from '../offer-input-fields/offer-input-fields';
import { IOfferDropdownOption } from '../../../../core/models/offers/offer-input-field-config';

@Component({
  selector: 'app-add-condition-type-modal',
  imports: [CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatIconModule,
    OfferInputFields],
  templateUrl: './add-condition-type-modal.html',
  styleUrl: './add-condition-type-modal.css',
})
export class AddConditionTypeModal implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<AddConditionTypeModal>);
  private offersService = inject(OffersService);
  private toaster = inject(Toaster);

  contextKeyOptions: IOfferDropdownOption[] = [];
  filteredOperatorOptions: IOfferDropdownOption[] = [];
  filteredInputTypeOptions: IOfferDropdownOption[] = [];

  isLoading = false;
  failBehaviourOptions = FAIL_BEHAVIOUR_OPTIONS;
  inputTypeOptions = INPUT_TYPE_OPTIONS;

  ngOnInit() {
    this.loadContextKeys();
  }

  loadContextKeys(): void {
    this.offersService.getContextKeys().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.contextKeyOptions = res.data.map(key => ({
            label: this.formatContextKey(key),
            value: key,
          }));
        }
      },
      error: () => this.toaster.error(OFFERS_MESSAGES.CONDITION_TYPES.LOAD_FAILED),
    });
  }

  private formatContextKey(key: string): string {
    return key
      .replace(/_/g, ' ')          
      .replace(/\b\w/g, c => c.toUpperCase());
  }

  form: FormGroup = this.fb.group({
    label: ['', [
      Validators.required,
      Validators.maxLength(100),
    ]],
    contextKey: ['', Validators.required],
    inputType: ['', Validators.required],
    defaultOperator: ['', Validators.required],
    defaultFailBehaviour: ['', Validators.required],
  });

  onInputTypeChange(value: string): void {
    const allowed = INPUT_TYPE_OPERATORS[value] ?? [];
    this.filteredOperatorOptions = allowed.map(op => ({
      label: OPERATOR_LABEL_MAP[op],
      value: op,
    }));
    const current = this.form.get('defaultOperator')?.value;
    if (!allowed.includes(current)) {
      this.form.get('defaultOperator')?.setValue('');
    }
  }

  onContextKeyChange(value: string): void {
    const allowed = CONTEXT_KEY_INPUT_TYPES[value] ?? [];

    this.filteredInputTypeOptions = INPUT_TYPE_OPTIONS.filter(opt =>
      allowed.includes(opt.value as string)
    );

    const currentInputType = this.form.get('inputType')?.value;
    if (!allowed.includes(currentInputType)) {
      this.form.get('inputType')?.setValue('');
      this.filteredOperatorOptions = [];
      this.form.get('defaultOperator')?.setValue('');
    }
  }

  getControl(name: string) {
    return this.form.get(name) as any;
  }

  onSave(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.form.disable();

    const dto: ICreateConditionTypeRequest = {
      label: this.form.value.label.trim(),
      contextKey: this.form.value.contextKey,
      inputType: this.form.value.inputType,
      defaultOperator: this.form.value.defaultOperator,
      defaultFailBehaviour: this.form.value.defaultFailBehaviour,
    };

    this.offersService.createCondtitionType(dto).subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res.success) {
          this.toaster.success(res.message);
          this.dialogRef.close({ created: true });
        } else {
          this.form.enable();
          this.toaster.error(res.message);
        }
      },
      error: (err) => {
        this.isLoading = false;
        this.form.enable();
        this.toaster.error(err?.error?.message ?? OFFERS_MESSAGES.CONDITION_TYPES.CREATE_FAILED);
      },
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}