import {
  Component,
  ElementRef,
  EventEmitter,
  HostListener,
  Output,
  ViewChild
} from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { AdminProfile } from '../../../../core/models/profile/profile.model';
import { AdminProfileService } from '../../../../core/services/profile/admin-profile-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { MatIcon } from "@angular/material/icon";
import { ProfileStateService } from '../../../../core/services/profile/profile-state-service';
import { AuthService } from '../../../../core/services/auth/auth-service';
import { NotificationBell } from '../../../features/booking-management/notification-bell/notification-bell';
import { NotificationService } from '../../../../core/services/booking-management/notification-service';
import { resolveImageUrl } from '../../../../core/utils/image-url.util';
import { ADMIN_ACCESS_TOKEN_KEY, ADMIN_USER_ROLE, API_BASE_URL } from '../../../../core/constants/environment-config';

@Component({
  selector: 'app-admin-header',
  standalone: true,
  imports: [CommonModule, RouterLink, MatIcon, NotificationBell],
  templateUrl: './admin-header.html',
  styleUrl: './admin-header.css'
})
export class AdminHeader {
  @Output() menuToggle = new EventEmitter<void>();
  @ViewChild('dropdownRef') dropdownRef!: ElementRef;

  baseUrl = API_BASE_URL;

  profile: AdminProfile = {
    name: '',
    role: '',
    mobileNumber: '',
    email: '',
    address: '',
    profileImage: ''
  };

  photoPreview: string = '';
  isDropdownOpen = false;

  constructor(
    private profileService: AdminProfileService,
    private profileState: ProfileStateService,
    private toaster: Toaster,
    private router: Router,
    private auth: AuthService,
    private notificationService: NotificationService
  ) { }

  ngOnInit(): void {
    this.loadProfile();
    this.notificationService.load()

    this.profileState.photoUrl$.subscribe(url => {
      if (url) this.photoPreview = url;
    });
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
    this.profileService.getProfile().subscribe({
      next: (res) => {
        if (res.success && res.data) {
          this.profile = res.data;
          this.photoPreview = resolveImageUrl(res.data.profileImage);

        } else {
          this.toaster.error(res.message);
        }
      },
      error: () => {
        this.toaster.error('Try to log in again.');
      }
    });
  }

  getAvatarText(): string {
    return this.profile.name
      ? this.profile.name
        .split(' ')
        .map(word => word[0])
        .join('')
        .toUpperCase()
        .slice(0, 2)
      : 'P';
  }

  toggleSidebar(): void {
    this.menuToggle.emit();
  }

  logout() {
    this.auth.logout().subscribe({
      next: (res) => {
        if (res.success) {
          this.toaster.success(res.message);
          localStorage.removeItem(ADMIN_ACCESS_TOKEN_KEY);
          localStorage.removeItem(ADMIN_USER_ROLE);
          this.router.navigate(['login']);
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