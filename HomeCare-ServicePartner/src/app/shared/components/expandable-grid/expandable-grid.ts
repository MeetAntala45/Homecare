import {
  Component,
  EventEmitter,
  Input,
  OnChanges,
  Output,
  SimpleChanges,
  TemplateRef,
} from '@angular/core';
import { NgClass, CurrencyPipe, NgTemplateOutlet, NgStyle } from '@angular/common';
import { IGridColumn } from '../../../core/models/shared-components/IDataGridModel';
import { IdFormatPipe } from '../../pipes/format/id-format-pipe';
import { TruncateTooltipDirective } from '../../directives/truncate-tooltip';
import { MatTooltip } from '@angular/material/tooltip';

@Component({
  selector: 'app-expandable-grid',
  standalone: true,
  imports: [
    NgClass,
    CurrencyPipe,
    IdFormatPipe,
    NgTemplateOutlet,
    NgStyle,
    TruncateTooltipDirective,
    MatTooltip,
  ],
  templateUrl: './expandable-grid.html',
  styleUrl: './expandable-grid.css',
})
export class ExpandableGrid implements OnChanges {
  @Input() columns: IGridColumn[] = [];
  @Input() data: any[] = [];
  @Input() rowDetailTemplate!: TemplateRef<any>;
  @Input() sortBy: string = '';
  @Input() sortOrder: string = 'desc';
  @Input() collapseAll: boolean = false;
  @Input() expandedCustomerId: number | null = null;
  @Input() expandedPaymentMethod: number | null = null;
  @Input() expandTriggered: boolean = false;
  @Input() highlightRowKey: string | null = null;
  @Input() isLoading: boolean = false;
  @Input() skeletonRowCount: number = 10;

  @Output() sortChanged = new EventEmitter<{ sortBy: string; sortOrder: string }>();
  @Output() deleteClicked = new EventEmitter<any>();
  @Output() rowExpanded = new EventEmitter<any>();

  expandedRowIds = new Set<string>();


private getRowKey(row: any): string {
  if (row.id != null) return String(row.id);
  return `${row.customerId}_${row.paymentMethodValue}`;
}

  get skeletonRows() {
    return Array(this.skeletonRowCount);
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['collapseAll']) {
      this.expandedRowIds.clear();
    }

    if (changes['expandTriggered'] && this.expandTriggered === true) {
      this.tryPreExpand();
    }
  }

  
  private tryPreExpand(): void {
    if (this.expandedCustomerId == null || this.expandedPaymentMethod == null) return;

    const key = `${this.expandedCustomerId}_${this.expandedPaymentMethod}`;
    const matchingRow = this.data.find(
      (r) =>
        r.customerId === this.expandedCustomerId &&
        r.paymentMethodValue === this.expandedPaymentMethod
    );

    if (matchingRow) {
      this.expandedRowIds.clear();
      this.expandedRowIds.add(key);
      this.rowExpanded.emit(matchingRow);

      setTimeout(() => {
        const el = document.getElementById(`row-${key}`);
        el?.scrollIntoView({ behavior: 'smooth', block: 'center' });
      }, 150);
    }
  }

  isHighlighted(row: any): boolean {
    return this.highlightRowKey === this.getRowKey(row);
  }

  toggleRow(row: any, event: Event): void {
    event.stopPropagation();
    const key = this.getRowKey(row);
    if (this.expandedRowIds.has(key)) {
      this.expandedRowIds.clear();
    } else {
      this.expandedRowIds.clear();
      this.expandedRowIds.add(key);
      this.rowExpanded.emit(row);
    }
  }

  isExpanded(row: any): boolean {
    return this.expandedRowIds.has(this.getRowKey(row));
  }

  onSort(col: IGridColumn): void {
    if (!col.sortable) return;
    if (this.sortBy === col.field) {
      this.sortOrder = this.sortOrder === 'desc' ? 'asc' : 'desc';
    } else {
      this.sortBy = col.field;
      this.sortOrder = 'desc';
    }
    this.expandedRowIds.clear();
    this.sortChanged.emit({ sortBy: this.sortBy, sortOrder: this.sortOrder });
  }

  getSortIcon(col: IGridColumn): string {
    if (!col.sortable) return '';
    if (this.sortBy !== col.field) return 'bi-arrow-down-up';
    return this.sortOrder === 'asc' ? 'bi-arrow-up' : 'bi-arrow-down';
  }

  onDelete(row: any, event: Event): void {
    event.stopPropagation();
    this.deleteClicked.emit(row);
  }
}
