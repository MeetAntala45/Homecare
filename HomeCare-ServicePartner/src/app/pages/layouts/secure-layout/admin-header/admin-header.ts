import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  inject,
  Output,
  ViewChild,
  OnInit
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatIcon } from "@angular/material/icon";
import { AuthService } from '../../../../core/services/auth/auth-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { ServicePartnerProfile } from '../../../../core/services/service-partner-profile';
import { PartnerSignalRService } from '../../../../core/services/notifications/partner-signalr-service';
import { PartnerNotificationService } from '../../../../core/services/notifications/partner-notification-service';
import { NotificationBell } from '../../../features/notification-bell/notification-bell';
import { ProfileStateService } from '../../../../core/services/profile/profile-state-service';
import { API_BASE_URL, MY_JOBS_VIEW_MODE, SERVICE_PARTNER_ACCESS_TOKEN_KEY } from '../../../../core/constants/environment-config';

@Component({
  selector: 'app-admin-header',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIcon, NotificationBell],
  templateUrl: './admin-header.html',
  styleUrl: './admin-header.css'
})
export class AdminHeader implements OnInit {
  @Output() menuToggle = new EventEmitter<void>();
  @ViewChild('dropdownRef') dropdownRef!: ElementRef;

  private readonly notificationService = inject(PartnerNotificationService);
  private readonly signalRService = inject(PartnerSignalRService);

  baseUrl = API_BASE_URL;
  photoPreview: string = '';
  fullName: string = '';
  isDropdownOpen = false;

  private readonly PARTNER_ID = 125;

  constructor(
    private toaster: Toaster,
    private router: Router,
    private auth: AuthService,
    private profileState: ProfileStateService,
    private partnerProfileService: ServicePartnerProfile
  ) { }

  ngOnInit(): void {
    this.loadProfile();
  
    this.notificationService.load();
  
    const partnerId = parseInt(localStorage.getItem('partner_id') ?? '0');
    if (partnerId) {
      this.signalRService.startConnection(partnerId);
    }
  
    this.profileState.photoUrl$.subscribe(url => {
      if (url) this.photoPreview = url;
    });

    this.profileState.name$.subscribe(name => {
      if(name) this.fullName = name;
    })
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (
      this.isDropdownOpen &&
      this.dropdownRef &&
      !this.dropdownRef.nativeElement.contains(event.target)
    ) {
      this.isDropdownOpen = false;
    }
  }

  toggleDropdown(event: MouseEvent): void {
    event.stopPropagation();
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  closeDropdown(): void {
    this.isDropdownOpen = false;
  }

  loadProfile(): void {
    this.partnerProfileService.getProfile().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.fullName = res.data.fullName || '';

          if (res.data.profileImage) {
            const url = res.data.profileImage.startsWith('http')
              ? res.data.profileImage
              : `${this.baseUrl}${res.data.profileImage}`;

            const img = new Image();
            img.onload = () => { this.photoPreview = url; };
            img.onerror = () => { this.photoPreview = ''; };
            img.src = url;
          }
        }
      },
      error: () => {
        this.toaster.error('Failed to load profile.');
      }
    });
  }

  getAvatarText(): string {
    return this.fullName
      ? this.fullName.split(' ').map(w => w[0]).join('').toUpperCase().slice(0, 2)
      : 'P';
  }

  toggleSidebar(): void {
    this.menuToggle.emit();
  }

  logout(): void {
    this.auth.logout().subscribe({
      next: (res) => {
        if (res.success) {
          this.toaster.success(res.message);
          localStorage.removeItem(SERVICE_PARTNER_ACCESS_TOKEN_KEY);
          localStorage.removeItem('partner_id');
          sessionStorage.removeItem(MY_JOBS_VIEW_MODE)
          this.router.navigate(['/login']);
        } else {
          this.toaster.error(res.message);
        }
      },
      error: (err) => {
        this.toaster.error(err.message);
      }
    });
    this.closeDropdown();
  }
}