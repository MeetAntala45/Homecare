import { Component, OnInit } from '@angular/core';
import * as bootstrap from 'bootstrap';
import { Chart, ChartConfiguration, ChartOptions, registerables } from 'chart.js';
import { NgChartsModule } from 'ng2-charts';
import { CommonModule } from '@angular/common';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { PeriodPickerComponent } from '../period-picker/period-picker';
import { PartnerDashboardMessages } from '../../../../core/constants/partner-dashboard-messages';
import { PartnerDashboardService } from '../../../../core/services/dashboard/dashboard-service';
import { IPartnerTopService } from '../../../../core/models/dashboard/IPartnerTopService';
import { IGetPartnerChartRequest } from '../../../../core/models/dashboard/IGetPartnerChartRequest';

Chart.register(...registerables);

@Component({
  selector: 'app-partner-top-service-chart',
  imports: [NgChartsModule, CommonModule, PeriodPickerComponent],
  templateUrl: './partner-top-service-chart.html',
  styleUrl: './partner-top-service-chart.css',
})
export class PartnerTopServiceChart implements OnInit {

  constructor(
    private readonly toaster: Toaster,
    private readonly dashboardService: PartnerDashboardService
  ) {}

  isLoading = true;
  isEmpty = false;

  selectedTab: 'week' | 'month' | 'year' = 'week';
  selectedWeek: 'this' | 'last' = 'this';

  serviceList: IPartnerTopService[] = [];
  totalBookings = 0;

  pieChartData: ChartConfiguration<'doughnut'>['data'] = {
    labels: [],
    datasets: [{ data: [], backgroundColor: [], borderWidth: 1 }]
  };

  pieChartOptions: ChartOptions<'doughnut'> = {
    responsive: true,
    cutout: '70%',
    plugins: {
      legend: { display: false }
    }
  };

  ngOnInit(): void {
    this.loadServiceData({ period: 'week', week: this.selectedWeek });
  }

  onPeriodChanged(event: IGetPartnerChartRequest): void {
    const newReq: IGetPartnerChartRequest = {
      period: event.period,
      week: event.period === 'week'
        ? (event.week ?? this.selectedWeek)
        : undefined,
    };

    let isSame = false;
    if (newReq.period === 'week') {
      isSame = this.selectedTab === 'week' && this.selectedWeek === newReq.week;
    }
    if (isSame) return;

    this.selectedTab = newReq.period;
    if (newReq.week) this.selectedWeek = newReq.week;
    this.loadServiceData(newReq);
  }

  private loadServiceData(request: IGetPartnerChartRequest): void {
    this.isLoading = true;
    this.isEmpty = false;
    const startTime = Date.now();

    this.dashboardService.getTopServices(request).subscribe({
      next: (res) => {
        if (res.success && res.data?.length) {
          const elapsed = Date.now() - startTime;
          const delay = Math.max(1000 - elapsed, 0);

          setTimeout(() => {
            this.serviceList = res.data!;
            this.isEmpty = this.serviceList.every(s => s.bookingCount === 0);
            this.totalBookings = this.serviceList.reduce((sum, s) => sum + s.bookingCount, 0);
            this.pieChartData = this.buildChartData();
            this.isLoading = false;
            setTimeout(() => this.initTooltips());
          }, delay);
        } else {
          
          this.isLoading = false;
          this.isEmpty = true;
        }
      },
      error: () => {
        this.toaster.error(PartnerDashboardMessages.SERVER.ERROR);
        this.isLoading = false;
      }
    });
  }

  private buildChartData(): ChartConfiguration<'doughnut'>['data'] {
    return {
      labels: this.serviceList.map(s => s.name),
      datasets: [{
        data: this.serviceList.map(s => s.bookingCount),
        backgroundColor: this.serviceList.map((_, i) => this.getColor(i)),
        borderWidth: 1
      }]
    };
  }

  getColor(index: number): string {
    const hue = (index * 137.508) % 360;
    return `hsl(${hue}, 65%, 55%)`;
  }

  private initTooltips(): void {
    document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el => {
      new bootstrap.Tooltip(el);
    });
  }

  getPeriodLabel(): string {
    switch (this.selectedTab) {
      case 'week':  return this.selectedWeek === 'this' ? 'this week' : 'last week';
      case 'month': return 'This Month';
      case 'year':  return 'This Year';
      default:      return 'this week';
    }
  }

  getEmptyStateType(): 'future' | 'current' | 'past' {
    if (this.selectedTab === 'week') {
      return this.selectedWeek === 'this' ? 'current' : 'past';
    }
    return 'current';
  }

  getEmptyMessage(): string {
    const type  = this.getEmptyStateType();
    const label = this.getPeriodLabel();
    if (type === 'future')  return PartnerDashboardMessages.EMPTY.FUTURE(label);
    if (type === 'current') return PartnerDashboardMessages.EMPTY.CURRENT(label);
    return PartnerDashboardMessages.EMPTY.PAST(label);
  }
}