import { Component, Inject, Input, OnInit, ViewEncapsulation } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormControl, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { IFilterPanelData } from '../../../core/models/shared-components/IFilterPanel';
import { MatInputModule } from '@angular/material/input';
import { MatSliderModule } from '@angular/material/slider';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { provideNativeDateAdapter } from '@angular/material/core';


@Component({
  selector: 'app-filter-panel',
  standalone: true,
  encapsulation: ViewEncapsulation.None,
  providers: [provideNativeDateAdapter()],
  imports: [
    CommonModule,
    FormsModule,
    MatDialogModule,
    MatSelectModule,
    MatFormFieldModule,
    MatIconModule,
    MatInputModule,
    MatSliderModule,
    MatDatepickerModule,
    MatNativeDateModule,
    ReactiveFormsModule,
    
  ],
  templateUrl: './filter-panel.html',
  styleUrl: './filter-panel.css'
})
export class FilterPanel implements OnInit {
  @Input() label = '';
  @Input() control!: FormControl;
  
  dateError: string | null = null;
  selectedValues: Record<string, any> = {};
  allowFuture?: boolean;
  constructor(
    private dialogRef: MatDialogRef<FilterPanel>,
    @Inject(MAT_DIALOG_DATA) public data: IFilterPanelData
  ) {
    if (data?.initialValues) {
      this.selectedValues = { ...data.initialValues };
    }
  }
  fieldControls: Record<string, FormControl> = {};

  ngOnInit(): void {
    this.data.fields.forEach(field => {
      if (field.type === 'date') {
        const initial = this.selectedValues[field.key]
          ? new Date(this.selectedValues[field.key])
          : null;
        this.fieldControls[field.key] = new FormControl(initial);
      }

      if (this.selectedValues[field.key] !== undefined) return;
       if (field.type === 'range') {
        this.selectedValues[field.key] = { min: field.min ?? 0, max: field.max ?? 100 };
      } else if (field.type === 'toggle') {
        this.selectedValues[field.key] = false;
      } else {
        this.selectedValues[field.key] = null;
      }
  });
  }

  onFilter() {
    let hasError = false;

    this.data.fields.forEach(field => {
      if (field.type === 'date') {
        const ctrl = this.fieldControls[field.key];
        ctrl.markAsTouched();
        ctrl.markAsDirty();

        this.selectedValues[field.key] = ctrl.value;

        if (ctrl.invalid) hasError = true;

        if (this.dateError) hasError = true;
      }
    });

    if (hasError) return;

    const cleaned: Record<string, any> = {};
    for (const key in this.selectedValues) {
      if (this.selectedValues[key] !== null && this.selectedValues[key] !== undefined) {
        cleaned[key] = this.selectedValues[key];
      }
    }
    this.dialogRef.close(cleaned);
  }

  onCancel() {
    this.resetFilters();
    this.dialogRef.close("reset");
  }

  private resetFilters() {
    this.selectedValues = {};
    this.dateError = null;

    this.data.fields.forEach(field => {
      if (field.type === 'range') {
        this.selectedValues[field.key] = { min: field.min ?? 0, max: field.max ?? 100 };
      } else if (field.type === 'toggle') {
        this.selectedValues[field.key] = false;
      } else {
        this.selectedValues[field.key] = null;
      }

      if (field.type === 'date' && this.fieldControls[field.key]) {
        this.fieldControls[field.key].reset();
      }
    });
  }
  onCancelButton(){
    this.dialogRef.close();
  }

  maxDate: Date = new Date();
  minDate: Date = new Date(1900, 0, 1);

  validateDate(value: string, allowFuture: boolean): string | null {
    if (!value) return null;
  
    const parts = value.split('/');
    if (parts.length !== 3) return 'Date must be in MM/DD/YYYY format';
  
    const month = Number(parts[0]);
    const day = Number(parts[1]);
    const year = Number(parts[2]);
  
    if (!month || !day || !year) return 'Invalid date';
  
    const daysInMonth = new Date(year, month, 0).getDate();
  
    if (month < 1 || month > 12) return 'Month must be between 01 and 12';
    if (day < 1 || day > daysInMonth) return `Day must be between 01 and ${daysInMonth}`;
    if (year < 1000) return 'Year must be valid';
  
    const enteredDate = new Date(year, month - 1, day);
    const today = new Date();
  
    if (!allowFuture && enteredDate > today) {
      return 'Future date not allowed';
    }
  
    return null;
  }

  onDateBlur(event: Event, key: string, allowFuture: boolean): void {
    const input = event.target as HTMLInputElement;
    const error = this.validateDate(input.value, allowFuture);
    const ctrl = this.fieldControls[key];
  
    if (error) {
      this.dateError = error;
      ctrl.setErrors({ invalidDate: true });
    } else {
      this.dateError = null;
      ctrl.setErrors(null);
    }
  }

  onDateInput(event: Event): void {
    const input = event.target as HTMLInputElement;

    let value = input.value.replace(/\D/g, '');

    if (value.length > 2) value = value.substring(0, 2) + '/' + value.substring(2);

    if (value.length > 5) value = value.substring(0, 5) + '/' + value.substring(5, 9);

    input.value = value;
  }
  onDateKeyPress(event: KeyboardEvent) {
    if (!/[0-9]/.test(event.key)) event.preventDefault();
  }

  getErrorMessage(): string {
    if (this.control.errors?.['matDatepickerParse']) {
      return 'Please enter date in DD/MM/YYYY format';
    }
    return 'Invalid value';
  }
}

