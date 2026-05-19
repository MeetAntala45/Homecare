import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ClickOutside } from '../../../../shared/directives/click-outside';
import { IGetChartRequest } from '../../../../core/models/dashboard/IGetChartRequest';


@Component({
  selector: 'app-period-picker',
  standalone: true,
  imports: [CommonModule, ClickOutside],
  templateUrl: './period-picker.html',
  styleUrl: './period-picker.css'
})
export class PeriodPickerComponent implements OnInit {

  @Input() selectedTab: 'week' | 'month' | 'year' = 'week';
  @Input() selectedWeek: 'this' | 'last' = 'this';
  @Input() selectedMonth: number = new Date().getMonth() + 1;
  @Input() selectedYear: number = new Date().getFullYear();

  @Output() periodChanged = new EventEmitter<IGetChartRequest>();

  currentMonth = new Date().getMonth() + 1;
  currentYear = new Date().getFullYear();

  dropdownOpen = false;
  isMobileDropdown = false;
  pickerView: 'week' | 'month' | 'year' | null = null;

  ngOnInit(): void { }

  toggleMobileDropdown(): void {
    this.isMobileDropdown = true;
    this.dropdownOpen = !this.dropdownOpen;
    this.pickerView = null;
  }

  openPicker(tab: 'week' | 'month' | 'year'): void {

    if (this.pickerView === tab) {
      this.close();
      return;
    }

    this.isMobileDropdown = false;
    this.dropdownOpen = true;
    this.pickerView = tab;
  }

  openMobilePicker(tab: 'week' | 'month' | 'year'): void {
    this.isMobileDropdown = true;
    this.dropdownOpen = true;
    this.pickerView = tab;
  }

  goBack(): void {
    this.pickerView = null;
    this.dropdownOpen = true;
    this.isMobileDropdown = true;
  }

  selectWeek(w: 'this' | 'last'): void {
    this.selectedWeek = w;
    this.selectedTab = 'week';
    this.close();
    this.periodChanged.emit({ period: 'week', week: w });
  }

  selectMonth() {
    this.selectedTab = 'month';
    this.close();
    this.periodChanged.emit({ period: 'month' });
  }

  selectYear() {
    this.selectedTab = 'year';
    this.close();
    this.periodChanged.emit({ period: 'year' });
  }

  close(): void {
    this.dropdownOpen = false;
    this.pickerView = null;
    this.isMobileDropdown = false;
  }

  getPeriodLabel(): string {
    switch (this.selectedTab) {
      case 'week': return this.selectedWeek === 'this' ? 'This Week' : 'Last Week';
      case 'month': return "This Month";
      case 'year': return "This Year";
      default: return 'This Week';
    }
  }

}
