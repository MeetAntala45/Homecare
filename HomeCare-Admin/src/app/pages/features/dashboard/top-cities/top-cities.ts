import { Component, OnInit } from '@angular/core';
import { ChartConfiguration, ChartOptions } from 'chart.js';
import { BaseChartDirective } from 'ng2-charts';
import { Chart, LineController, LineElement, PointElement, CategoryScale, LinearScale, Tooltip, Legend, Filler } from 'chart.js';
import { CommonModule } from '@angular/common';
import { DashboardService } from '../../../../core/services/dashboard/dashboard-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { PeriodPickerComponent } from '../period-picker/period-picker';
import { IGetChartRequest } from '../../../../core/models/dashboard/IGetChartRequest';
import { DashboardMessages } from '../../../../core/constants/dashboard-messages';

Chart.register(LineController, LineElement, PointElement, CategoryScale, LinearScale, Tooltip, Legend, Filler);

const LINE_COLORS = ['#f28e2c', '#8e6ad6', '#59c36a', '#e15759'];

@Component({
  selector: 'app-top-cities',
  imports: [BaseChartDirective, CommonModule, PeriodPickerComponent],
  templateUrl: './top-cities.html',
  styleUrl: './top-cities.css',
})
export class TopCities implements OnInit {

  citiesLoading = true;
  isEmpty = false;

  selectedTab: 'week' | 'month' | 'year' = 'week';
  selectedWeek: 'this' | 'last' = 'this';

  weekSkeletonHeights = [60, 90, 75, 120, 50, 140, 100];
  monthSkeletonHeights = Array.from({ length: 30 }, (_, i) => 40 + (i % 5) * 20);
  yearSkeletonHeights = [80, 100, 120, 90, 110, 130, 70, 95, 115, 85, 105, 125];

  line1Points = [
    { x: 0, y: 180 }, { x: 100, y: 150 }, { x: 200, y: 130 },
    { x: 300, y: 120 }, { x: 400, y: 140 }, { x: 500, y: 40 },
    { x: 600, y: 60 }, { x: 700, y: 50 }
  ];

  line2Points = [
    { x: 0, y: 80 }, { x: 100, y: 50 }, { x: 200, y: 60 },
    { x: 300, y: 30 }, { x: 400, y: 160 }, { x: 500, y: 130 },
    { x: 600, y: 155 }, { x: 700, y: 140 }
  ];

  lineChartData: ChartConfiguration<'line'>['data'] = {
    labels: [],
    datasets: []
  };

  lineChartOptions: ChartOptions<'line'> = {
    responsive: true,
    maintainAspectRatio: false,
    animation: { duration: 0 },
    animations: {
      y: {
        easing: 'easeOutQuart',
        duration: 800,
        delay: (ctx) => ctx.dataIndex * 100
      }
    },
    plugins: {
      legend: { display: false },
      tooltip: {
        callbacks: {
          label: (ctx) => `${ctx.dataset.label}: ${this.formatValue(ctx.parsed.y!)}`
        }
      }
    },
    scales: {
      x: { grid: { display: false } },
      y: {
        beginAtZero: true,
        grid: { color: '#DDE4EE' },
        border: { dash: [4, 4] },
        ticks: {
          maxTicksLimit: 6,
          callback: (value) => this.formatValue(value as number)
        }
      }
    }
  };

  constructor(
    private dashboardService: DashboardService,
    private toaster: Toaster
  ) { }

  initial: IGetChartRequest = {
    period: 'week',
    week: 'this'
  }


  ngOnInit(): void {
    this.loadCitiesChart(this.initial);
  }

  onPeriodChanged(event: IGetChartRequest): void {

    const newReq: IGetChartRequest = {
      period: event.period,
      week: event.period === 'week'
        ? (event.week ?? this.selectedWeek)
        : undefined,
    };

    let isSame = false;

    if (newReq.period === 'week') {
      isSame =
        this.selectedTab === 'week' &&
        this.selectedWeek === newReq.week;
    }

    if (isSame) return;

    this.selectedTab = newReq.period;
    if (newReq.week) this.selectedWeek = newReq.week;
    this.loadCitiesChart(newReq);
  }

  loadCitiesChart(req: IGetChartRequest): void {
    this.citiesLoading = true;
    const startTime = Date.now();

    this.dashboardService.getTopCities(req).subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const elapsed = Date.now() - startTime;
          const delay = Math.max(1000 - elapsed, 0);

          setTimeout(() => {
            const isAllZero = res.data?.every(city =>
              city.data?.every(value => value === 0)
            ) ?? false;

            this.isEmpty = !res.data?.length || isAllZero;

            this.lineChartData = {
              labels: this.getLabels(req.period, res.data![0]?.data?.length ?? 0),
              datasets: res.data!.map((city, index) => ({
                label: city.city,
                data: city.data,
                borderColor: LINE_COLORS[index % LINE_COLORS.length],
                backgroundColor: 'transparent',
                pointBackgroundColor: LINE_COLORS[index % LINE_COLORS.length],
                pointRadius: 4,
                pointHoverRadius: 6,
                tension: 0.4,
                borderWidth: 2
              }))
            };
            this.citiesLoading = false;
          }, delay);
        } else {
          this.toaster.error(DashboardMessages.FAIL.CHART);
          this.citiesLoading = false;
        }
      },
      error: (err) => {
        this.toaster.error(DashboardMessages.SERVER.ERROR);
        this.citiesLoading = false;
      }
    });
  }

  getLabels(period: string, length: number): string[] {
    switch (period) {
      case 'week': return ['Mon', 'Tue', 'Wed', 'Thu', 'Fri', 'Sat', 'Sun'];
      case 'month': return Array.from({ length }, (_, i) => (i + 1).toString());
      case 'year': return ['Jan', 'Feb', 'Mar', 'Apr', 'May', 'Jun', 'Jul', 'Aug', 'Sep', 'Oct', 'Nov', 'Dec'];
      default: return [];
    }
  }

  formatValue(value: number): string {
    if (value >= 10_000_000) return (value / 10_000_000).toFixed(1).replace(/\.0$/, '') + 'Cr';
    if (value >= 100_000) return (value / 100_000).toFixed(1).replace(/\.0$/, '') + 'L';
    if (value >= 1_000) return (value / 1_000).toFixed(1).replace(/\.0$/, '') + 'K';
    return value.toString();
  }

  getPeriodLabel(): string {
    switch (this.selectedTab) {
      case 'week': return this.selectedWeek === 'this' ? 'this week' : 'last week';
      case 'month': return "this month";
      case 'year': return "this year";
      default: return 'this week';
    }
  }

  getEmptyStateType(): 'future' | 'current' | 'past' {
    const today = new Date();

    if (this.selectedTab === 'week') {
      if (this.selectedWeek === 'this') return 'current';
      return 'past';
    }

    return 'current';
  }

  getEmptyMessage(): string {
    const type = this.getEmptyStateType();
    const label = this.getPeriodLabel();

    if (type === 'future')
      return DashboardMessages.EMPTY.FUTURE(label);

    if (type === 'current')
      return DashboardMessages.EMPTY.CURRENT(label);

    return DashboardMessages.EMPTY.PAST(label);
  }
}
