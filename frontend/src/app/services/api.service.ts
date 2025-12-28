import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HelloWorldResponse } from '../models/hello-world-response.model';

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

  getHello(): Observable<HelloWorldResponse> {
    return this.http.get<HelloWorldResponse>(this.apiUrl);
  }

  getHelloWithName(name: string): Observable<HelloWorldResponse> {
    return this.http.get<HelloWorldResponse>(`${this.apiUrl}/${name}`);
  }

  extractReceiptData(file: File): Observable<ReceiptExtractionResponse> {
    const formData = new FormData();
    formData.append('file', file);
    
    return this.http.post<ReceiptExtractionResponse>(`${this.receiptUrl}/extract`, formData);
  }
}
