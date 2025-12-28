import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ApiService } from '../../services/api.service';
import { HelloWorldResponse } from '../../models/hello-world-response.model';

@Component({
  selector: 'app-hello-world',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './hello-world.component.html',
  styleUrls: ['./hello-world.component.scss'],
})
export class HelloWorldComponent implements OnInit {
  message: string = '';
  timestamp: string = '';
  name: string = '';
  isLoading: boolean = false;
  error: string = '';

  constructor(private readonly apiService: ApiService) {}

  ngOnInit(): void {
    this.loadHello();
  }

  loadHello(): void {
    this.isLoading = true;
    this.error = '';

    this.apiService.getHello().subscribe({
      next: (response: HelloWorldResponse) => {
        this.message = response.message;
        this.timestamp = new Date(response.timestamp).toLocaleString();
        this.isLoading = false;
      },
      error: (error) => {
        this.error = 'Failed to load message from API';
        this.isLoading = false;
        console.error('API Error:', error);
      },
    });
  }

  loadHelloWithName(): void {
    if (!this.name.trim()) {
      return;
    }

    this.isLoading = true;
    this.error = '';

    this.apiService.getHelloWithName(this.name.trim()).subscribe({
      next: (response: HelloWorldResponse) => {
        this.message = response.message;
        this.timestamp = new Date(response.timestamp).toLocaleString();
        this.isLoading = false;
      },
      error: (error) => {
        this.error = 'Failed to load personalized message from API';
        this.isLoading = false;
        console.error('API Error:', error);
      },
    });
  }
}
