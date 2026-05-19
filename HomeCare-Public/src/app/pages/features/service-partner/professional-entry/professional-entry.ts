import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormArray, FormGroup, AbstractControl, ValidationErrors } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { InputFields } from '../input-fields/input-fields';
import { AsFormControlpipePipe } from '../../../../shared/pipes/as-form-controlpipe-pipe';
import { MatTooltip } from '@angular/material/tooltip';
import { IInputFieldConfig } from '../../../../core/models/service-partner/IInputFieldConfig';
 
function dateRangeValidator(group: AbstractControl): ValidationErrors | null {
  const from = group.get('fromDate')?.value;
  const to = group.get('toDate')?.value;
 
  if (!from || !to) return null;
 
  const fromTime = new Date(from.getFullYear(), from.getMonth(), from.getDate()).getTime();
  const toTime = new Date(to.getFullYear(), to.getMonth(), to.getDate()).getTime();
 
  if (fromTime === toTime) {
    return { dateRangeEqual: true };
  }
 
  if (fromTime > toTime) {
    return { dateRangeInvalid: true };
  }
 
  return null;
}
@Component({
  selector: 'app-professional-entry',
  imports: [
    InputFields,
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    AsFormControlpipePipe,
    MatTooltip
  ],
  templateUrl: './professional-entry.html',
  styleUrl: './professional-entry.css',
})
export class ProfessionalEntry implements OnInit {
  @Input() professionalFormArray!: FormArray;
 
  professionalFields: IInputFieldConfig[] = [
    { type: 'text', label: 'Company Name', placeholder: 'Company name' },
    { type: 'text', label: 'Your Role', placeholder: 'Your role' },
    { type: 'date', label: 'From', placeholder: 'Start date' },
    { type: 'date', label: 'To', placeholder: 'End date' },
  ];
 
  controlNames = ['companyName', 'role', 'fromDate', 'toDate'];
 
  getProfessionalGroup(index: number): FormGroup {
    return this.professionalFormArray.at(index) as FormGroup;
  }
 
 
  ngOnInit(): void {
    this.applyValidatorsToAll();
 
    this.professionalFormArray.valueChanges.subscribe(() => {
      const count = this.professionalFormArray.length;
      for (let i = 0; i < count; i++) {
        const group = this.getProfessionalGroup(i);
        if (!group.hasValidator(dateRangeValidator)) {
          this.applyValidator(group);
        }
      }
    });
  }
 
  private applyValidatorsToAll(): void {
    for (let i = 0; i < this.professionalFormArray.length; i++) {
      this.applyValidator(this.getProfessionalGroup(i));
    }
  }
 
  private applyValidator(group: FormGroup): void {
    group.addValidators(dateRangeValidator);
    group.updateValueAndValidity({ emitEvent: false });
 
 
    group.get('fromDate')?.valueChanges.subscribe(() => {
      group.updateValueAndValidity({ emitEvent: false });
    });
 
    group.get('toDate')?.valueChanges.subscribe(() => {
      group.updateValueAndValidity({ emitEvent: false });
    });
  }
 
 
  hasDateRangeError(index: number): boolean {
    const group = this.getProfessionalGroup(index);
    return group.errors?.['dateRangeInvalid'] &&
           (group.get('fromDate')?.touched || group.get('toDate')?.touched)
           ? true : false;
  }
  hasDateEqualError(i: number): boolean {
    const group = this.getProfessionalGroup(i);
    return !!group.errors?.['dateRangeEqual'] &&
      (group.get('fromDate')?.touched || group.get('toDate')?.touched) ? true : false;
  }
}