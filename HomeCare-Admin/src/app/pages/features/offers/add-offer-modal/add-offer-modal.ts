import { Component, inject, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule,
  FormBuilder,
  FormGroup,
  FormArray,
  Validators,
} from '@angular/forms';
import { MatDialogRef, MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatIconModule } from '@angular/material/icon';
import { MatTooltipModule } from '@angular/material/tooltip';
import { OffersService } from '../../../../core/services/offers/offers-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { AddConditionTypeModal } from '../add-condition-type-modal/add-condition-type-modal';
import { IConditionTypeResponse } from '../../../../core/models/offers/condition-type.response';
import { ICreateOfferRequest } from '../../../../core/models/offers/create-offer.request';
import { IOfferDropdownOption, IOfferInputFieldConfig } from '../../../../core/models/offers/offer-input-field-config';
import { FAIL_BEHAVIOUR_OPTIONS, INPUT_TYPE_OPERATORS, OPERATOR_LABEL_MAP } from '../../../../core/constants/offer-constants';
import { OFFERS_MESSAGES } from '../../../../core/constants/offer-messages';
import { AsFormGroupPipe } from '../../../../shared/pipes/form-group/as-form-group-pipe';
import { AsFormControlpipePipe } from '../../../../shared/pipes/form-control/as-form-controlpipe-pipe';
import { OfferInputFields } from '../offer-input-fields/offer-input-fields';
import { MatNativeDateModule } from '@angular/material/core';
import { dateTimeRangeValidator } from '../../../../core/validators/dateTimeRangeValidator';
import { IServiceTypeGroup } from '../../../../core/models/offers/service-type-hierarchy.model';

@Component({
  selector: 'app-add-offer-modal',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatDialogModule,
    MatIconModule,
    MatTooltipModule,
    OfferInputFields,
    AsFormControlpipePipe,
    AsFormGroupPipe,
    MatNativeDateModule
  ],
  templateUrl: './add-offer-modal.html',
  styleUrl: './add-offer-modal.css',
})
export class AddOfferModal implements OnInit {
  private fb = inject(FormBuilder);
  private dialogRef = inject(MatDialogRef<AddOfferModal>);
  private offersService = inject(OffersService);
  private toaster = inject(Toaster);
  private dialog = inject(MatDialog);
  serviceTypeHierarchy: IServiceTypeGroup[] = [];

  isLoading = false;
  conditionTypes: IConditionTypeResponse[] = [];
  failBehaviourOptions = FAIL_BEHAVIOUR_OPTIONS;

  readonly ADD_TYPE_VALUE = '__add_type__';

  form: FormGroup = this.fb.group({
    offerCode: ['', [Validators.required, Validators.maxLength(50)]],
    description: ['', Validators.maxLength(200)],
    discountPct: [null, [Validators.required, Validators.min(1), Validators.max(100)]],
    conditions: this.fb.array([]),
  });

  get conditionsArray(): FormArray {
    return this.form.get('conditions') as FormArray;
  }

  ngOnInit(): void {
    this.loadConditionTypes();
    this.loadServiceTypeHierarchy();
  }

  loadConditionTypes(): void {
    this.offersService.getActiveConditionTypes().subscribe({
      next: (res) => {
        if (res.success && res.data) this.conditionTypes = res.data;
      },
      error: () => this.toaster.error(OFFERS_MESSAGES.CONDITION_TYPES.LOAD_FAILED),
    });
  }

  get conditionTypeOptions() {
    return [
      ...this.conditionTypes.map(ct => ({ label: ct.label, value: ct.id })),
      { label: '+ Add Type', value: this.ADD_TYPE_VALUE },
    ];
  }

  loadServiceTypeHierarchy(): void {
    this.offersService.getServiceTypeHierarchy().subscribe({
      next: (res) => {
        if (res.success && res.data) this.serviceTypeHierarchy = res.data;
      },
      error: () => this.toaster.error('Failed to load service categories.'),
    });
  }

  getConditionType(index: number): IConditionTypeResponse | undefined {
    const typeId = this.conditionsArray.at(index).get('conditionTypeId')?.value;
    return this.conditionTypes.find(ct => ct.id === typeId);
  }

  getInputType(index: number): string {
    return this.getConditionType(index)?.inputType ?? 'text';
  }

  getOperatorOptions(index: number): IOfferDropdownOption[] {
    const inputType = this.getInputType(index);
    const allowed = INPUT_TYPE_OPERATORS[inputType] ?? [];
    return allowed.map(op => ({
      label: OPERATOR_LABEL_MAP[op],
      value: op,
    }));
  }

  getOperatorValue(index: number): string {
    return this.conditionsArray.at(index).get('operator')?.value ?? '';
  }

  isBetween(index: number): boolean {
    return this.getOperatorValue(index) === 'between';
  }

