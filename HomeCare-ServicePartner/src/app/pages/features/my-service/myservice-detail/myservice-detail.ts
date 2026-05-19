import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { IService } from '../../../../core/models/service-management/service';
import { PartnerMyService } from '../../../../core/services/my-service/my-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { API_BASE_URL } from '../../../../core/constants/environment-config';

@Component({
  selector: 'app-service-detail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './myservice-detail.html',
  styleUrl: './myservice-detail.css',
})
export class ServiceDetailComponent implements OnInit {
  service: IService | null = null;
  isLoading = false;
  BASE_URL = API_BASE_URL;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private serviceApi: PartnerMyService,
    private toaster: Toaster
  ) { }

  ngOnInit(): void {
    const id = Number(this.route.snapshot.paramMap.get('id'));
    if (id) {
      this.loadService(id);
    }
  }

  loadService(id: number): void {
    this.isLoading = true;

    setTimeout(() => {
      this.serviceApi.getServiceById(id).subscribe({
        next: (res) => {
          this.service = res.data ?? null;
        },
        error: (err) => {
          this.toaster.error(err?.error?.message || 'Failed to load service');
        },
        complete: () => {
          this.isLoading = false;
        }
      });
    }, 600);
  }

  goBack(): void {
    this.router.navigate(['/service-partner/my-services']);
  }

  getImageUrl(path: string): string {
    return `${this.BASE_URL}${path}`;
  }

  onImageError(event: Event): void {
    (event.target as HTMLImageElement).src = 'assets/images/no-image.png';
  }

}