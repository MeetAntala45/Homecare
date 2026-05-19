import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ChartConfiguration, ChartOptions } from 'chart.js';
import { NgChartsModule } from 'ng2-charts';
import {
  Chart,
  BarController,
  BarElement,
  CategoryScale,
  LinearScale,
  Tooltip,
  Legend,
} from 'chart.js';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { PeriodPickerComponent } from '../period-picker/period-picker';
import { PartnerDashboardMessages } from '../../../../core/constants/partner-dashboard-messages';
import { PartnerDashboardService } from '../../../../core/services/dashboard/dashboard-service';
import { IGetPartnerChartRequest } from '../../../../core/models/dashboard/IGetPartnerChartRequest';

Chart.register(BarController, BarElement, CategoryScale, LinearScale, Tooltip, Legend);

@Component({
  selector: 'app-partner-revenue-chart',
  imports: [CommonModule, NgChartsModule, PeriodPickerComponent],
  templateUrl: './partner-revenue-chart.html',
  styleUrl: './partner-revenue-chart.css',
})
export class PartnerRevenueChart implements OnInit {
  loading = true;
  isEmpty = false;

  selectedTab: 'week' | 'month' | 'year' = 'week';
  selectedWeek: 'this' | 'last' = 'this';

  weekBarHeights = [120, 80, 150, 110, 60, 140, 100];
  monthBarHeights = Array.from({ length: 30 }, (_, i) => 60 + (i % 6) * 18);
  yearBarHeights = [90, 110, 130, 150, 120, 100, 140, 160, 130, 110, 90, 120];

  chartData: ChartConfiguration<'bar'>['data'] = {
    labels: [],
    datasets: [],
  };

  chartOptions: ChartOptions<'bar'> = {
    responsive: true,
    maintainAspectRatio: false,
    animation: { duration: 800, easing: 'easeOutQuart' },
    animations: {
      y: {
        duration: 800,
        easing: 'easeOutQuart',
        from: (ctx) => {
          if (ctx.type === 'data') {
            return ctx.chart.scales['y'].getPixelForValue(0);
          }
          return undefined;
        },
        delay: (ctx) => ctx.dataIndex * 80,
      },
    },
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (ctx) => '$' + this.formatValue(ctx.parsed.y!),
        },
      },
    },
    scales: {
      x: { grid: { display: false } },
      y: {
        beginAtZero: true,
        min: 0,
        grid: { color: '#DDE4EE' },
        border: { dash: [4, 4] },
        ticks: {
          maxTicksLimit: 6,
          callback: (value) => '$' + this.formatValue(value as number),
        },
      },
    },
  };

  constructor(private dashboardService: PartnerDashboardService, private toaster: Toaster) {}

  ngOnInit(): void {
    this.loadChart({ period: 'week', week: 'this' });
  }

  onPeriodChanged(event: IGetPartnerChartRequest): void {
    const newReq: IGetPartnerChartRequest = {
      period: event.period,
      week: event.period === 'week' ? event.week ?? this.selectedWeek : undefined,
    };

    let isSame = false;
    if (newReq.period === 'week') {
      isSame = this.selectedTab === 'week' && this.selectedWeek === newReq.week;
    }
    if (isSame) return;

    this.selectedTab = newReq.period;
    if (newReq.week) this.selectedWeek = newReq.week;
    this.loadChart(newReq);
  }

  loadChart(req: IGetPartnerChartRequest): void {
    this.loading = true;
    const startTime = Date.now();

    this.dashboardService.getRevenueChart(req).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const elapsed = Date.now() - startTime;
          const delay = Math.max(1000 - elapsed, 0);

          setTimeout(() => {
            const values = res.data!.data.map((x) => x.value);
            this.isEmpty = values.every((v) => v === 0);
            this.chartData = {
              labels: res.data!.data.map((x) => x.label),
              datasets: [
                {
                  data: values,
                  backgroundColor: '#6366f1',
                  hoverBackgroundColor: '#4f46e5',
                  borderRadius: 6,
                },
              ],
            };
            this.loading = false;
          }, delay);
        } else {
          
          this.loading = false;
        }
      },
      error: () => {
        this.toaster.error(PartnerDashboardMessages.SERVER.ERROR);
        this.loading = false;
      },
    });
  }

  getPeriodLabel(): string {
    switch (this.selectedTab) {
      case 'week':
        return this.selectedWeek === 'this' ? 'this week' : 'last week';
      case 'month':
        return 'this month';
      case 'year':
        return 'this year';
      default:
        return 'this week';
    }
  }

  formatValue(value: number): string {
    if (value >= 10_000_000) return (value / 10_000_000).toFixed(1).replace(/\.0$/, '') + 'Cr';
    if (value >= 100_000) return (value / 100_000).toFixed(1).replace(/\.0$/, '') + 'L';
    if (value >= 1_000) return (value / 1_000).toFixed(1).replace(/\.0$/, '') + 'K';
    return value.toString();
  }

  getEmptyStateType(): 'future' | 'current' | 'past' {
    if (this.selectedTab === 'week') {
      return this.selectedWeek === 'this' ? 'current' : 'past';
    }
    return 'current';
  }

  getEmptyMessage(): string {
    const type = this.getEmptyStateType();
    const label = this.getPeriodLabel();
    if (type === 'future') return PartnerDashboardMessages.EMPTY.FUTURE(label);
    if (type === 'current') return PartnerDashboardMessages.EMPTY.CURRENT(label);
    return PartnerDashboardMessages.EMPTY.PAST(label);
  }
}
