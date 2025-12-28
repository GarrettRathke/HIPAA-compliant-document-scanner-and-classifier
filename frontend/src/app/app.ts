import { Component, signal } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReceiptUploadComponent } from './components/receipt-upload/receipt-upload.component';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, CommonModule, ReceiptUploadComponent],
  templateUrl: './app.html',
  styleUrl: './app.scss'
})
export class App {
  protected readonly title = signal('receipt-ocr-demo');
  currentView: 'hello' | 'receipt' = 'receipt';
}

