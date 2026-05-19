import { Component, OnInit } from '@angular/core';
import { IGridColumn } from '../../../../core/models/shared-components/IDataGridModel';
import { CustomerManagementService } from '../../../../core/services/customer-user-management/customer-management-service';
import { Toaster } from '../../../../core/services/toaster/toaster';
import { ICustomerFilterRequest } from '../../../../core/models/customer-user/ICustomerFilterRequest';
import { ICustomerList } from '../../../../core/models/customer-user/ICustomerList';
import { GridLayout } from '../../../../shared/components/grid-layout/grid-layout';
import { IFilterPanelData } from '../../../../core/models/shared-components/IFilterPanel';
import { MatDialog, MatDialogModule } from '@angular/material/dialog';
import { FilterPanel } from '../../../../shared/components/filter-panel/filter-panel';
import { DeleteConfirmation } from '../../../../shared/components/delete-confirmation/delete-confirmation';
import { IFormField } from '../../../../core/models/shared-components/IFormField.model';
import { IFormModalData } from '../../../../core/models/shared-components/IFormModalData.model';
import { Observable } from 'rxjs';
import { ICustomerRequest } from '../../../../core/models/customer-user/ICustomerRequest';
import { FormModal } from '../../../../shared/components/form-modal/form-modal';
import { BlockConfirmation } from '../../../../shared/components/block-confirmation/block-confirmation';
import { ActivatedRoute, Router } from '@angular/router';
import { CUSTOMER_MESSAGES } from '../../../../core/constants/customer-user-messages';
import { ECustomerStatus } from '../../../../core/enums/customer-user-management/customer-status';

@Component({
  selector: 'app-customer-user',
  imports: [GridLayout, MatDialogModule],
  templateUrl: './customer-user.html',
  styleUrl: './customer-user.css',
})
export class CustomerUser implements OnInit {

  readonly MESSAGES = CUSTOMER_MESSAGES;

  constructor(
    private customerService: CustomerManagementService,
    private toaster: Toaster,
    private dialog: MatDialog,
    private router: Router,
    private route: ActivatedRoute
  ) { }

  columns: IGridColumn[] = [
    { header: 'ID', field: 'id', type: 'id', sortable: true },
    { header: 'Name', field: 'name', type: 'text', sortable: true },
    { header: 'Mobile Number', field: 'mobileNumber', type: 'text', sortable: false },
    { header: 'Email', field: 'email', type: 'text', sortable: true },
    { header: 'Pending Bookings', field: 'pendingBookings', type: 'text', sortable: true },
    { header: 'Total Bookings', field: 'totalBookings', type: 'text', sortable: true },
    { header: 'Status', field: 'status', type: 'status', sortable: false },
  ];

  filter: ICustomerFilterRequest = {
    pageNumber: 1,
    pageSize: 10,
    status: '',
    sortBy: '',
    sortOrder: '',
    userName: '',
    minBookings: null,
    maxBookings: null
  };
  totalCount = 0;
  data: ICustomerList[] = [];
  isLoading = true;

  minBookings = 0;
  maxBookings = 999;

  activeFilters: Record<string, any> = {};

  ngOnInit(): void {
    this.route.queryParams.subscribe(params => {
      if (Object.keys(params).length > 0) {
        this.filter.pageNumber = Number(params['pageNumber']) || 1;
        this.filter.pageSize = Number(params['pageSize']) || 10;
        this.filter.sortBy = params['sortBy'] || '';
        this.filter.sortOrder = params['sortOrder'] || '';
        this.filter.status = params['status'] || '';         
        this.filter.userName = params['userName'] || '';     
        this.filter.minBookings = params['minBookings'] ? Number(params['minBookings']) : null;
        this.filter.maxBookings = params['maxBookings'] ? Number(params['maxBookings']) : null;
  
        this.activeFilters = { ...params };
        if (this.activeFilters['minBookings'] || this.activeFilters['maxBookings']) {
          this.activeFilters['bookings'] = {
            min: this.filter.minBookings,
            max: this.filter.maxBookings
          };
        }
      }
  
      this.loadCustomers();
    });
  }

  loadCustomers() {
    this.isLoading = true;
    const params: any = {};

    if (this.filter.pageNumber) params['pageNumber'] = this.filter.pageNumber;
    if (this.filter.pageSize) params['pageSize'] = this.filter.pageSize;
    if (this.filter.sortBy) params['sortBy'] = this.filter.sortBy;
    if (this.filter.sortOrder) params['sortOrder'] = this.filter.sortOrder;
    if (this.filter.status) params['status'] = this.filter.status;
    if (this.filter.userName) params['userName'] = this.filter.userName;
    if (this.filter.minBookings != null) params['minBookings'] = this.filter.minBookings;
    if (this.filter.maxBookings != null) params['maxBookings'] = this.filter.maxBookings;
    if (this.filter.name) params['name'] = this.filter.name;

    this.customerService.getCustomerList(params).subscribe({
      next: (res) => {
        this.totalCount = res.data?.totalCount ?? 0;
        this.minBookings = res.data?.min ?? 0;
        this.maxBookings = res.data?.max ?? 0;
        this.data = (res.data?.data ?? []).map((c: ICustomerList) => ({
          id: c.id,
          name: c.name,
          email: c.email,
          mobileNumber: c.mobileNumber ?? '-',
          pendingBookings: c.pendingBookings,
          totalBookings: c.totalBookings,
          status: c.status,
          actions: [
            ...(c.status === ECustomerStatus.Inactive
              ? [
                  { label: 'Active', icon: 'bi-check2-circle', action: 'activate' },
                  { label: 'Block',    icon: 'bi-slash-circle',  action: 'block'    }
                ]
              : c.status === ECustomerStatus.Blocked
              ? [
                  { label: 'Unblock', icon: 'bi-check-circle', action: 'unblock' },
                  { label: 'Delete',  icon: 'bi-trash',        action: 'delete'  }
                ]
              : [
                  { label: 'Block',  icon: 'bi-slash-circle', action: 'block'  },
                  { label: 'Delete', icon: 'bi-trash',        action: 'delete' }
                ])
          ]
        }));
      },
      error: (err) => {
        this.isLoading = false;
        this.toaster.error(err?.error?.message ?? this.MESSAGES.LOAD_FAILED);
      },
      complete: () => {
        this.isLoading = false;
      }
    });
  }

