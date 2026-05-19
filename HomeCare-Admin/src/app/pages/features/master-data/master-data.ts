import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MasterDataService } from '../../../core/services/master-data/master-data-service';
import { FormsModule } from '@angular/forms';
import { MatDialog } from '@angular/material/dialog';
import { AddServiceDialog } from './add-service-dialog/add-service-dialog';
import { MatIcon } from "@angular/material/icon";
import { EditServiceDialog } from './edit-service-dialog/edit-service-dialog';
import { ManageServiceDialog } from './manage-service-dialog/manage-service-dialog';
import { DeleteConfirmation } from '../../../shared/components/delete-confirmation/delete-confirmation';
import { IServiceType } from '../../../core/models/master-data/service-type/service-type';
import { IApiResponse } from '../../../core/models/api-response/api-response';
import { ActionDropdown } from '../../../shared/components/action-dropdown/action-dropdown';
import { Toaster } from '../../../core/services/toaster/toaster';
import { MASTER_DATA_MESSAGES } from '../../../core/constants/master-data-messages';
import { CategoryService } from '../../../core/services/category/category-service';
import { API_BASE_URL } from '../../../core/constants/environment-config';

@Component({
  selector: 'app-master-data',
  imports: [CommonModule, FormsModule, MatIcon, ActionDropdown],
  templateUrl: './master-data.html',
  styleUrl: './master-data.css',
})
export class MasterData {

  BASE_URL = API_BASE_URL;

  serviceTypes: IServiceType[] = [];
  loading = false;

  constructor(
    private serviceTypeService: MasterDataService,
    private dialog: MatDialog,
    private toaster: Toaster,
    private categoryservice: CategoryService
  ) { }

  ngOnInit(): void {
    this.loadServiceTypes();
  }

  loadServiceTypes() {
    this.loading = true;
    this.serviceTypeService.getAll().subscribe({
      next: (res: IApiResponse<IServiceType[]>) => {
        this.serviceTypes = res.data;
        setTimeout(()=>{
          this.loading = false;
        },400);
      },
      error: (err) => {
        this.toaster.error(MASTER_DATA_MESSAGES.common.LOAD_FAILED);
        this.loading = false;
      }
    });
  }

  addServiceDialog() {
    const dialogRef = this.dialog.open(AddServiceDialog, {
      width: '400px'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadServiceTypes();
      }
    });
  }

  manageServiceDialog(service: IServiceType) {
    const dialogRef = this.dialog.open(ManageServiceDialog, {
      maxWidth: '50rem',
      maxHeight: '520px',
      data: service,
      panelClass: 'manage-service-dialog'
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadServiceTypes();
      }
    });
  }

  editServiceDialog(service: IServiceType) {
    const dialogRef = this.dialog.open(EditServiceDialog, {
      width: '400px',
      data: service
    });

    dialogRef.afterClosed().subscribe(result => {
      if (result) {
        this.loadServiceTypes();
      }
    });
  }

  trackById(item: IServiceType) {
    return item.id;
  }

  deleteService(id: number, name: string) {
    this.categoryservice.getCategoryById(id).subscribe({
      next: (res) => {
        if (res.data.length > 0) {
          this.toaster.error(MASTER_DATA_MESSAGES.serviceType.HAS_CATEGORY);
          return;
        }

        const dialogRef = this.dialog.open(DeleteConfirmation, {
          width: '400px',
          data: {
            message: MASTER_DATA_MESSAGES.conformation.SERVICETYPE_DELETED(name),
            apiCall: () => this.serviceTypeService.deleteServiceType(id)
          }
        });

        dialogRef.afterClosed().subscribe(result => {
          if (result) {
            this.serviceTypes = this.serviceTypes.filter(s => s.id !== id);
          }
        });
      },
      error: () => {
        this.toaster.error(MASTER_DATA_MESSAGES.serviceType.FAIL);
      }
    });
  }
  getActions(service: any) {
    return [
      { label: 'Manage', icon: 'bi-list-ul', action: 'manage' },
      { label: 'Edit', icon: 'bi-pencil', action: 'edit' },
      { label: 'Delete', icon: 'bi-trash', action: 'delete' }
    ];
  }

  handleAction(action: string, service: any) {
    if (action === 'manage') {
      this.manageServiceDialog(service);
    }
    if (action === 'edit') {
      this.editServiceDialog(service);
    }
    if (action === 'delete') {
      this.deleteService(service.id, service.name);
    }
  }
}