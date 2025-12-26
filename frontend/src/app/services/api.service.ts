import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { HelloWorldResponse } from '../models/hello-world-response.model';

@Injectable({
  providedIn: 'root',
})
export class ApiService {
  private readonly apiUrl = '/api/hello';

  constructor(private http: HttpClient) {}

  getHello(): Observable<HelloWorldResponse> {
    return this.http.get<HelloWorldResponse>(this.apiUrl);
  }

  getHelloWithName(name: string): Observable<HelloWorldResponse> {
    return this.http.get<HelloWorldResponse>(`${this.apiUrl}/${name}`);
  }
}
