import { Component, ElementRef, HostListener, Input, ViewChild } from '@angular/core';
import { CommonModule, Location } from '@angular/common';
import { Router, RouterLink } from '@angular/router';
import { MatMenuModule } from '@angular/material/menu';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { AuthService } from '../../../../core/services/auth/auth-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { Subscription } from 'rxjs';
import { ChatBotService } from '../../../../core/services/chatbot/chatbot-service';
import { CUSTOMER_ACCESS_TOKEN_KEY } from '../../../../core/constants/environment-config';

@Component({
  selector: 'app-customer-header',
  imports: [CommonModule, RouterLink, MatMenuModule, MatButtonModule, MatIconModule],
  templateUrl: './customer-header.html',
  styleUrl: './customer-header.css',
})
export class CustomerHeader {
  isLoggedIn = false;
  isDropdownOpen = false;
  isMobileMenuOpen = false;
  private authSub: Subscription | undefined;

  @ViewChild('dropdownRef') dropdownRef!: ElementRef;

  constructor(
    private auth: AuthService,
    private toaster: Toaster,
    private router: Router,
    private eRef: ElementRef,
    private location: Location,
    private chatbot: ChatBotService
  ) {}

  ngOnInit() {
    this.authSub = this.auth.isLoggedIn$.subscribe((value) => {
      this.isLoggedIn = value;
    });
  }

  ngOnDestroy() {
    this.authSub?.unsubscribe();
  }

  @Input() showMenu: boolean = true;
  @Input() showLogin: boolean = true;
  @Input() showProfile: boolean = true;
  @Input() showBackButton: boolean = false;
  @Input() headerTitle: boolean = false;
  @Input() isMobileMenu: boolean = true;
  @Input() isCheckoutHeader = false;

  menuItems = [
    { label: 'Home', route: '' },
    { label: 'Services', route: 'services' },
    { label: 'Contact Us', route: 'contact-us' },
  ];

  toggleDropdown(): void {
    this.isDropdownOpen = !this.isDropdownOpen;
  }

  closeDropdown(): void {
    this.isDropdownOpen = false;
  }

  toggleMobileMenu(): void {
    this.isMobileMenuOpen = !this.isMobileMenuOpen;
    document.body.style.overflow = 'hidden';
  }

  closeMobileMenu(): void {
    this.isMobileMenuOpen = false;
    document.body.style.overflow = '';
  }

  logout() {
    this.auth.logout().subscribe({
      next: (res) => {
        if (res.success) {
          this.chatbot.triggerLogoutReset();
          this.toaster.success(res.message);
          localStorage.removeItem(CUSTOMER_ACCESS_TOKEN_KEY);
          localStorage.removeItem('booking_tab');
          localStorage.removeItem('email_change_pending_email');
          localStorage.removeItem('email_change_expires_at');
          this.auth.setLoggedIn(false);
          this.router.navigate(['/customer/home']);
        } else {
          this.toaster.error(res.message);
        }
      },
      error: (err) => {
        this.toaster.error(err.message);
      },
    });
    this.closeDropdown();
    this.closeMobileMenu();
  }

  goBack() {
    this.location.back();
  }

  goBackFromCheckout(): void {
    this.location.back();
  }

  @HostListener('document:click', ['$event'])
  clickOutside(event: MouseEvent): void {
    if (
      this.isDropdownOpen &&
      this.dropdownRef &&
      !this.dropdownRef.nativeElement.contains(event.target)
    ) {
      this.closeDropdown();
    }
  }
}
