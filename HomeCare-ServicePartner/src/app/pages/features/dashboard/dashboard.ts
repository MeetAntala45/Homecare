import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { PartnerTopServiceChart } from './partner-top-service-chart/partner-top-service-chart';
import { PartnerSummaryCards } from './partner-summary-cards/partner-summary-cards';
import { PartnerRevenueChart } from './partner-revenue-chart/partner-revenue-chart';

@Component({
  selector: 'app-partner-dashboard',
  imports: [CommonModule, PartnerRevenueChart, PartnerTopServiceChart, PartnerSummaryCards],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class PartnerDashboard {}