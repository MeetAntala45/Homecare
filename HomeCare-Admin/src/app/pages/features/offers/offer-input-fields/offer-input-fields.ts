import { Component, Input, Output, EventEmitter, SimpleChanges, OnInit, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatNativeDateModule, provideNativeDateAdapter } from '@angular/material/core';
import { MatTooltipModule } from '@angular/material/tooltip';
import { IOfferDropdownOption, IOfferInputFieldConfig } from '../../../../core/models/offers/offer-input-field-config';
import { IServiceTypeGroup, ISubcategoryOption } from '../../../../core/models/offers/service-type-hierarchy.model';

export const DAY_OPTIONS = [
  { label: 'Monday', value: 'monday' },
  { label: 'Tuesday', value: 'tuesday' },
  { label: 'Wednesday', value: 'wednesday' },
  { label: 'Thursday', value: 'thursday' },
  { label: 'Friday', value: 'friday' },
  { label: 'Saturday', value: 'saturday' },
  { label: 'Sunday', value: 'sunday' },
];

@Component({
  selector: 'app-offer-input-fields',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    MatSelectModule,
    MatIconModule,
    MatDatepickerModule,
    MatNativeDateModule,
    MatTooltipModule,
  ],
  providers: [provideNativeDateAdapter()],
  templateUrl: './offer-input-fields.html',
  styleUrl: './offer-input-fields.css',
})
export class OfferInputFields implements OnInit, OnChanges {
  @Input() config: IOfferInputFieldConfig = {};
  @Input() control!: FormControl;
  @Input() maxLength?: number;
  @Output() selectionChange = new EventEmitter<any>();