  getValueFieldType(index: number): IOfferInputFieldConfig['type'] {
    switch (this.getInputType(index)) {
      case 'number': return 'number';
      case 'date': return 'date';
      case 'time': return 'time';
      case 'days': return 'multi-select';
      case 'subcategory': return 'subcategory-select';
      default:            return 'text';
    }
  }

  buildConditionGroup(defaultOperator = ''): FormGroup {
    return this.fb.group({
      conditionTypeId: [null, Validators.required],
      operator: [defaultOperator, Validators.required],
      value: [''],
      valueFrom: [''],
      valueTo: [''],
      failBehaviour: ['', Validators.required],
    },
    {
      validators: dateTimeRangeValidator
    });
  }

  addCondition(): void {
    this.conditionsArray.push(this.buildConditionGroup());
  }

  removeCondition(index: number): void {
    this.conditionsArray.removeAt(index);
  }

  onConditionTypeChange(value: any, index: number): void {
    if (value === this.ADD_TYPE_VALUE) {
      this.conditionsArray.at(index).get('conditionTypeId')?.setValue(null);

      this.dialog
        .open(AddConditionTypeModal, { width: '480px', disableClose: true })
        .afterClosed()
        .subscribe((result) => {
          if (!result?.created) return;
          this.offersService.getActiveConditionTypes().subscribe({
            next: (res) => {
              if (!res.success || !res.data) return;
              this.conditionTypes = res.data;
              const newest = res.data[res.data.length - 1];
              this.conditionsArray.at(index).get('conditionTypeId')?.setValue(newest.id);
              this.updateValueValidators(index);
            },
          });
        });
      return;
    }

    const ct = this.conditionTypes.find(c => c.id === value);
    if (ct) {
      this.conditionsArray.at(index).get('operator')?.setValue(ct.defaultOperator);
    }
    this.updateValueValidators(index);
  }

  updateValueValidators(index: number): void {
    const group = this.conditionsArray.at(index) as FormGroup;
    const isBetween = this.isBetween(index);

    group.get('value')?.clearValidators();
    group.get('valueFrom')?.clearValidators();
    group.get('valueTo')?.clearValidators();
    group.get('value')?.setValue('');
    group.get('valueFrom')?.setValue('');
    group.get('valueTo')?.setValue('');

    if (isBetween) {
      group.get('valueFrom')?.setValidators(Validators.required);
      group.get('valueTo')?.setValidators(Validators.required);
    } else {
      group.get('value')?.setValidators(Validators.required);
    }

    group.get('value')?.updateValueAndValidity();
    group.get('valueFrom')?.updateValueAndValidity();
    group.get('valueTo')?.updateValueAndValidity();
  }

  onOperatorChange(index: number): void {
    this.updateValueValidators(index);
  }

  getControl(group: FormGroup, name: string) {
    return group.get(name) as any;
  }

  private validateDuplicateConditions(): string | null {
    const ids = this.conditionsArray.controls.map(c => c.get('conditionTypeId')?.value);
    const seen = new Set();
    for (const id of ids) {
      if (seen.has(id)) {
        const label = this.conditionTypes.find(ct => ct.id === id)?.label ?? 'Unknown';
        return OFFERS_MESSAGES.VALIDATION.DUPLICATE_CONDITION(label);
      }
      seen.add(id);
    }
    return null;
  }

  onSave(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    const duplicateError = this.validateDuplicateConditions();
    if (duplicateError) {
      this.toaster.error(duplicateError);
      return;
    }

    this.isLoading = true;
    this.form.disable();

    const formValue = this.form.getRawValue();

    const req: ICreateOfferRequest = {
      offerCode: formValue.offerCode,
      description: formValue.description,
      discountPct: Number(formValue.discountPct),
      conditions: formValue.conditions.map((c: any, i: number) => ({
        conditionTypeId: c.conditionTypeId,
        operator: c.operator,
        value: c.operator === 'between'
          ? `${this.formatValue(c.valueFrom, i)},${this.formatValue(c.valueTo, i)}`
          : this.formatValue(c.value, i),
        failBehaviour: c.failBehaviour,
      })),
    };

    this.offersService.create(req).subscribe({
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
        this.toaster.error(err.message ?? OFFERS_MESSAGES.GENERIC_ERROR);
      },
    });
  }

  private formatValue(val: any, index: number): string {
    const inputType = this.getInputType(index);

    if (inputType === 'date' && val instanceof Date) {
      const y = val.getFullYear();
      const m = String(val.getMonth() + 1).padStart(2, '0');
      const d = String(val.getDate()).padStart(2, '0');
      return `${y}-${m}-${d}`;
    }
    
    if (inputType === 'time') {
      if (!val) return '';
      const [time, period] = val.split('|');
      let [hour, minute] = time.split(':').map(Number);
      if (period === 'PM' && hour < 12)
        hour += 12;
      if (period === 'AM' && hour === 12)
        hour = 0;
      return `${String(hour).padStart(2, '0')}:${String(minute).padStart(2, '0')}`;
    }
    return String(val);
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}