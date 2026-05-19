import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import {
  ReactiveFormsModule, FormGroup,
  FormControl, Validators,
  FormsModule
} from '@angular/forms';
import { LeaveService } from '../../../core/services/partner_leave/leave-service.js';
import { Toaster } from '../../../core/services/toaster/toaster.js';
import { LeaveResponse } from '../../../core/models/servicepartner_leave/leave.js';
import { NotificationBell } from '../../../shared/components/notification-bell/notification-bell';
import { FormModal } from '../../../shared/components/form-modal/form-modal.js';

@Component({
  selector: 'app-partner-leaves',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, NotificationBell,FormsModule],
  templateUrl: './partner-leave.html',
  styleUrl: './partner-leave.css'
})
export class PartnerLeave implements OnInit {
  leaves: LeaveResponse[] = [];
  totalCount = 0;
  page = 1;
  pageSize = 10;

  showModal = false;
  submitting = false;
  showCancelConfirm = false;
  selectedLeaveId: number | null = null;
  today = new Date().toISOString().split('T')[0];

  form = new FormGroup({
    fromDate: new FormControl('', Validators.required),
    toDate:   new FormControl('', Validators.required),
    reason:   new FormControl('', [
      Validators.required,
      Validators.minLength(10)
    ])
  });

  constructor(
    private leaveService: LeaveService,
    private toastr: Toaster
  ) {}

  ngOnInit(): void {
    this.load();
  }


  load(): void {
    this.leaveService.getMyLeaves({
      pageNumber: this.page,
      pageSize: this.pageSize
    }).subscribe(r => {
      this.leaves = r.data.data;
      this.totalCount = r.data.totalCount;
    });
  }

  get pendingCount()  { return this.leaves.filter(l => l.statusId === 1).length; }
  get approvedCount() { return this.leaves.filter(l => l.statusId === 2).length; }
  get rejectedCount() { return this.leaves.filter(l => l.statusId === 3).length; }

  get minToDate(): string {
    return this.form.get('fromDate')?.value || this.today;
  }


  openModal():  void { this.showModal = true; this.form.reset(); }
  closeModal(): void { this.showModal = false; }

  submit(): void {
    if (this.form.invalid) { this.form.markAllAsTouched(); return; }
    this.submitting = true;
    this.leaveService.applyLeave(this.form.value as any).subscribe({
      next: () => {
        this.closeModal();
        this.load();
        this.toastr.success('Leave application submitted!');
      },
      error: (e) => {
        this.toastr.error(e.error?.message ?? 'Something went wrong');
        this.submitting = false;
      },
      complete: () => { this.submitting = false; }
    });
  }


  cancel(id: number): void {
    this.leaveService.cancelLeave(id).subscribe({
      next:  () => { this.load(); this.toastr.success('Leave cancelled'); },
      error: (e) => this.toastr.error(e.error?.message)
    });
  }

  openCancelConfirm(id: number): void {
    this.selectedLeaveId = id;
    this.showCancelConfirm = true;
  }

  closeCancelConfirm(): void {
    this.showCancelConfirm = false;
    this.selectedLeaveId = null;
  }

  confirmCancel(): void {
    if (!this.selectedLeaveId) return;
    this.cancel(this.selectedLeaveId);
    this.closeCancelConfirm();
  }

  badgeClass(statusId: number): string {
    const map: Record<number, string> = {
      1: 'lv-pending',
      2: 'lv-approved',
      3: 'lv-rejected',
      4: 'lv-cancelled'
    };
    return map[statusId] ?? 'lv-cancelled';
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