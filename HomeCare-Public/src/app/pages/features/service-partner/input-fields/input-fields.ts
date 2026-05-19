import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormControl } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule } from '@angular/material/core';
import { IDropdownOption } from '../../../../core/models/service-partner/service-partner';
import { IInputFieldConfig } from '../../../../core/models/service-partner/IInputFieldConfig';
import { SERVICE_PARTNER_MESSAGES } from '../../../../core/constants/service-partner-messages';

@Component({
  selector: 'app-input-fields',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
  ],
  templateUrl: './input-fields.html',
  styleUrl: './input-fields.css',
})
export class InputFields {
  @Input() config: IInputFieldConfig = {};
  @Input() control!: FormControl;
  @Output() selectionChange = new EventEmitter<string>();
  dateError: string | null = null;

  onSelectChange(value: string): void {
    this.selectionChange.emit(value);
  }

  get type() { return this.config.type ?? 'text'; }
  get label() { return this.config.label ?? ''; }
  get placeholder() { return this.config.placeholder ?? ''; }
  get prefixIcon() { return this.config.prefixIcon ?? ''; }
  get suffixIcon() { return this.config.suffixIcon ?? ''; }
  get suffixText() { return this.config.suffixText ?? ''; }
  get options() { return this.config.options ?? []; }
  get rows() { return this.config.rows = 3; }
  get suffixBsIcon() { return this.config.suffixBsIcon ?? ''; }
  get prefixBsIcon() { return this.config.prefixBsIcon ?? ''; }
  get requiredMessage() { return this.config.requiredMessage ?? ''; }

  getErrorMessage(): string {
    if (!this.control?.errors) return '';
    const label = this.config.label ?? '';
    const requiredMessage = this.config.requiredMessage;
    const msgs = SERVICE_PARTNER_MESSAGES.inputField;

    if (this.control.errors['required']) {
      return requiredMessage ?? `${label} ${msgs.REQUIRED_SUFFIX}`;
    }
    if (this.control.errors['pattern']) return `${label} ${msgs.FORMAT_INVALID_SUFFIX}`;
    if (this.control.errors['minlength']) return `${label} ${msgs.TOO_SHORT_SUFFIX}`;
    if (this.control.errors['maxlength']) return `${label} ${msgs.TOO_LONG_SUFFIX}`;
    if (this.control.errors['matDatepickerParse']) return msgs.DATE_FORMAT_INVALID;
    if (this.control.errors['min'] || this.control.errors['max']) return msgs.MARKS_RANGE;
    if (this.control.errors['pattern'] && label === 'Passing Year') return msgs.PASSING_YEAR_INVALID;
    if (this.control.errors['futureYear']) return msgs.FUTURE_YEAR;

    return msgs.INVALID_VALUE;
  }

  maxDate: Date = new Date();
  minDate: Date = new Date(1900, 0, 1);

  validateDOB(value: string): string | null {
    if (!value) return null;

    const msgs = SERVICE_PARTNER_MESSAGES.dob;
    const parts = value.split('/');

    if (parts.length !== 3) return msgs.FORMAT_INVALID;

    const month = Number(parts[0]);
    const day = Number(parts[1]);
    const year = Number(parts[2]);

    if (!month || !day || !year) return msgs.INVALID_DATE;

    const daysInMonth = new Date(year, month, 0).getDate();

    if (month < 1 || month > 12) return msgs.MONTH_RANGE;
    if (day < 1 || day > daysInMonth) return `${msgs.DAY_RANGE_PREFIX} ${daysInMonth}`;
    if (year < 1000) return msgs.YEAR_INVALID;

    const today = new Date();
    if (year > today.getFullYear()) return msgs.FUTURE_YEAR;

    const enteredDate = new Date(year, month - 1, day);
    if (enteredDate > today) return msgs.FUTURE_DATE;

    return null;
  }

  onDateBlur(event: Event): void {
    const input = event.target as HTMLInputElement;
    const value = input.value;

    const error = this.validateDOB(value);

    if (error) {
      this.dateError = error;
      this.control.setErrors({ invalidDob: true });
    } else {
      this.dateError = null;
      this.control.setErrors(null);
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
}