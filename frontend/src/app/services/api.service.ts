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
    // Read file as base64 to ensure binary data integrity across HTTP transport
    return new Observable((observer) => {
      const reader = new FileReader();

      reader.onload = () => {
        // Extract base64 data (remove 'data:image/png;base64,' prefix if present)
        const base64String = (reader.result as string).split(',')[1] || reader.result;

        // Send base64-encoded data with a custom header to tell Lambda it's pre-encoded
        this.http
          .post<ReceiptExtractionResponse>(`${this.receiptUrl}/extract`, base64String, {
            headers: {
              'Content-Type': file.type || 'application/octet-stream',
              'X-File-Content-Encoding': 'base64',
            },
          })
          .subscribe(observer);
      };

      reader.onerror = (error) => {
        observer.error(new Error('Failed to read file: ' + error));
      };

      reader.readAsDataURL(file);
    });
  }
}
