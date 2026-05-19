import { Component, HostListener, QueryList, ViewChild, ViewChildren } from '@angular/core';
import { GradientOverlay } from "../../../../shared/components/gradient-overlay/gradient-overlay";
import { SearchBox } from "../../../../shared/components/search-box/search-box";
import { CommonModule } from '@angular/common';
import { MatIcon } from "@angular/material/icon";
import { ActivatedRoute, Router } from '@angular/router';
import { ICategory } from '../../../../core/models/service-listing/ICategory';
import { ISubCategory } from '../../../../core/models/service-listing/ISubCategory';
import { ServiceListing } from '../../../../core/services/service-listing/service-listing';
import { ServiceDetailService } from '../../../../core/services/service-details/service-detail';
import { IServiceView } from '../../../../core/models/service/IServiceView';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { IService } from '../../../../core/models/service/IService';
import { SERVICE_LISTING_MESSAGE } from '../../../../core/constants/service-listing-messages';
import { CountFormatPipe } from "../../../../shared/pipes/count-formatter/count-format-pipe";
import { ChangeDetectorRef, ElementRef } from '@angular/core';
import { forkJoin } from 'rxjs';
import { SlotService } from '../../../../core/services/checkout/slot-service';
import { API_BASE_URL, DEFAULT_IMG } from '../../../../core/constants/environment-config';

@Component({
  selector: 'app-service-listing-page',
  imports: [GradientOverlay, SearchBox, CommonModule, MatIcon, CountFormatPipe],
  templateUrl: './service-listing-page.html',
  styleUrl: './service-listing-page.css',
})
export class ServiceListingPage {

  constructor(
    private route: ActivatedRoute,
    private serviceListing: ServiceListing,
    private serviceDetail: ServiceDetailService,
    private router: Router,
    private toaster: Toaster,
    private cdr: ChangeDetectorRef,
    private slotService: SlotService
  ) { }

  @ViewChild(SearchBox) searchBoxRef!: SearchBox<IServiceView>;
  @ViewChildren('descRef') descRefs!: QueryList<ElementRef>;

  isOverflow: boolean[] = [];;
  serviceTypeId!: number;
  serviceTypeName!: string;
  categories: ICategory[] = [];
  subCategories: ISubCategory[] = [];
  services: IServiceView[] = [];
  allServicesByType: IServiceView[] = [];
  isSearchActive = false;
  selectedCategoryIndex: number | null = null;
  selectedSubCategoryIndex: number | null = null;
  openedCategoryIndex: number | null = null;
  isLoading = true;
  savedCategoryId: number | null = null;
  savedSubCategoryId: number | null = null;
  isOpen = false;
  expanded: boolean[] = [];
  submittedSearchText = '';
  totalServicesCount: number = 0;

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      this.serviceTypeId = params['id'];
      this.serviceTypeName = params['name'];

      this.savedCategoryId = params['categoryId'] ? +params['categoryId'] : null;
      this.savedSubCategoryId = params['subCategoryId'] ? +params['subCategoryId'] : null;

