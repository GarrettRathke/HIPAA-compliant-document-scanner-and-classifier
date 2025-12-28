import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, timeout } from 'rxjs';

export interface ReceiptExtractionResponse {
  extractedData: { [key: string]: any };
  processingStatus: string;
  processedAt: string;
  errorMessage?: string;
}

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private readonly apiUrl = '/api/hello';
  private readonly receiptUrl = '/api/receipt';

  constructor(private readonly http: HttpClient) {}

  extractReceiptData(file: File): Observable<ReceiptExtractionResponse> {
    const formData = new FormData();
    formData.append('file', file);

    return this.http
      .post<ReceiptExtractionResponse>(`${this.receiptUrl}/extract`, formData)
      .pipe(timeout(30000)); // 30 second timeout
  }
}
