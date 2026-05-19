import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatTooltip } from '@angular/material/tooltip';
import { CountFormatPipe } from '../../../../shared/pipes/count-formatter/count-format-pipe';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { PartnerDashboardService } from '../../../../core/services/dashboard/dashboard-service';
import { PartnerDashboardMessages } from '../../../../core/constants/partner-dashboard-messages';

@Component({
  selector: 'app-partner-summary-cards',
  imports: [CommonModule, CountFormatPipe, MatTooltip],
  templateUrl: './partner-summary-cards.html',
  styleUrl: './partner-summary-cards.css',
})
export class PartnerSummaryCards implements OnInit {
  loading = true;

  summaryCards = [
    {
      label: 'Total Bookings',
      value: 0,
      target: 0,
      change: 0,
      isIncrease: true,
      icon: 'assets/totalBookings.png',
      isRevenue: false,
      isRating: false,
    },
    {
      label: 'Unique Customers',
      value: 0,
      target: 0,
      change: 0,
      isIncrease: true,
      icon: 'assets/activeUsers.png',
      isRevenue: false,
      isRating: false,
    },
    {
      label: 'Average Rating',
      value: 0,
      target: 0,
      change: 0,
      isIncrease: true,
      icon: 'assets/activePartners.png',
      isRevenue: false,
      isRating: true,
    },
    {
      label: 'Total Revenue',
      value: 0,
      target: 0,
      change: 0,
      isIncrease: true,
      icon: 'assets/totalRevenue.png',
      isRevenue: true,
      isRating: false,
    },
  ];

  constructor(private dashboardService: PartnerDashboardService, private toaster: Toaster) {}

  ngOnInit(): void {
    this.loadMetrics();
  }

  loadMetrics(): void {
    const startTime = Date.now();

    this.dashboardService.getAllMetricCards().subscribe({
      next: (res) => {
        if (res.success && res.data != null) {
          const data = res.data;

          const result = [
            data.totalBookings,
            data.uniqueCustomers,
            data.averageRating,
            data.totalRevenue,
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
        this.toaster.error(PartnerDashboardMessages.FAIL.METRICS);
      },
    });
  }

  animateValue(card: any): void {
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
      } else {
        card.value = start;
      }
    }, stepTime);
  }
}
