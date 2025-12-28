import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApiService, ReceiptExtractionResponse } from '../../services/api.service';

@Component({
  selector: 'app-receipt-upload',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './receipt-upload.component.html',
  styleUrls: ['./receipt-upload.component.scss'],
})
export class ReceiptUploadComponent {
  isProcessing = false;
  extractionResult: ReceiptExtractionResponse | null = null;
  error: string = '';
  dragOver = false;
  selectedFile: File | null = null;
  previewUrl: string | null = null;

  constructor(private apiService: ApiService) {}

  onFileSelected(event: Event): void {
    const target = event.target as HTMLInputElement;
    if (target.files && target.files.length > 0) {
      this.handleFile(target.files[0]);
    }
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = true;
  }

  onDragLeave(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = false;
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragOver = false;
    
    if (event.dataTransfer?.files && event.dataTransfer.files.length > 0) {
      this.handleFile(event.dataTransfer.files[0]);
    }
  }

  private handleFile(file: File): void {
    // Validate file type
    const allowedTypes = ['image/png', 'image/jpeg', 'image/jpg'];
    if (!allowedTypes.includes(file.type)) {
      this.error = 'Please select a PNG or JPG image file.';
      return;
    }

    // Validate file size (10MB)
    const maxSize = 10 * 1024 * 1024;
    if (file.size > maxSize) {
      this.error = 'File size must be less than 10MB.';
      return;
    }

    this.selectedFile = file;
    this.error = '';
    this.extractionResult = null;

    // Create preview URL
    const reader = new FileReader();
    reader.onload = (e) => {
      this.previewUrl = e.target?.result as string;
    };
    reader.readAsDataURL(file);
  }

  processReceipt(): void {
    if (!this.selectedFile) {
      this.error = 'Please select a file first.';
      return;
    }

    this.isProcessing = true;
    this.error = '';
    this.extractionResult = null;

    this.apiService.extractReceiptData(this.selectedFile).subscribe({
      next: (response) => {
        this.extractionResult = response;
        this.isProcessing = false;
      },
      error: (error) => {
        this.error = 'Failed to process receipt. Please try again.';
        this.isProcessing = false;
        console.error('Receipt processing error:', error);
      },
    });
  }

  clearSelection(): void {
    this.selectedFile = null;
    this.previewUrl = null;
    this.extractionResult = null;
    this.error = '';
  }

  getExtractedDataEntries(): [string, any][] {
    if (!this.extractionResult?.extractedData) {
      return [];
    }
    return Object.entries(this.extractionResult.extractedData);
  }
}
