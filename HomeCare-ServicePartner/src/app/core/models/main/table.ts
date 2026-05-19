export interface TableColumn {
    key: string;
    label: string;
    sortable?: boolean;
    width?: string;
    type?: 'text' | 'badge' | 'action' | 'custom';
    badgeConfig?: BadgeConfig;
  }
  
  export interface BadgeConfig {
    [key: string]: 'success' | 'danger' | 'warning' | 'info';
  }
  
  export interface TableConfig {
    columns: TableColumn[];
    rowsPerPageOptions?: number[];
    defaultRowsPerPage?: number;
    totalCount?: number;
  }
  
  export interface SortEvent {
    column: string;
    direction: 'asc' | 'desc';
  }
  
  export interface PageEvent {
    page: number;
    rowsPerPage: number;
  }

  export interface PageAction {
    label: string;
    icon?: string;
    type?: 'primary' | 'secondary' | 'danger';
    action: string;
  }