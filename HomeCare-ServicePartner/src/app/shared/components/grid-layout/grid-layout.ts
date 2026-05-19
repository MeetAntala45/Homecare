import { Component, EventEmitter, Input, Output, SimpleChanges, TemplateRef, OnChanges, ElementRef, ViewChild, OnInit } from '@angular/core';
import { DataGrid } from '../data-grid/data-grid';
import { FormsModule } from '@angular/forms';
import { IGridColumn } from '../../../core/models/shared-components/IDataGridModel';
import { IDirectAction } from '../../../core/models/shared-components/IDirectAction';
import { QueryParamService } from '../../../core/services/shared/query-param-service';
import { ExpandableGrid } from "../expandable-grid/expandable-grid";
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-grid-layout',
  imports: [DataGrid, FormsModule, ExpandableGrid, CommonModule],
  templateUrl: './grid-layout.html',
  styleUrl: './grid-layout.css',
})
export class GridLayout implements OnChanges, OnInit {
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
  @Input() showPendingButton: boolean = false;
  @Input() showCompletedButton: boolean = false;
  @Input() showViewToggle: boolean = false;
  @Input() activeStatus: 'pending' | 'completed' = 'pending';
  @Input() showPending: boolean = false;
  @Input() showCompleted: boolean = false;
  @Input() showFilterInData: boolean = false;
  @Input() initialViewMode: 'list' | 'calendar' = 'list';
  @Input() highlightedRowId: number | null = null;

  @Output() statusChanged = new EventEmitter<'pending' | 'completed'>();
  @Output() pageChanged = new EventEmitter<{ pageNumber: number; pageSize: number }>();
  @Output() filterClicked = new EventEmitter<void>();
  @Output() dataFilterClicked = new EventEmitter<void>();
  @Output() addClicked = new EventEmitter<void>();
  @Output() actionClicked = new EventEmitter<any>();
  @Output() rowClicked = new EventEmitter<any>();
  @Output() deleteClicked = new EventEmitter<any>();
  @Output() rowExpanded = new EventEmitter<any>();
  @Output() expertDetailClicked = new EventEmitter<any>();
  @Output() viewModeChanged = new EventEmitter<'list' | 'calendar'>();
  @Output() listCalled = new EventEmitter<void>();

  @ViewChild('listBtn') listBtn!: ElementRef;
  @ViewChild('calendarBtn') calendarBtn!: ElementRef;

  internalLoading: boolean = false;
  viewMode: 'list' | 'calendar' = 'list';
  indicatorStyle: any = {};

  private minDisplayTimer: any = null;
  private readonly MIN_SKELETON_MS = 400;

  constructor(private queryParamService: QueryParamService) { }

  ngOnInit(): void {
    this.viewMode = this.initialViewMode;
  }

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

  ngAfterViewInit() {
    setTimeout(() => {
      const el =
        this.viewMode === 'list'
          ? this.listBtn?.nativeElement
          : this.calendarBtn?.nativeElement;

      if (el) {
        this.setViewMode(this.viewMode, el);
      }
    });
  }

  setViewMode(mode: 'list' | 'calendar', el?: HTMLElement) {
    this.viewMode = mode;
    this.viewModeChanged.emit(mode);

    if (el) {
      const rect = el.getBoundingClientRect();
      const parentRect = el.parentElement!.getBoundingClientRect();

      this.indicatorStyle = {
        width: `${rect.width}px`,
        left: `${rect.left - parentRect.left}px`
      };
    }

    if (mode === 'list') {
      this.listCalled.emit();
    }
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

  onStatusChanged(status: 'pending' | 'completed') {
    this.activeStatus = status;
    this.statusChanged.emit(status);
  }
}