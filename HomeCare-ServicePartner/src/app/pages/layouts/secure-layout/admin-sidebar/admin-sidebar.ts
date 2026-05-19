import { CommonModule } from '@angular/common';
import { Component, computed, EventEmitter, OnInit, Output } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MatTooltipModule } from '@angular/material/tooltip';
import { NotificationBell } from '../../../../shared/components/notification-bell/notification-bell';
import { PartnerSystemNotification } from '../../../../core/services/partner_leave/partner-systerm-notification.js';

@Component({
  selector: 'app-admin-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, CommonModule, MatTooltipModule],
  templateUrl: './admin-sidebar.html',
  styleUrl: './admin-sidebar.css',
})
export class AdminSidebar implements OnInit {
  @Output() sidebarClose = new EventEmitter<void>();
  leaveUnreadCount = computed(() =>
    this.partnerSvc.notifications().filter(
      n => !n.isRead
    ).length
  );
  menu = [
    { label: 'Dashboard',         route: 'dashboard',                        icon: 'bi-grid' },
    { label: 'My Jobs',           route: '/service-partner/my-jobs',         icon: 'bi-briefcase' },
    { label: 'Services',          route: '/service-partner/my-services',     icon: 'bi-tag' },
    { label: 'Reviews & Ratings', route: '/service-partner/ratings-reviews', icon: 'bi-star-half' },
    { label: 'Leave Management',  route: '/service-partner/leaves',          icon: 'bi-calendar-x' },
  ];

  constructor(
    private router: Router,
    public partnerSvc: PartnerSystemNotification
  ) {}

  ngOnInit(): void {
    this.partnerSvc.loadAll();
    this.menu.forEach((item: any) => {
      if (item.children) {
        item.expanded = this.isAnyChildActive(item);
      }
    });
  }

  isAnyChildActive(item: any): boolean {
    return item.children?.some((child: any) =>
      this.router.url.includes(child.route)
    );
  }

  toggleDropdown(item: any): void {
    item.expanded = !item.expanded;
  }

  isTextOverflow(element: HTMLElement): boolean {
    return element.scrollWidth > element.clientWidth;
  }

  closeSidebar(): void {
    this.sidebarClose.emit();
  }

  onMenuClick(): void {
    this.closeSidebar();
  }
}