  ngOnInit(): void {
    if (this.type === 'subcategory-select' && this.control?.value) {
      this.selectedSubcategoryIds = this.control.value
        .split(',')
        .filter(Boolean)
        .map(Number);
    }
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['config'] || changes['control']) {
      const hierarchy = this.config.serviceTypeHierarchy ?? [];
      if (
        hierarchy.length &&
        this.type === 'subcategory-select' &&
        this.control?.value
      ) {
        this.restoreSubcategorySelections();
      }
    }
  }

  get minDate(): Date | null {
    return this.config.disableMinDate ? null : new Date();
  }

  private restoreSubcategorySelections(): void {
    const hierarchy = this.config.serviceTypeHierarchy ?? [];
    if (!hierarchy.length) return;
    if (!this.control?.value) return;

    const storedIds = this.control.value
      .split(',')
      .filter(Boolean)
      .map(Number);

    if (!storedIds.length) return;

    this.selectedSubcategoryIds = storedIds;
    this.selectedCategoryIds = [];
    this.selectedServiceTypeId = null;

    for (const st of hierarchy) {
      for (const cat of st.categories) {
        const hasMatch = cat.subcategories.some(sc => storedIds.includes(sc.id));
        if (hasMatch) {
          this.selectedServiceTypeId = st.serviceTypeId;
          if (!this.selectedCategoryIds.includes(cat.categoryId)) {
            this.selectedCategoryIds.push(cat.categoryId);
          }
        }
      }
    }
  }

  selectedServiceTypeId: number | null = null;
  selectedCategoryIds: number[] = [];
  selectedSubcategoryIds: number[] = [];

  dayOptions = DAY_OPTIONS;
  get timePeriod(): 'AM' | 'PM' {
    const val = this.control?.value;
    if (!val) return 'AM';
    const parts = val.split('|');
    return (parts[1] as 'AM' | 'PM') ?? 'AM';
  }

  get type() { return this.config.type ?? 'text'; }
  get label() { return this.config.label ?? ''; }
  get placeholder() { return this.config.placeholder ?? ''; }
  get suffixText() { return this.config.suffixText ?? ''; }
  get options(): IOfferDropdownOption[] { return this.config.options ?? []; }
  get min() { return this.config.min ?? null; }
  get max() { return this.config.max ?? null; }
  get hint() { return this.config.hint ?? ''; }

  get serviceTypeHierarchy(): IServiceTypeGroup[] {
    return this.config.serviceTypeHierarchy ?? [];
  }

  get filteredCategories() {
    if (!this.selectedServiceTypeId) return [];
    return this.serviceTypeHierarchy
      .find(st => st.serviceTypeId === this.selectedServiceTypeId)
      ?.categories ?? [];
  }

  get filteredSubcategories(): ISubcategoryOption[] {
    if (!this.filteredCategories.length) return [];
    return this.filteredCategories
      .filter(c => this.selectedCategoryIds.includes(c.categoryId))
      .flatMap(c => c.subcategories);
  }

  onServiceTypeSelect(serviceTypeId: number): void {
    this.selectedServiceTypeId = serviceTypeId;
    this.selectedCategoryIds = [];
    this.selectedSubcategoryIds = [];
    this.syncSubcategoryValue();
  }

  onCategoryToggle(categoryId: number): void {
    const idx = this.selectedCategoryIds.indexOf(categoryId);
    if (idx === -1) {
      this.selectedCategoryIds.push(categoryId);
    } else {
      this.selectedCategoryIds.splice(idx, 1);
      const removedSubs = this.filteredCategories
        .find(c => c.categoryId === categoryId)
        ?.subcategories.map(s => s.id) ?? [];
      this.selectedSubcategoryIds = this.selectedSubcategoryIds
        .filter(id => !removedSubs.includes(id));
    }
    this.syncSubcategoryValue();
  }

  onSubcategoryToggle(id: number): void {
    const idx = this.selectedSubcategoryIds.indexOf(id);
    if (idx === -1) {
      this.selectedSubcategoryIds.push(id);
    } else {
      this.selectedSubcategoryIds.splice(idx, 1);
    }
    this.syncSubcategoryValue();
  }

  onSelectChange(value: any): void {
    this.selectionChange.emit(value);
  }

  onDaysChange(values: string[]): void {
    this.control.setValue(values.join(','));
    this.selectionChange.emit(values);
  }

  get selectedDays(): string[] {
    const val = this.control?.value;
    if (!val) return [];
    return typeof val === 'string' ? val.split(',').filter(Boolean) : val;
  }

  onTimeInputChange(value: string) {
    if (!value) return;

    const [hour24, minute] = value.split(':').map(Number);
    const period = hour24 >= 12 ? 'PM' : 'AM';
    const hour12 = hour24 % 12 || 12;
    this.control.setValue(
      `${String(hour12).padStart(2, '0')}:${String(minute).padStart(2, '0')}|${period}`
    );
    this.selectionChange.emit(this.control.value);
  }

  togglePeriod(period: 'AM' | 'PM') {
    if (this.timePeriod === period) return;

    if (this.control.value) {
      const currentTime = this.control.value.split('|')[0];
      this.control.setValue(`${currentTime}|${period}`);
      this.selectionChange.emit(this.control.value);
    }
  }

  increaseTime(minutes = 30) {
    this.adjustTime(minutes);
  }

  decreaseTime(minutes = 30) {
    this.adjustTime(-minutes);
  }

  private adjustTime(step: number) {
    let hour12 = 9;
    let minute = 0;
    let period = this.timePeriod;

    if (this.control.value) {
      const parts = this.control.value.split('|');
      const [h, m] = parts[0].split(':').map(Number);
      hour12 = h;
      minute = m;
      period = parts[1] as 'AM' | 'PM';
    }

    let hour24 = hour12;
    if (period === 'PM' && hour12 !== 12) hour24 += 12;
    if (period === 'AM' && hour12 === 12) hour24 = 0;

    let totalMinutes = hour24 * 60 + minute + step;

    if (totalMinutes < 0) totalMinutes += 1440;
    if (totalMinutes >= 1440) totalMinutes -= 1440;

    const newHour24 = Math.floor(totalMinutes / 60);
    const newMinute = totalMinutes % 60;

    const newPeriod = newHour24 >= 12 ? 'PM' : 'AM';
    const newHour12 = newHour24 % 12 || 12;

    this.control.setValue(
      `${String(newHour12).padStart(2, '0')}:${String(newMinute).padStart(2, '0')}|${newPeriod}`
    );

    this.selectionChange.emit(this.control.value);
  }

  limitLength(event: any) {
    if (!this.maxLength) return;
    let value = event.target.value;
    if (value.length > this.maxLength) {
      value = value.substring(0, this.maxLength);
      event.target.value = value;
      this.control.setValue(value);
    }
  }

  convert12To24(val: string) {
    const [time, period] = val.split(' ');

    let [hour, minute] = time.split(':').map(Number);

    if (period === 'PM' && hour < 12)
      hour += 12;

    if (period === 'AM' && hour === 12)
      hour = 0;

    return { hour, minute };
  }

  onTimeChange(value: string) {
    this.control.setValue(value);
    this.selectionChange.emit(value);
  }

  formatTime12(time: string): string {
    if (!time) return '';
    const [hour, minute] = time.split(':');
    const h = Number(hour);
    const suffix = h >= 12 ? 'PM' : 'AM';
    const hour12 = h % 12 || 12;
    return `${String(hour12).padStart(2, '0')}:${minute} ${suffix}`;
  }

  isCategorySelected(id: number): boolean {
    return this.selectedCategoryIds.includes(id);
  }

  isSubcategorySelected(id: number): boolean {
    return this.selectedSubcategoryIds.includes(id);
  }

  private syncSubcategoryValue(): void {
    this.control.setValue(this.selectedSubcategoryIds.join(','));
    this.selectionChange.emit(this.selectedSubcategoryIds);
  }

  getErrorMessage(): string {
    if (!this.control?.errors) return '';
    const label = this.config.label ?? 'This field';

    if (this.control.errors['required'])
      return this.config.requiredMessage ?? `${label} is required.`;
    if (this.control.errors['min'])
      return `${label} must be at least ${this.config.min}.`;
    if (this.control.errors['max'])
      return `${label} must be at most ${this.config.max}.`;
    if (this.control.errors['pattern'])
      return `${label} format is invalid.`;

    return 'Invalid value.';
  }
}