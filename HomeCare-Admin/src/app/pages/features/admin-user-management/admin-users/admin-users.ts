import { Component, inject, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { GridLayout } from '../../../../shared/components/grid-layout/grid-layout';
import { MatDialog } from '@angular/material/dialog';
import { IFormModalData } from '../../../../core/models/shared-components/IFormModalData.model';
import { FormModal } from '../../../../shared/components/form-modal/form-modal';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { IChangePasswordRequest } from '../../../../core/models/admin-users/IChangePasswordRequest';
import { DeleteConfirmation } from '../../../../shared/components/delete-confirmation/delete-confirmation';
import { IFilterPanelData } from '../../../../core/models/shared-components/IFilterPanel';
import { FilterPanel } from '../../../../shared/components/filter-panel/filter-panel';
import { AuthService } from '../../../../core/services/auth/auth-service';
import { IGridColumn } from '../../../../core/models/shared-components/IDataGridModel';
import { IAdminGridDto } from '../../../../core/models/admin-users/IAdminGridDto';
import { IAdminRequest } from '../../../../core/models/admin-users/IAdminRequest';
import { IFormField } from '../../../../core/models/shared-components/IFormField.model';
import { ADMIN_USERS_MESSAGES } from '../../../../core/constants/admin-user-messages';
import { EAdminRole } from '../../../../core/enums/auth/admin-role';
import { EActiveStatus } from '../../../../core/enums/admin-user-management/active-status';
import { AdminManagementService } from '../../../../core/services/Admin-User-Management/admin-management-service';

@Component({
  selector: 'app-admin-users',
  imports: [GridLayout],
  templateUrl: './admin-users.html',
  styleUrl: './admin-users.css',
})
export class AdminUsers implements OnInit {
  auth = inject(AuthService);

  role = this.auth.role();
  activeFilters: Record<string, any> = {};

  readonly MESSAGES = ADMIN_USERS_MESSAGES;

  constructor(
    private adminService: AdminManagementService,
    private toaster: Toaster,
    private dialog: MatDialog
  ) { }

  columns: IGridColumn[] = [
    { field: 'id', header: 'ID', type: 'id', width: '20%', sortable: true },
    { field: 'name', header: 'Name', width: '25%', sortable: true },
    { field: 'email', header: 'Email', width: '30%', sortable: true },
    { field: 'isActive', header: 'Status', type: 'status', width: '25%' },
  ];

  pageNumber = 1;
  pageSize = 10;
  totalCount = 0;
  filter = { role: null as EAdminRole | null, isActive: null as EActiveStatus | null, sortBy: '', sortOrder: '', userName: '' };
  data: IAdminGridDto[] = [];
  isLoading = true;

  loadAdmins() {
    this.isLoading = true;
    const params: any = { pageNumber: this.pageNumber, pageSize: this.pageSize };
    if (this.filter.userName) params.userName = this.filter.userName;
    if (this.filter.role !== null) params.role = this.filter.role;
    if (this.filter.isActive !== null) params.isActive = this.filter.isActive;
    if (this.filter.sortBy) params.sortBy = this.filter.sortBy;
    if (this.filter.sortOrder) params.sortOrder = this.filter.sortOrder;

    this.adminService.getAllAdminList(params).subscribe({
      next: (res) => {
        this.totalCount = res.data.totalCount;
        this.data = res.data.data?.map((admin: IAdminGridDto) => ({
          id: admin.id,
          name: admin.name,
          email: admin.email,
          mobileNumber: admin.mobileNumber,
          isActive: admin.isActive,
          actions: [
            { label: 'Edit', icon: 'bi-pencil', action: 'edit' },
            { label: 'Change Password', icon: 'bi-lock', action: 'password' },
            { label: 'Delete', icon: 'bi-trash', action: 'delete' },
          ],
        }));
      },
      error: (err) => {
        this.isLoading = false;
        this.toaster.error(err?.error?.message ?? this.MESSAGES.LOAD_FAILED);
      },
      complete: () => {
        this.isLoading = false;
      },
    });
  }

  onPageChanged(event: { pageNumber: number; pageSize: number }) {
    this.pageNumber = event.pageNumber;
    this.pageSize = event.pageSize;
    this.loadAdmins();
  }

  onSortChanged(event: { sortBy: string; sortOrder: string }) {
    this.filter.sortBy = event.sortBy;
    this.filter.sortOrder = event.sortOrder;
    this.pageNumber = 1;
    this.loadAdmins();
  }

  ngOnInit() {
    this.loadAdmins();
  }

  handleAction(event: { action: string; row: IAdminGridDto }) {
    const { action, row } = event;
    switch (action) {
      case 'edit':
        this.openAdminModal(row);
        break;
      case 'password':
        this.changePassword(row);
        break;
      case 'delete':
        this.deleteAdmin(row);
        break;
    }
  }

  openAdminModal(admin?: IAdminRequest) {
    const isEdit = !!admin;
    const fields: IFormField[] = [
      { name: 'name', label: 'Name', type: 'text', required: true, maxLength: 50 },
      { name: 'mobileNumber', label: 'Mobile Number', type: 'tel', required: true, icon: 'phone' },
      { name: 'email', label: 'Email', type: 'email', required: true, icon: 'mail' },
      ...(!isEdit
        ? ([
          { name: 'password', label: 'Password', type: 'password', required: true },
          {
            name: 'confirmPassword',
            label: 'Confirm Password',
            type: 'password',
            required: true,
          },
        ] as IFormField[])
        : []),
    ];

    const modalData: IFormModalData = {
      title: isEdit ? this.MESSAGES.DIALOG.EDIT_TITLE : this.MESSAGES.DIALOG.ADD_TITLE,
      submitLabel: isEdit ? this.MESSAGES.DIALOG.EDIT_SUBMIT : this.MESSAGES.DIALOG.ADD_SUBMIT,
      fields,
      initialData: isEdit
        ? {
          name: admin!.name,
          email: admin!.email.toLowerCase(),
          mobileNumber: admin!.mobileNumber,
        }
        : undefined,

      onSubmit: (formValue) =>
        new Observable((observer) => {
          const req: IAdminRequest = {
            ...(isEdit && { id: admin!.id }),
            name: formValue.name,
            email: formValue.email.toLowerCase(),
            mobileNumber: formValue.mobileNumber,
            password: formValue.password || undefined,
            confirmPassword: formValue.confirmPassword || undefined,
          };

          this.adminService.saveAdminUser(req).subscribe({
            next: (res) => {
              if (res.success) {
                this.toaster.success(res.message);
                this.loadAdmins();
                observer.next(res);
                observer.complete();
              } else {
                this.toaster.error(res.message);
                observer.error(res);
              }
            },
            error: (err) => {
              const message = err.error?.errors
                ? Object.values(err.error.errors).flat().join('\n')
                : err.error?.message ?? this.MESSAGES.GENERIC_ERROR;
              this.toaster.error(message);
              observer.error(err);
            },
          });
        }),
    };

    this.dialog.open(FormModal, { width: '450px', data: modalData, disableClose: true });
  }

  changePassword(admin: IAdminGridDto) {
    const modalData: IFormModalData = {
      title: this.MESSAGES.DIALOG.CHANGE_PASSWORD_TITLE,
      submitLabel: this.MESSAGES.DIALOG.CHANGE_PASSWORD_SUBMIT,
      fields: [
        { name: 'newPassword', label: 'New Password', type: 'password', required: true },
        { name: 'confirmPassword', label: 'Confirm Password', type: 'password', required: true },
      ],
      onSubmit: (formValue) =>
        new Observable((observer) => {
          const req: IChangePasswordRequest = {
            id: admin.id,
            newPassword: formValue.newPassword,
            confirmPassword: formValue.confirmPassword,
          };

          this.adminService.changePassword(req).subscribe({
            next: (res) => {
              if (res.success) {
                this.toaster.success(res.message);
                observer.next(res);
                observer.complete();
              } else {
                this.toaster.error(res.message);
                observer.error(res);
              }
            },
            error: (err) => {
              const message = err.error?.errors
                ? Object.values(err.error.errors).flat().join('\n')
                : err.error?.message ?? this.MESSAGES.GENERIC_ERROR;
              this.toaster.error(message);
              observer.error(err);
            },
          });
        }),
    };

    this.dialog.open(FormModal, { width: '450px', data: modalData, disableClose: true });
  }

  deleteAdmin(admin: IAdminGridDto) {
    const dialogRef = this.dialog.open(DeleteConfirmation, {
      width: '400px',
      disableClose: true,
      data: {
        message: this.MESSAGES.DELETE.body(admin.name),
        apiCall: () => this.adminService.deleteAdminUser(admin.id),
      },
    });

    dialogRef.afterClosed().subscribe((result) => {
      if (result) this.loadAdmins();
    });
  }

  openFilter() {
    const data: IFilterPanelData = {
      fields: [
        {
          key: 'userName',
          label: 'Name',
          type: 'input',
        },
        {
          key: 'role',
          label: 'Role',
          options: [
            { label: 'Super Admin', value: EAdminRole.SuperAdmin },
            { label: 'Admin', value: EAdminRole.Admin },
          ],
        },
        {
          key: 'isActive',
          label: 'Status',
          options: [
            { label: 'Active', value: true },
            { label: 'Inactive', value: false },
          ],
        },
      ],
      initialValues: this.activeFilters,
    };

    const dialogRef = this.dialog.open(FilterPanel, {
      data: { ...data, useMatInput: true },
      position: { right: '0', top: '0' },
      height: '100vh',
      maxWidth: '360px',
      panelClass: 'filter-panel-dialog',
    });

    dialogRef.afterClosed().subscribe((filters: Record<string, any> | null | 'reset') => {
      if (filters === undefined) return;

      if (filters === 'reset') {
        this.activeFilters = {};
        this.filter.userName = '';
        this.filter.role = null;
        this.filter.isActive = null;
        this.pageNumber = 1;
        this.loadAdmins();
        return;
      }

      this.activeFilters = filters!;
      this.filter.userName = filters!['userName']?.trim() ?? '';
      this.filter.role = filters!['role'] ?? null;
      this.filter.isActive = filters!['isActive'] ?? null;
      this.pageNumber = 1;
      this.loadAdmins();
    });
  }
}
