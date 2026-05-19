
import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { LeaveService } from '../../../core/services/partner_leave/leave.js';
import { Toaster } from '../../../core/services/toaster/toaster.js';
import { AdminLeaveItem } from '../../../core/models/partner_leave/leave.js';
import { NotificationBell } from '../../../shared/components/notification-bell/notification-bell';
import { MatPaginatorModule } from '@angular/material/paginator';
@Component({
  selector: 'app-admin-leave-requests',
  standalone: true,
  imports: [CommonModule, FormsModule,NotificationBell,MatPaginatorModule],
  templateUrl: './admin-leave-request.html',
  styleUrl: './admin-leave-request.css'
})
export class AdminLeaveRequest implements OnInit {
  leaves: AdminLeaveItem[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 10;
  filterStatus: number | null = null;
  reviewingLeave: AdminLeaveItem | null = null;
  remarks = '';
  reviewing = false;

  constructor(
    private leaveService: LeaveService,
    private toastr: Toaster
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    const filter: any = {
      pageNumber: this.page,
      pageSize: this.pageSize
    };
    if (this.filterStatus !== null) filter.statusId = this.filterStatus;

    this.leaveService.getAllLeaves(filter).subscribe(r => {
      this.leaves = r.data.data;
      this.totalCount = r.data.totalCount;
    });
  }

  setFilter(status: number | null): void {
    this.filterStatus = status;
    this.page = 1;
    this.load();
  }

  openReview(leave: AdminLeaveItem): void {
    this.reviewingLeave = leave;
    this.remarks = '';
  }

  review(isApproved: boolean): void {
    if (!this.reviewingLeave) return;
    this.reviewing = true;

    this.leaveService.reviewLeave(this.reviewingLeave.id, {
      isApproved,
      adminRemarks: this.remarks
    }).subscribe({
      next: () => {
        this.reviewingLeave = null;
        this.load();
        this.toastr.success(
          `Leave ${isApproved ? 'approved' : 'rejected'} successfully`);
      },
      error: (e) => this.toastr.error(e.error?.message ?? 'Something went wrong'),
      complete: () => { this.reviewing = false; }
    });
  }

  badgeClass(statusId: number): string {
    const map: Record<number, string> = {
      1: 'al-pending',
      2: 'al-approved',
      3: 'al-rejected',
      4: 'al-cancelled'
    };
    return map[statusId] ?? 'al-cancelled';
  }
  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }
  
  get startItem(): number {
    return (this.page - 1) * this.pageSize + 1;
  }
  
  get endItem(): number {
    return Math.min(this.page * this.pageSize, this.totalCount);
  }
  
  nextPage(): void {
    if (this.page < this.totalPages) {
      this.page++;
      this.load();
    }
  }
  
  prevPage(): void {
    if (this.page > 1) {
      this.page--;
      this.load();
    }
  }
  
  onPageSizeChange(): void {
    this.page = 1;
    this.load();
  }
}