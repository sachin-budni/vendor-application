import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ResourceService {
  private readonly baseUrl = `${environment.apiUrl}/Resources`;

  constructor(private http: HttpClient) {}

  getResources(pageSize: number = 100): Observable<any> {
    const params = new HttpParams().set('PageSize', pageSize.toString());
    return this.http.get<any>(this.baseUrl, { params });
  }

  getResourceById(id: number): Observable<any> {
    return this.http.get<any>(`${this.baseUrl}/${id}`);
  }

  createResource(resource: any): Observable<any> {
    return this.http.post<any>(this.baseUrl, resource);
  }

  updateResource(id: number, resource: any): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, resource);
  }

  updateResourceStatus(id: number, isActive: boolean): Observable<any> {
    return this.http.put<any>(`${this.baseUrl}/${id}`, { isActive });
  }

  deleteResource(id: number): Observable<any> {
    return this.http.delete<any>(`${this.baseUrl}/${id}`);
  }

  exportToExcel(vendorId?: number): Observable<Blob> {
    let params = new HttpParams().set('PageSize', '100');
    if (vendorId) {
      params = params.set('VendorId', vendorId.toString());
    }
    return this.http.get(`${this.baseUrl}/export`, { params, responseType: 'blob' });
  }

  exportHeadCountToExcel(vendorId?: number): Observable<Blob> {
    let params = new HttpParams().set('PageSize', '1000');
    if (vendorId) {
      params = params.set('VendorId', vendorId.toString());
    }
    return this.http.get(`${this.baseUrl}/export-headcount`, { params, responseType: 'blob' });
  }

  // Lookups
  getDisciplines(): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/Disciplines`);
  }

  getGroups(): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/Groups`);
  }

  getSkillLevels(): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/SkillLevels`);
  }

  getVendors(): Observable<any> {
    return this.http.get<any>(`${environment.apiUrl}/Vendors`);
  }
}