      this.loadCategories();
      this.loadTotalCount();
    });
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }
  loadTotalCount() {
    this.serviceListing.getSearchService(this.serviceTypeId)
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.allServicesByType = (res.data || []).map((service: IService): IServiceView => ({
              ...service,
              images: (service.imagePaths || []).map((img: string) => this.getImageUrl(img))
            }));
            this.totalServicesCount = res.data?.length ?? 0;
          }
        },
        error: () => { }
      });
  }

  onSearchClick(searchText: string) {
    if (!searchText?.trim()) return;

    this.isLoading = true;

    forkJoin({
      services: this.serviceListing.getSearchService(this.serviceTypeId, searchText),
      partners: this.slotService.getServicePartnerAvailability()
    }).subscribe({
      next: ({ services, partners }) => {
        this.isLoading = false;

        if (!services.success) {
          this.toaster.error(SERVICE_LISTING_MESSAGE.error.LOAD_FAIL_SERVICE);
          return;
        }

        this.services = (services.data || []).map((service: IService): IServiceView => ({
          ...service,
          hasPartner: partners.data?.[service.id!] ?? false,
          images: (service.imagePaths || []).map(img => this.getImageUrl(img))
        }));

        this.isSearchActive = true;
        this.submittedSearchText = searchText;
        this.openedCategoryIndex = null;
        this.selectedCategoryIndex = null;
        this.selectedSubCategoryIndex = null;
        this.subCategories = [];
      },
      error: () => {
        this.isLoading = false;
        this.toaster.error(SERVICE_LISTING_MESSAGE.error.LOAD_FAIL_SERVICE);
      }
    });
  }

  loadCategories() {
    this.isLoading = true;
    this.serviceListing.getCategoryById(this.serviceTypeId)
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.categories = res.data || [];

            if (!this.categories.length) {
              this.selectedCategoryIndex = null;
              this.selectedSubCategoryIndex = null;
              this.openedCategoryIndex = null;
              this.subCategories = [];
              this.services = [];
              return;
            }

            let categoryIndex = 0;
            let categoryIdToLoad: number;

            if (this.savedCategoryId) {
              const index = this.categories.findIndex(c => c.id === this.savedCategoryId);
              if (index !== -1) {
                categoryIndex = index;
              }
            }

            this.selectedCategoryIndex = categoryIndex;
            this.openedCategoryIndex = categoryIndex;

            categoryIdToLoad = this.categories[categoryIndex]?.id || 0;
            if (!categoryIdToLoad) return;

            this.loadSubCategories(categoryIdToLoad);
          }
        },
        error: () => {
          this.isLoading = false;
          this.toaster.error(SERVICE_LISTING_MESSAGE.error.LOAD_FAIL_CATEGORY);
        }
      })
  }

  loadSubCategories(categoryId: number) {
    this.isLoading = true;
    this.serviceListing.getSubCategories(categoryId)
      .subscribe({
        next: (res) => {
          if (res.success) {
            this.subCategories = res.data || [];

            if (!this.subCategories.length) {
              this.selectedSubCategoryIndex = null;
              this.services = [];
              this.updateQueryParams(categoryId, null);
              return;
            }

            let subCategoryIndex: number | null = null;

            if (this.savedSubCategoryId) {
              const foundSubCategoryIndex = this.subCategories.findIndex(
                subCategory => subCategory.id === this.savedSubCategoryId
              );

              if (foundSubCategoryIndex !== -1) {
                subCategoryIndex = foundSubCategoryIndex;
              }
            }

            if (subCategoryIndex === null) {
              subCategoryIndex = 0;
            }

            this.selectedSubCategoryIndex = subCategoryIndex;

            const selectedSubCategoryId = this.subCategories[subCategoryIndex]?.id;
            if (!selectedSubCategoryId) return;

            this.updateQueryParams(categoryId, selectedSubCategoryId);
            this.loadServices(selectedSubCategoryId);
          }
        },
        error: () => {
          this.isLoading = false;
          this.toaster.error(SERVICE_LISTING_MESSAGE.error.LOAD_FAIL_SUBCATEGORY);
        }
      })
  }

  loadServices(subCategoryId: number) {
    this.isLoading = true;
    forkJoin({
      services: this.serviceDetail.getServicesBySubCategory(subCategoryId),
      partners: this.slotService.getServicePartnerAvailability()
    }).subscribe({
      next: ({ services, partners }) => {
        if (services.success) {
          this.isLoading = false;
          this.services = (services.data || []).map((service): IServiceView => ({
            ...service,
            hasPartner: partners.data[service.id!] ?? false,
            images: (service.imagePaths || []).map((img: string) => this.getImageUrl(img))
          }));
        }
      },
      error: () => {
        this.isLoading = false;
        this.toaster.error(SERVICE_LISTING_MESSAGE.error.LOAD_FAIL_SERVICE);
      }
    });
  }

  getImageUrl(path: string): string {
    if (!path) return DEFAULT_IMG;
    if (path.startsWith('http://') || path.startsWith('https://')) {
      return path;
    }
    return `${API_BASE_URL}${path}`;
  }

  selectCategory(categoryIndex: number) {
    this.resetSearch();

    if (this.openedCategoryIndex === categoryIndex) {
      this.openedCategoryIndex = null;
      return;
    }

    this.openedCategoryIndex = categoryIndex;
    this.selectedCategoryIndex = categoryIndex;

    const categoryId = this.categories[categoryIndex]?.id;
    if (!categoryId) return;

    this.savedCategoryId = categoryId;

    this.updateQueryParams(categoryId, this.savedSubCategoryId);
    this.loadSubCategories(categoryId);
  }

  selectSubCategory(subCategoryIndex: number) {
    this.resetSearch();
    this.selectedSubCategoryIndex = subCategoryIndex;
    const categoryId =
      this.selectedCategoryIndex !== null
        ? this.categories[this.selectedCategoryIndex]?.id
        : null;

    const subCategoryId = this.subCategories[subCategoryIndex]?.id;
    if (!subCategoryId) return;

    this.savedSubCategoryId = subCategoryId;
    this.updateQueryParams(categoryId ?? null, subCategoryId);
    this.loadServices(subCategoryId);
    this.openedCategoryIndex = null;
    if (this.isOpen) {
      this.openedCategoryIndex = null;
      this.closePanel();
    }
    this.closePanel();
  }

  onSearchCleared() {
    this.isSearchActive = false;
    if (this.selectedSubCategoryIndex !== null) {
      const subCategoryId = this.subCategories[this.selectedSubCategoryIndex]?.id;
      if (subCategoryId) {
        this.loadServices(subCategoryId);
      }
    } else {
      this.services = [];
    }
  }

  private resetSearch() {
    if (this.isSearchActive) {
      this.isSearchActive = false;
      this.searchBoxRef?.clearSearch();
    }
  }


  onViewClick(id: number) {
    this.router.navigate(['/customer/service-details', id]);
  }

  onAddClick(id: number) {
    const selectedCategoryId =
      this.selectedCategoryIndex !== null
        ? this.categories[this.selectedCategoryIndex]?.id
        : null;

    const selectedSubCategoryId =
      this.selectedSubCategoryIndex !== null
        ? this.subCategories[this.selectedSubCategoryIndex]?.id
        : null;

    const service = this.services.find(s => s.id === id);
    this.updateQueryParams(selectedCategoryId!, selectedSubCategoryId!);
    if (!service?.isAvailable) {
      this.toaster.error(SERVICE_LISTING_MESSAGE.error.SERVICE_NOT_AVAILABLE);
      return;
    }
    if (!service?.hasPartner) {
      this.toaster.error(SERVICE_LISTING_MESSAGE.error.PARTNER_NOT_AVAILABLE)
      return;
    }

    this.router.navigate(['/customer/checkout', id], {
      queryParams: {
        id: this.serviceTypeId,
        name: this.serviceTypeName,
        categoryId: selectedCategoryId,
        subCategoryId: selectedSubCategoryId
      }
    });
  }


  private updateQueryParams(categoryId: number | null, subCategoryId: number | null) {
    this.router.navigate([], {
      relativeTo: this.route,
      queryParams: {
        categoryId,
        subCategoryId
      },
      queryParamsHandling: 'merge',
      replaceUrl: true
    });
  }

  getServiceImage(service: IServiceView): string {
    return service?.images?.[0] || DEFAULT_IMG;
  }

  getSubCategoryImage(id: number) {
    const service = this.allServicesByType
      ?.find(s => s.subCategoryId === id);

    return service?.images[0] || DEFAULT_IMG;
  }

  openPanel() {
    this.isOpen = true;
    document.body.style.overflow = 'hidden';
  }

  closePanel() {
    this.isOpen = false;
    document.body.style.overflow = '';
  }

  @HostListener('window:resize') onResize() {
    if (window.innerWidth >= 768 && this.isOpen) {
      this.closePanel();
    }
  }

  private resizeObserver!: ResizeObserver;

  ngAfterViewInit() {
    setTimeout(() => {
      this.calculateOverflow();
      this.observeResize();
    }, 100);
  }

  calculateOverflow() {
    this.descRefs.forEach((ref, i) => {
      const el = ref.nativeElement;

      el.classList.remove('clamp');
      const fullHeight = el.scrollHeight;
      el.classList.add('clamp');

      const clampedHeight = el.clientHeight;

      this.isOverflow[i] = fullHeight > clampedHeight;

      if (this.expanded[i] === undefined) {
        this.expanded[i] = false;
      }
    });

    this.cdr.detectChanges();
  }

  observeResize() {
    this.resizeObserver = new ResizeObserver(() => {
      this.expanded = this.expanded.map(() => false);
      this.calculateOverflow();
    });
    this.resizeObserver.observe(document.body);
  }

  ngOnDestroy() {
    if (this.resizeObserver) {
      this.resizeObserver.disconnect();
    }
  }

  toggle(index: number) {
    this.expanded[index] = !this.expanded[index];
  }

}