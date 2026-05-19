import {
  AfterViewInit,
  Component,
  ElementRef,
  QueryList,
  ViewChildren,
  OnDestroy,
  ChangeDetectorRef,
  OnInit,
} from '@angular/core';
import { MatIcon } from '@angular/material/icon';
import { SearchBox } from "../../../shared/components/search-box/search-box";
import { Router } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { IService } from '../../../core/models/service/IService';
import KeenSlider, { KeenSliderInstance } from 'keen-slider';
import { IServiceType } from '../../../core/models/serviceType/IServiceType';
import { ServiceType } from '../../../core/services/home/serviceType/service-type';
import { IApiResponse } from '../../../core/models/apiResponse/IApiResponse';
import { Service } from '../../../core/services/home/service/service';
import { CommonModule } from '@angular/common';
import { CountFormatPipe } from "../../../shared/pipes/count-formatter/count-format-pipe";
import { ICustomerList } from '../../../core/models/homepage/ICustomerList';
import { CustomerCount } from '../../../core/services/home/customer-count/customer-count';
import { BookingCount } from '../../../core/services/home/booking-count/booking-count';
import { SlotService } from '../../../core/services/checkout/slot-service';
import { Toaster } from '../../../core/services/toaster/toaster';
import { MatTooltipModule } from '@angular/material/tooltip';
import { CouponBanner } from "./coupon-banner/coupon-banner";
import { API_BASE_URL } from '../../../core/constants/environment-config';

type SliderKey = 'serviceTypes' | 'popular' | 'allServices';

interface SliderState {
  instance?: KeenSliderInstance;
  isPrevDisabled: boolean;
  isNextDisabled: boolean;
}

@Component({
  selector: 'app-homepage',
  imports: [MatIcon, SearchBox, CommonModule, CountFormatPipe, MatTooltipModule, CouponBanner],
  templateUrl: './homepage.html',
  styleUrl: './homepage.css',
})
export class Homepage implements OnDestroy, OnInit {
  @ViewChildren('sliderRef') sliderRefs!: QueryList<ElementRef<HTMLElement>>;

  loading: boolean = false;

  BASE_URL = API_BASE_URL;

  serviceTypes: IServiceType[] = [];
  popularServices: IService[] = [];
  allServices: IService[] = [];

  constructor(private router: Router,
    private cdr: ChangeDetectorRef,
    private serviceType: ServiceType,
    private service: Service,
    private customer: CustomerCount,
    private bookingCount: BookingCount,
    private slotService: SlotService,
    private toaster: Toaster,
  ) { }
  sliderState: Record<SliderKey, SliderState> = {
    serviceTypes: { isPrevDisabled: true, isNextDisabled: false },
    popular: { isPrevDisabled: true, isNextDisabled: false },
    allServices: { isPrevDisabled: true, isNextDisabled: false },
  };
  selectedService?: IService;
  customerList?: ICustomerList;
  totalCount: number = 0;

  ngOnInit(): void {
    this.loadServiceTypes();
    this.loadServices();
    this.loadCustomerCount();
  }

