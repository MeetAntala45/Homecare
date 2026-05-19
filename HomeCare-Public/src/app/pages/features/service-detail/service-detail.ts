import { CommonModule } from '@angular/common';
import { Component, OnInit } from '@angular/core';
import { MatIcon } from '@angular/material/icon';
import { ActivatedRoute } from '@angular/router';
import { ServiceDetailService } from '../../../core/services/service-details/service-detail';
import { IService } from '../../../core/models/service/IService';
import { IApiResponse } from '../../../core/models/apiResponse/IApiResponse';
import { NgxSkeletonLoaderModule } from 'ngx-skeleton-loader';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { SlotService } from '../../../core/services/checkout/slot-service';
import { ReviewService } from '../../../core/services/my-bookings/review-service';
import { IServiceReviewSummary } from '../../../core/models/reviewListing/review-listing';
import { API_BASE_URL } from '../../../core/constants/environment-config';

@Component({
  selector: 'app-service-detail',
  standalone: true,
  imports: [CommonModule, MatIcon, NgxSkeletonLoaderModule],
  templateUrl: './service-detail.html',
  styleUrl: './service-detail.css',
})

export class ServiceDetail implements OnInit {
  BASE_URL = API_BASE_URL;
  service!: IService;

  otherServices: IService[] = [];
  images: string[] = [];
  includes: string[] = [];
  excludes: string[] = [];

  isLoading = true;
  reviewSummary: IServiceReviewSummary | null = null;

  constructor(
    private router: Router,
    private route: ActivatedRoute,
    private serviceDetailService: ServiceDetailService,
    private slotService: SlotService,
    private reviewService : ReviewService
  ) { }

  ngOnInit(): void {
    this.route.paramMap.subscribe(params => {
      const id = Number(params.get('id'));
      if (id) {
        this.loadService(id);
        this.reviewService.getServiceReviews(id).subscribe({
          next: (res) => {
            if (res.success && res.data && res.data.totalReviews > 0) {
              this.reviewSummary = res.data;
            }
          },
          error: () => {}
        });
        
      }
    });
  }

  loadService(id: number) {
    this.isLoading = true;

    forkJoin({
      service: this.serviceDetailService.getServiceById(id),
      partners: this.slotService.getServicePartnerAvailability()
    }).subscribe({
      next: ({ service, partners }) => {
        if (service.success) {
          this.service = {
            ...service.data,
            hasPartner: partners.data[service.data.id!] ?? false
          };

          this.images = (this.service.imagePaths || []).map(img => this.getImageUrl(img));
          this.includes = this.service.inclusions || [];
          this.excludes = this.service.exclusions || [];

          const subCategoryId = this.service.subCategoryId;

          if (subCategoryId) {
            this.serviceDetailService.getServicesBySubCategory(subCategoryId).subscribe({
              next: (subRes: IApiResponse<IService[]>) => {
                if (subRes.success) {
                  this.otherServices = (subRes.data || [])
                    .filter(x => x.id !== this.service.id)
                    .map(s => ({
                      ...s,
                      hasPartner: partners.data[s.id!] ?? false
                    }));
                }
              },
              error: () => {
                this.otherServices = [];
              }
            });
          }
        }

        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  getImageUrl(path?: string): string {
    if (!path) return 'assets/images/default_image.png';
    return `${path}`;
  }

  get imageCount(): number {
    return this.images.length;
  }

  getGridClass(): string {
    if (this.imageCount === 1) return 'col-12';
    if (this.imageCount === 2) return 'col-md-6';
    if (this.imageCount === 4) return 'col-md-6';
    if (this.imageCount === 6) return 'col-md-4';
    return 'col-md-4 col-sm-6';
  }

  showAllServices = false;
  initialVisibleCount = 4;

  get visibleOtherServices() {
    return this.showAllServices
      ? this.otherServices
      : this.otherServices.slice(0, this.initialVisibleCount);
  }

  toggleViewAll() {
    this.showAllServices = !this.showAllServices;
  }

  goToServiceDetail(id: number): void {
    if (!id) return;
    this.router.navigate(['/customer/service-details', id]);
  }

  goToCheckOutPage(id: number): void {
    this.router.navigate(['/customer/checkout', id]);
  }

  canBook(service: IService): boolean {
    return !!service.isAvailable && !!service.hasPartner;
  }
  getUnavailableReason(service: IService): string {
    if (!service.isAvailable) return 'Service not available';
    if (!service.hasPartner) return 'No partner available';
    return '';
  }

  getRatingColor(avg: number): string {
    if (avg >= 4) return 'rating-green';
    if (avg >= 3) return 'rating-yellow';
    return 'rating-red';
  }
  
  getRatingLabel(avg: number): string {
    if (avg >= 4.5) return 'Excellent';
    if (avg >= 4)   return 'Very Good';
    if (avg >= 3)   return 'Good';
    if (avg >= 2)   return 'Fair';
    return 'Poor';
  }
  
  getStarArray(rating: number): { filled: boolean }[] {
    return Array.from({ length: 5 }, (_, i) => ({ filled: i < rating }));
  }
  
  getInitial(name: string): string {
    return name?.charAt(0).toUpperCase() || '?';
  }
  
  getTimeAgo(dateStr: string): string {
    const diff = Date.now() - new Date(dateStr).getTime();
    const days = Math.floor(diff / 86400000);
    if (days === 0) return 'Today';
    if (days === 1) return 'Yesterday';
    if (days < 30)  return `${days} days ago`;
    if (days < 365) return `${Math.floor(days / 30)} months ago`;
    return `${Math.floor(days / 365)} years ago`;
  }
  
}