import { Component, Output, EventEmitter, ElementRef, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MatIconModule } from '@angular/material/icon';
import { IAttachment } from '../../../../core/models/service-partner/service-partner-profile';
import { SERVICE_PARTNER_MESSAGES } from '../../../../core/constants/service-partner-profile-messages';

@Component({
  selector: 'app-file-upload',
  imports: [CommonModule, MatIconModule],
  templateUrl: './file-upload.html',
  styleUrl: './file-upload.css',
})
export class FileUpload {
  @Output() filesChanged = new EventEmitter<IAttachment[]>();
  @ViewChild('fileInput') fileInput!: ElementRef<HTMLInputElement>;
  uploadedFiles: IAttachment[] = [];
  isDragOver = false;
  isDropping = false;

  readonly allowedTypes = ['application/pdf', 'image/jpeg', 'image/png', 'image/jpg'];
  readonly maxFileSizeMb = 20;
  readonly maxFileSizeBytes = this.maxFileSizeMb * 1024 * 1024;

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.isDragOver = false;
    this.isDropping = true;

    const files = event.dataTransfer?.files;
    if (files) {
      this.processFiles(Array.from(files));
    }
    setTimeout(() => (this.isDropping = false), 100);
  }

  onFileInputChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files) {
      this.processFiles(Array.from(input.files));
    }
    input.value = '';
  }

  private processFiles(files: File[]): void {
    const msgs = SERVICE_PARTNER_MESSAGES.attachment;

    files.forEach((file) => {
      if (!this.isValidFileType(file)) {
        alert(`${file.name}: ${msgs.INVALID_TYPE}`);
        return;
      }

      if (!this.isValidFileSize(file)) {
        alert(
          `${file.name}: ${msgs.SIZE_EXCEEDED_PREFIX} ${this.maxFileSizeMb}${msgs.SIZE_EXCEEDED_SUFFIX}`
        );
        return;
      }

      const uniqueFileName = this.getUniqueFileName(file.name);

      const attachment: IAttachment = {
        file,
        fileName: uniqueFileName,
        fileSize: file.size,
        fileType: file.type,
      };

      this.uploadedFiles.push(attachment);
    });

    this.filesChanged.emit(this.uploadedFiles);
  }

  private getUniqueFileName(fileName: string): string {
    const name = fileName.substring(0, fileName.lastIndexOf('.')) || fileName;
    const extension = fileName.substring(fileName.lastIndexOf('.'));

    let newName = fileName;
    let count = 1;

    const existingNames = this.uploadedFiles.map((f) => f.fileName);

    while (existingNames.includes(newName)) {
      newName = `${name}(${count})${extension}`;
      count++;
    }

    return newName;
  }

  private isValidFileType(file: File): boolean {
    return this.allowedTypes.includes(file.type);
  }

  private isValidFileSize(file: File): boolean {
    return file.size <= this.maxFileSizeBytes;
  }

  removeFile(index: number): void {
    this.uploadedFiles.splice(index, 1);
    this.filesChanged.emit(this.uploadedFiles);
  }

  onDivClick(): void {
    if (this.isDropping) return;
    this.fileInput.nativeElement.click();
  }

  formatFileSize(bytes: number): string {
    if (bytes < 1024) return `${bytes} B`;
    if (bytes < 1024 * 1024) return `${(bytes / 1024).toFixed(1)} KB`;
    return `${(bytes / (1024 * 1024)).toFixed(1)} MB`;
  }
}