  loadServiceTypes() {
    this.loading = true;
    this.serviceType.getAll().subscribe({
      next: (res: IApiResponse<IServiceType[]>) => {
        this.serviceTypes = res.data;
        this.loading = false;
        this.cdr.detectChanges();

        const sliderMap: SliderKey[] = ['serviceTypes', 'popular', 'allServices'];

        this.sliderRefs.forEach((sliderRef, index) => {
          const key = sliderMap[index];
          if (key) {
            this.initSlider(sliderRef.nativeElement, key, this.getPerView(key));
          }
        });
      },
      error: (err) => {
        this.toaster.error(err?.error?.message);
        this.loading = false;
      }
    });
  }
  loadServices() {
    this.loading = true;

    this.service.getAllServices().subscribe({
      next: (serviceRes: IApiResponse<IService[]>) => {
        const services: IService[] = (serviceRes.data || []).map(s => ({
          id: s.id,
          name: s.name,
          price: s.price,
          bookingCount: 0,
          isAvailable: s.isAvailable,
          hasPartner: false,
          imagePaths: [
            s.imagePaths?.length
              ? s.imagePaths[0]
              : '/assets/images/default_image.png'
          ]
        }));

        forkJoin({
          counts: this.bookingCount.getAllServiceBookingCounts(),
          partners: this.slotService.getServicePartnerAvailability()
        }).subscribe({
          next: ({ counts, partners }) => {
            services.forEach(s => {
              s.bookingCount = counts.data[s.id!] || 0;
              s.hasPartner = partners.data[s.id!] ?? false;
            });

            this.allServices = [...services].sort((a, b) =>
              (a.name || '').toLowerCase().localeCompare((b.name || '').toLowerCase())
            );

            this.popularServices = [...services]
              .filter(service => this.canBook(service))
              .sort((a, b) => (b.bookingCount || 0) - (a.bookingCount || 0))
              .slice(0, 10);

            this.loading = false;
            this.cdr.detectChanges();

            const sliderMap: SliderKey[] = ['serviceTypes', 'popular', 'allServices'];
            this.sliderRefs.forEach((sliderRef, index) => {
              const key = sliderMap[index];
              if (key) {
                this.initSlider(sliderRef.nativeElement, key, this.getPerView(key));
              }
            });
          },
          error: (err) => {
            this.toaster.error(err?.error?.message);
            this.loading = false;
          }
        });
      },
      error: (err) => {
        this.toaster.error(err?.error?.message);
        this.loading = false;
      }
    });
  }

  loadCustomerCount() {
    this.loading = true;

    const params: any = {
    };

    this.customer.getCustomerList(params).subscribe({
      next: (res) => {
        this.totalCount = res.data?.totalCount ?? 1000;
      }
    });
  }

  private initSlider(
    element: HTMLElement,
    key: SliderKey,
    desktopPerView: number
  ): void {
    this.sliderState[key].instance = new KeenSlider(element, {
      loop: false,
      mode: 'snap',
      rubberband: true,
      slides: {
        perView: 2,
        spacing: 16,
      },
      breakpoints: {
        '(min-width: 576px)': {
          slides: {
            perView: 3,
            spacing: 18,
          },
        },
        '(min-width: 990px)': {
          slides: {
            perView: 4,
            spacing: 20,
          },
        },
        '(min-width: 1200px)': {
          slides: {
            perView: 5,
            spacing: 24,
          },
        },
      },
      created: (slider) => this.updateSliderButtons(slider, key),
      slideChanged: (slider) => this.updateSliderButtons(slider, key),
      updated: (slider) => this.updateSliderButtons(slider, key),
    });
  }

  private getPerView(key: SliderKey): number {
    switch (key) {
      case 'serviceTypes':
        return 6;
      case 'popular':
      case 'allServices':
        return 5;
      default:
        return 5;
    }
  }

  private updateSliderButtons(slider: KeenSliderInstance, key: SliderKey): void {
    const details = slider.track.details;
    if (!details) return;

    this.sliderState[key].isPrevDisabled = details.rel === 0;
    this.sliderState[key].isNextDisabled = details.rel >= details.maxIdx;
  }

  prevSlide(key: SliderKey): void {
    this.sliderState[key].instance?.prev();
  }

  nextSlide(key: SliderKey): void {
    this.sliderState[key].instance?.next();
  }

  ngOnDestroy(): void {
    Object.values(this.sliderState).forEach((slider) => {
      slider.instance?.destroy();
    });
  }
  onServiceSearch(service: IService) {
    this.router.navigate(['/customer/service-details', service.id]);
  }

  onServiceTypeClick(serviceType: IServiceType) {
    this.router.navigate(['/customer/service-list'], {
      queryParams: { id: serviceType.id, name: serviceType.name }
    });
  }

  goToServiceDetail(id: number): void {
    if (!id) return;
    this.router.navigate(['/customer/service-details', id]);
  }

  goToCheckOutPage(id: number): void {
    this.router.navigate(['/customer/checkout', id]);
  }

  OnJoinNow() {
    this.router.navigate(['/customer/service-partner/onboarding']).then(() => {
      window.scrollTo(0, 0);
    });
  }

  onServiceTypePage() {
    this.router.navigate(['/customer/services']);
  }

  canBook(service: IService): boolean {
    return !!service.isAvailable && !!service.hasPartner;
  }
  getUnavailableReason(service: IService): string {
    if (!service.isAvailable) return 'Service not available';
    if (!service.hasPartner) return 'No partner available';
    return '';
  }
}
