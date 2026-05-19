import { Component } from '@angular/core';
import { DashboardService } from '../../../../core/services/dashboard/dashboard-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { MatTooltip } from '@angular/material/tooltip';
import { CountFormatPipe } from '../../../../shared/pipes/count-formatter/count-format-pipe';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-summary-cards',
  imports: [CommonModule, CountFormatPipe, MatTooltip],
  templateUrl: './summary-cards.html',
  styleUrl: './summary-cards.css',
})
export class SummaryCards {
  selectedRange: string = 'week';
  loading = true;

  summaryCards = [
    {
      label: 'Total Bookings',
      value: 0,
      target: 0,
      change: 0,
      isIncrease: true,
      icon: 'assets/totalBookings.png'
    },
    {
      label: 'Active Customers',
      value: 0,
      target: 0,
      change: 0,
      isIncrease: true,
      icon: 'assets/activeUsers.png'
    },
    {
      label: 'Active Partners',
      value: 0,
      target: 0,
      change: 0,
      isIncrease: true,
      icon: 'assets/activePartners.png'
    },
    {
      label: 'Total Revenue',
      value: 0,
      target: 0,
      change: 0,
      isIncrease: true,
      icon: 'assets/totalRevenue.png',
      isRevenue: true
    }
  ];

  constructor(
    private dashboardService: DashboardService,
    private toaster: Toaster,
  ) { }

  ngOnInit(): void {
    this.loadMetrics();
  }

  loadMetrics() {

    const startTime = Date.now();
    this.dashboardService.getAllMetricCards()
      .subscribe({
        next: (res) => {

          if (res.success && res.data != null) {
            const data = res.data;

            const result = [
              data.totalBookings,
              data.activeCustomers,
              data.activePartners,
              data.totalRevenue
            ];

            const elapsed = Date.now() - startTime;
            const delay = Math.max(1000 - elapsed, 0);

            setTimeout(() => {
              this.loading = false;
              this.summaryCards.forEach((card, i) => {
                card.target = result[i].value;
                card.change = result[i].change;
                card.isIncrease = result[i].isIncrease;
                this.animateValue(card);
              });
            }, delay);
          }
        },
        error: (err) => {
          this.toaster.error(err.message);
        }
      });
  }
  animateValue(card: any) {
    let start = 0;
    const end = card.target;
    const duration = 1200;
    const stepTime = 16;

    const step = Math.ceil(end / (duration / stepTime));

    const timer = setInterval(() => {

      start += step;

      if (start >= end) {
        card.value = end;
        clearInterval(timer);
      }
      else {
        card.value = start;
      }

    }, stepTime);
  }
}

