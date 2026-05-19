import { Component, EventEmitter, Input, Output, SimpleChanges, TemplateRef, OnChanges } from '@angular/core';
import { DataGrid } from '../data-grid/data-grid';
import { FormsModule } from '@angular/forms';
import { IGridColumn } from '../../../core/models/shared-components/IDataGridModel';
import { IDirectAction } from '../../../core/models/shared-components/IDirectAction';
import { QueryParamService } from '../../../core/services/shared/query-param-service';
import { ExpandableGrid } from "../expandable-grid/expandable-grid";
import { ExportMode, IExportConfig } from '../../../core/models/shared-components/IExportConfig';
import { ExportService } from '../../../core/services/shared/export.service';
import { HostListener } from '@angular/core';

@Component({
  selector: 'app-grid-layout',
  imports: [DataGrid, FormsModule, ExpandableGrid],
  templateUrl: './grid-layout.html',
  styleUrl: './grid-layout.css',
})
export class GridLayout implements OnChanges {
  @Input() title: string = '';
  @Input() showFilter: boolean = false;
  @Input() showAdd: boolean = false;
  @Input() addButtonLabel: string = 'Add';
  @Input() columns: IGridColumn[] = [];
  @Input() data: any[] = [];
  @Input() showActions: boolean = true;
  @Input() directActions: IDirectAction[] = [];
  @Input() showPagination: boolean = true;
  @Input() totalCount: number = 0;
  @Input() pageNumber: number = 1;
  @Input() pageSize: number = 10;
  @Input() pageSizeOptions: number[] = [5, 10, 25, 50];
  @Input() sortBy: string = '';
  @Input() sortOrder: string = 'desc';
  @Input() detailRoutePath: string = ''; 
  @Input() currentFilters: any = {};
  @Input() expandable: boolean = false;
  @Input() rowDetailTemplate?: TemplateRef<any>;
  @Input() expandedCustomerId: number | null = null;
  @Input() expandedPaymentMethod: number | null = null;
  @Input() expandTriggered: boolean = false;
  @Input() highlightRowKey: string | null = null;
  @Input() isLoading: boolean = false;
  @Input() skeletonRowCount: number = this.pageSize || 10;
  @Input() exportConfig?: IExportConfig;

  @Output() pageChanged = new EventEmitter<{ pageNumber: number; pageSize: number }>();
  @Output() filterClicked = new EventEmitter<void>();
  @Output() addClicked = new EventEmitter<void>();
  @Output() actionClicked = new EventEmitter<any>();
  @Output() rowClicked = new EventEmitter<any>();
  @Output() deleteClicked = new EventEmitter<any>();
  @Output() rowExpanded = new EventEmitter<any>();
  @Output() expertDetailClicked = new EventEmitter<any>();

  internalLoading: boolean = false;
  isExportingCsv = false;
  isExportingPdf = false;
  activeMenu: 'csv' | 'pdf' | null = null;
  private minDisplayTimer: any = null;
  private readonly MIN_SKELETON_MS = 400;

  constructor(
    private queryParamService: QueryParamService,
    private exportService: ExportService,
  ) {}

  ngOnChanges(changes: SimpleChanges): void {

    if (changes['pageNumber']) {
      this.pageNumber = changes['pageNumber'].currentValue;
    }

    if (changes['totalCount'] || changes['pageSize']) {
      this.redirectIfPageEmpty();
    }

    if (changes['isLoading']) {
      const incoming = changes['isLoading'].currentValue;

      if (incoming) {
        clearTimeout(this.minDisplayTimer);
        this.internalLoading = true;
      } else {
        this.minDisplayTimer = setTimeout(() => {
          this.internalLoading = false;
        }, this.MIN_SKELETON_MS);
      }
    }
  }

  exportCSV(mode: ExportMode): void {
    this.activeMenu = null;
    if (!this.exportConfig) return;
    this.exportService.download(
      { ...this.exportConfig, mode },
      'csv',
      () => this.isExportingCsv = true,
      () => this.isExportingCsv = false
    );
  }
  
  exportPDF(mode: ExportMode): void {
    this.activeMenu = null;
    if (!this.exportConfig) return;
    this.exportService.download(
      { ...this.exportConfig, mode },
      'pdf',
      () => this.isExportingPdf = true,
      () => this.isExportingPdf = false
    );
  }

  toggleMenu(menu: 'csv' | 'pdf', event: MouseEvent): void {
    event.stopPropagation(); 
    this.activeMenu = this.activeMenu === menu ? null : menu;
  }

  toggleDropdown(event: MouseEvent): void {
    const element = event.currentTarget as HTMLElement;
    if (document.activeElement === element) {
      element.blur();
      event.preventDefault();
    }
  }

  @HostListener('document:click')
  closeGlobalMenu() {
    this.activeMenu = null;
  }

  private redirectIfPageEmpty(): void {
    if (this.totalCount === 0) {
      return;
    }

    const newTotalPages = Math.ceil(this.totalCount / this.pageSize);
  
    if (this.pageNumber > newTotalPages) {
      this.pageNumber = newTotalPages || 1;
  
      this.pageChanged.emit({
        pageNumber: this.pageNumber,
        pageSize: this.pageSize,
      });
    }
  }

  onRowClicked(row: any) {
    if (this.detailRoutePath) {
      this.queryParamService.navigateToDetail(this.detailRoutePath, row.id, this.currentFilters);
    }
    
    this.rowClicked.emit(row);
  }
 

  @Output() sortChanged = new EventEmitter<{ sortBy: string; sortOrder: string }>();
  get totalPages(): number {
    return Math.ceil(this.totalCount / this.pageSize);
  }

  get rangeStart(): number {
    return this.totalCount === 0 ? 0 : (this.pageNumber - 1) * this.pageSize + 1;
  }

  get rangeEnd(): number {
    return Math.min(this.pageNumber * this.pageSize, this.totalCount);
  }

  get isFirstPage(): boolean {
    return this.pageNumber <= 1;
  }

  get isLastPage(): boolean {
    return this.pageNumber >= this.totalPages;
  }

  onPageSizeChange(newSize: number) {
    this.pageSize = Number(newSize);
    this.pageNumber = 1;
    this.pageChanged.emit({ pageNumber: this.pageNumber, pageSize: this.pageSize });
  }

  goToPrev() {
    if (this.isFirstPage) return;
    this.pageNumber--;
    this.pageChanged.emit({ pageNumber: this.pageNumber, pageSize: this.pageSize });
  }

  goToNext() {
    if (this.isLastPage) return;
    this.pageNumber++;
    this.pageChanged.emit({ pageNumber: this.pageNumber, pageSize: this.pageSize });
  }

  onGridAction(event: any) {
    this.actionClicked.emit(event);
  }
  onGridSort(event: { sortBy: string; sortOrder: string }) {
    this.sortChanged.emit(event);
  }
  onRowDelete(row: any) {
    this.deleteClicked.emit(row);
  }
  onRowExpanded(row: any) {
    this.rowExpanded.emit(row);
  }
}
