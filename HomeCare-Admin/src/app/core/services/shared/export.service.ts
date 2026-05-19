import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { finalize } from 'rxjs';
import { IExportConfig } from '../../models/shared-components/IExportConfig';
import { API_BASE_URL } from '../../constants/environment-config';
    
@Injectable({ providedIn: 'root' })
export class ExportService {
    private baseUrl = `${API_BASE_URL}/api`;

  private http = inject(HttpClient);

  download(
    config: IExportConfig,
    type: 'csv' | 'pdf',
    onStart: () => void,
    onEnd: () => void
  ): void {
    const url = type === 'csv' ? config.csvUrl : config.pdfUrl;
    if (!url) return;

    const mimeType = type === 'csv' ? 'text/csv' : 'application/pdf';
    const extension = type === 'csv' ? 'csv' : 'pdf';
    const filename = `${config.filename ?? 'export'}.${extension}`;

    const rawParams = config.queryParams?.() ?? {};

    let finalParams: Record<string, any>;

    if (config.mode === 'currentPage') {
      finalParams = rawParams;
    } else {
      const { pageNumber, pageSize, ...filtersOnly } = rawParams;
      finalParams = filtersOnly;
    }

    const cleanParams = Object.fromEntries(
      Object.entries(finalParams).filter(([, v]) => v !== null && v !== undefined && v !== '')
    );

    onStart();

    this.http.get(`${this.baseUrl}/${url}`, { params: cleanParams, responseType: 'blob' })      
      .pipe(finalize(() => onEnd()))
      .subscribe({
        next: (blob) => this.triggerDownload(blob, filename, mimeType),
        error: () => console.error(`Failed to export ${type.toUpperCase()}`)
      });
  }

  private triggerDownload(blob: Blob, filename: string, mimeType: string): void {
    const typedBlob = new Blob([blob], { type: mimeType });
    const url = window.URL.createObjectURL(typedBlob);
    const a = document.createElement('a');
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    document.body.removeChild(a);
    window.URL.revokeObjectURL(url);
  }
}