  openCustomerModal() {
    const fields: IFormField[] = [
      { name: 'name', label: 'Name', type: 'text', required: true , maxLength: 50},
      { name: 'mobileNumber', label: 'Mobile Number', type: 'tel', required: true, icon: 'phone' },
      { name: 'email', label: 'Email', type: 'email', required: true, icon: 'mail' }
    ];

    const modalData: IFormModalData = {
      title: this.MESSAGES.DIALOG.ADD_TITLE,
      submitLabel: this.MESSAGES.DIALOG.ADD_SUBMIT,
      fields,
      onSubmit: (formValue) => new Observable(observer => {
        const req: ICustomerRequest = {
          name: formValue.name,
          mobileNumber: formValue.mobileNumber,
          email: formValue.email,
        };

        this.customerService.addCustomer(req).subscribe({
          next: (res) => {
            if (res.success) {
              this.toaster.success(res.message);
              this.loadCustomers();
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
          }
        });
      })
    };

    this.dialog.open(FormModal, { width: '450px', data: modalData, disableClose: true });
  }

  onPageChanged(event: { pageNumber: number; pageSize: number }) {
    this.filter.pageNumber = event.pageNumber;
    this.filter.pageSize = event.pageSize;
    this.loadCustomers();
  }

  onSortChanged(event: { sortBy: string; sortOrder: string }) {
    this.filter.sortBy = event.sortBy;
    this.filter.sortOrder = event.sortOrder;
    this.filter.pageNumber = 1;
    this.loadCustomers();
  }

  onFilterApplied(filterValues: Partial<ICustomerFilterRequest>) {
    this.filter = {
      ...this.filter,
      ...filterValues,
      pageNumber: 1
    };
    this.loadCustomers();
  }

  handleAction(event: { action: string; row: ICustomerList }) {
    const { action, row } = event;
    switch (action) {
      case 'block': this.blockCustomer(row); break;
      case 'unblock': this.unblockCustomer(row); break;
      case 'delete': this.deleteCustomer(row); break;
      case 'activate': this.activateCustomer(row); break;
    }
  }

  deleteCustomer(customer: ICustomerList) {
    const dialogRef = this.dialog.open(DeleteConfirmation, {
      width: '400px',
      disableClose: true,
      data: {
        message: this.MESSAGES.DELETE.body(customer.name),
        apiCall: () => this.customerService.deleteCustomer(customer.id)
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) this.loadCustomers();
    });
  }

  blockCustomer(customer: ICustomerList) {
    const dialogRef = this.dialog.open(BlockConfirmation, {
      width: '400px',
      disableClose: true,
      data: {
        message: this.MESSAGES.BLOCK.body(customer.name),
        apiCall: () => this.customerService.blockCustomer(customer.id)
      }
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) this.loadCustomers();
    });
  }

  unblockCustomer(customer: ICustomerList) {
    this.customerService.unblockCustomer(customer.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.toaster.success(res.message);
          this.loadCustomers();
        } else {
          this.toaster.error(res.message);
        }
      },
      error: (err) => this.toaster.error(err.message)
    });
  }
  activateCustomer(customer: ICustomerList) {
    this.customerService.activateCustomer(customer.id).subscribe({
      next: (res) => {
        if (res.success) {
          this.toaster.success(res.message);
          this.loadCustomers();
        } else {
          this.toaster.error(res.message);
        }
      },
      error: (err) => this.toaster.error(err?.error?.message)
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
          key: 'status', label: 'Status',
          type: 'select',
          options: [
            { label: 'Active', value: ECustomerStatus.Active },
            { label: 'Blocked', value: ECustomerStatus.Blocked },
            { label: 'Inactive', value: ECustomerStatus.Inactive }
          ]
        },
        {
          key: 'bookings',
          label: 'Bookings',
          type: 'range',
          min: this.minBookings,
          max: this.maxBookings,
        }
      ],
      initialValues: this.activeFilters
    };

    const dialogRef = this.dialog.open(FilterPanel, {
      data: { ...data, useMatInput: true },
      position: { right: '0', top: '0' },
      height: '100vh',
      maxWidth: '360px',
      panelClass: 'filter-panel-dialog'
    });

    dialogRef.afterClosed().subscribe((filters: Record<string, any> | null | 'reset') => {
      if (filters === undefined) return;

      if (filters === 'reset') {
        this.activeFilters = {};
        this.filter.userName = '';
        this.filter.status = '';
        this.filter.pageNumber = 1;
        this.filter.minBookings = null;
        this.filter.maxBookings = null;
        this.loadCustomers();
        return;
      }

      this.activeFilters = filters!;
      this.filter.userName = filters!['userName']?.trim() ?? '';
      this.filter.status = filters!['status'] ?? '';  
      this.filter.minBookings = filters!['bookings']?.min ?? null;
      this.filter.maxBookings = filters!['bookings']?.max ?? null;
      this.filter.pageNumber = 1;
      this.loadCustomers();
    });
  }

}