export type ExportMode = 'all' | 'currentPage';

export interface IExportConfig {
    csvUrl?: string;
    pdfUrl?: string;
    filename?: string; 
    mode?: ExportMode;
    queryParams?: () => Record<string, any>; 
  }