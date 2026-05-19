import { CommonModule } from '@angular/common';
import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { Router, RouterLink, RouterLinkActive } from '@angular/router';
import { MatTooltipModule } from '@angular/material/tooltip';
import { computed } from '@angular/core';
import { AdminSystemNotification } from '../../../../core/services/partner_leave/admin-system-notification.js';

@Component({
  selector: 'app-admin-sidebar',
  standalone: true,
  imports: [RouterLink, RouterLinkActive, CommonModule, MatTooltipModule],
  templateUrl: './admin-sidebar.html',
  styleUrl: './admin-sidebar.css',
})
export class AdminSidebar implements OnInit {
  @Output() sidebarClose = new EventEmitter<void>();

  constructor(
    private router: Router,
    public adminSvc: AdminSystemNotification
  ) {}
  menu = [
    { label: 'Dashboard', route: 'dashboard', icon: 'bi-grid' },
    { label: 'Service Management', route: '/admin/service-management', icon: 'bi-briefcase' },
    {
      label: 'User Management',
      icon: 'bi-people',
      expanded: false,
      children: [
        { label: 'Customers', route: '/admin/customer-users', icon: 'bi-person' },
        { label: 'Service Partners', route: '/admin/service-partners', icon: 'bi-person-badge' },
        { label: 'Admin Users', route: '/admin/admin-users', icon: 'bi-shield-lock' },
      ],
    },
    { label: 'Booking Management', route: '/admin/booking-management', icon: 'bi-calendar-check' },
    { label: 'Offers', route: '/admin/offers', icon: 'bi-tag' },
    { label: 'Payments & Transactions', route: '/admin/payments', icon: 'bi-credit-card' },
    { label: 'Master Data', route: '/admin/master-data', icon: 'bi-database' },
    { label: 'Support', route: '/admin/support', icon: 'bi-headset' },
    // { label: 'Leave Requests', route: '/admin/leave-requests', icon: 'bi-calendar-x' }
  ];

  ngOnInit(): void {
    this.adminSvc.loadAll();
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

  leaveUnreadCount = computed(() =>
    this.adminSvc.notifications().filter(
      n => !n.isRead
    ).length
  );
}