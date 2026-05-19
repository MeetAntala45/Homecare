import { CommonModule } from '@angular/common';
import { Component } from '@angular/core';
import { RevenueChart } from './revenue-chart/revenue-chart';
import { TopServicePartners } from './top-service-partners/top-service-partners';
import { TopServiceChart } from './top-service-chart/top-service-chart';
import { TopCities } from './top-cities/top-cities';
import { SummaryCards } from "./summary-cards/summary-cards";
@Component({
  selector: 'app-dashboard',
  imports: [CommonModule, RevenueChart, TopServiceChart, TopServicePartners, TopCities, SummaryCards],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.css',
})
export class Dashboard {

}
