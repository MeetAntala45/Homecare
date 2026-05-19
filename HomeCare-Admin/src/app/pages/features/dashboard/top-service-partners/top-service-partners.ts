import { Component } from '@angular/core';
import { ITopServicePartnerResponse } from '../../../../core/models/dashboard/ITopServicePartnerResponse';
import { DashboardService } from '../../../../core/services/dashboard/dashboard-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { Router } from '@angular/router';
import { DashboardMessages } from '../../../../core/constants/dashboard-messages';
import { API_BASE_URL } from '../../../../core/constants/environment-config';

@Component({
  selector: 'app-top-service-partners',
  imports: [],
  templateUrl: './top-service-partners.html',
  styleUrl: './top-service-partners.css',
})
export class TopServicePartners {
  partners: ITopServicePartnerResponse[] = [];
  partnersLoading = true;
  baseUrl = API_BASE_URL;

  constructor(private dashboardService: DashboardService,
    private toaster: Toaster,
    private router: Router
  ) { }

  readonly avatarColors = [
    '#4e79a7', '#f28e2c', '#59c36a', '#b07cc6',
    '#e15759', '#76b7b2', '#ff9da7', '#9c755f'
  ];

  ngOnInit(): void {
    this.loadTopServicePartners();
  }

  loadTopServicePartners(): void {
    this.partnersLoading = true;
    const startTime = Date.now();

    this.dashboardService.getTopServicePartners().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          const elapsed = Date.now() - startTime;
          const delay = Math.max(1000 - elapsed, 0);
          setTimeout(() => {
            this.partners = res.data!;
            this.partners.forEach(p => {
              if(!p.profileImage) {
                (p as any).safeImage = '';
              }
              
              const url = p.profileImage?.startsWith('http')
              ? p.profileImage
              : `${this.baseUrl}${p.profileImage}`
              const img = new Image();

              img.onload = () => {
                (p as any).safeImage = url;
              };
            
              img.onerror = () => {
                (p as any).safeImage = '';
              };
            
              img.src = url;
            });
            this.partnersLoading = false;
          }, delay);
        } else {
          this.toaster.error(DashboardMessages.FAIL.SERVICE_PARTNER);
          this.partnersLoading = false;
        }
      },
      error: () => {
        this.toaster.error(DashboardMessages.SERVER.ERROR);
        this.partnersLoading = false;
      }
    });
  }

  getInitials(name: string): string {
    const parts = name.trim().split(' ');
    if (parts.length === 1) return parts[0][0].toUpperCase();
    return (parts[0][0] + parts[1][0]).toUpperCase();
  }

  getAvatarColor(name: string): string {
    let hash = 0;
    for (let i = 0; i < name.length; i++) {
      hash = name.charCodeAt(i) + ((hash << 5) - hash);
    }
    return this.avatarColors[Math.abs(hash) % this.avatarColors.length];
  }

  getBadgeRank(partner: ITopServicePartnerResponse): number | null {

    const uniqueValues = [...new Set(this.partners.map(p => p.jobsCompleted))]
      .sort((a, b) => b - a);
    const rank = uniqueValues.indexOf(partner.jobsCompleted);
    return rank <= 2 ? rank : null;
  }

  viewAll(): void {

    this.router.navigate(['/admin/service-partners']);
  }

}

