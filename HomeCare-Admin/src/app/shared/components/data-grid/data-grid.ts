import { Component, EventEmitter, Input, Output } from '@angular/core';
import { CurrencyPipe, NgClass, NgFor, NgIf, NgStyle } from '@angular/common';
import { ActionDropdown } from '../action-dropdown/action-dropdown';
import { IdFormatPipe } from '../../pipes/format/id-format-pipe';
import { IGridColumn } from '../../../core/models/shared-components/IDataGridModel';
import { MatTooltip } from '@angular/material/tooltip';
import { TruncateTooltipDirective } from '../../directives/truncate-tooltip';
import { Toaster } from '../../../core/services/toaster/toaster';
import { IDirectAction } from '../../../core/models/shared-components/IDirectAction';

@Component({
  selector: 'app-data-grid',
  imports: [ActionDropdown, NgClass, NgStyle, CurrencyPipe, IdFormatPipe, MatTooltip, TruncateTooltipDirective],
  templateUrl: './data-grid.html',
  styleUrl: './data-grid.css',
  standalone: true
})
export class DataGrid {
  @Input() columns: IGridColumn[] = []
  @Input() data: any[] = [];
  @Input() showActions: boolean = false;
  @Input() directActions: IDirectAction[] = [];
  @Input() sortBy: string = '';
  @Input() sortOrder: string = 'desc';
  @Input() highlightedRowId: number | null = null;
  @Input() isLoading: boolean = false;
  @Input() skeletonRowCount: number = 10;
  
  @Output() actionClicked = new EventEmitter<any>();
  @Output() rowClicked = new EventEmitter<any>();
  @Output() sortChanged = new EventEmitter<{ sortBy: string; sortOrder: string; }>();
  @Output() expertDetailClicked = new EventEmitter<any>();

  constructor(
    private toaster : Toaster
  ){}

  get skeletonRows() {
    return Array(this.skeletonRowCount);
  }

  copyPhone(phone: string) {
    navigator.clipboard.writeText(phone).then(() => {
      this.toaster.success("Mobile Number Copied")
    });
  }
  onAction(action: string, row: any) {
    this.actionClicked.emit({ action, row });
  }

  onSort(col: IGridColumn) {
    if (!col.sortable) return;

    if (this.sortBy === col.field) {
      if (this.sortOrder === 'desc') {
        this.sortOrder = 'asc';
      } else if (this.sortOrder === 'asc') {
        this.sortBy = '';
        this.sortOrder = 'desc';
      }
    } else {
      this.sortBy = col.field;
      this.sortOrder = 'desc';
    }

    this.sortChanged.emit({ sortBy: this.sortBy, sortOrder: this.sortOrder });
  }

  getSortIcon(col: IGridColumn): string {
    if (!col.sortable) return '';
    if (this.sortBy !== col.field) return 'bi-arrow-down-up';
    return this.sortOrder === 'asc' ? 'bi-arrow-up' : 'bi-arrow-down';
  }
  getBookingStatusClass(status: string): string {
    switch (status) {
      case 'Completed':   return 'badge-booking-completed';
      case 'Pending':     return 'badge-booking-pending';
      case 'Cancelled':   return 'badge-booking-cancelled';
      case 'InProgress':  return 'badge-booking-inprogress';       
      default:            return 'badge-booking-default';
    }
  }

  getRatingStarColor(rating: number): string {
    if (rating >= 4) return '#1a8a4a';
    if (rating >= 3) return '#b07d00';
    return '#c0392b';
  }
}
