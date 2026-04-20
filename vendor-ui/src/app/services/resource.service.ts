import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ResourceService {
  private readonly baseUrl = `${environment.apiUrl}/Resources`;

  constructor(private http: HttpClient) { }

  getResources(filters?: {
    vendorId?: number,
    disciplineId?: number,
    skillLevelId?: number,
    groupId?: number,
    isActive?: boolean,
    engineerName?: string,
    currentProjectName?: string,
    managerName?: string
  }): Observable<any> {
    let params = new HttpParams().set('PageSize', '1000');
    if (filters?.vendorId) params = params.set('VendorId', filters.vendorId.toString());
    if (filters?.disciplineId) params = params.set('DisciplineId', filters.disciplineId.toString());
    if (filters?.skillLevelId) params = params.set('SkillLevelId', filters.skillLevelId.toString());
    if (filters?.groupId) params = params.set('GroupId', filters.groupId.toString());
    if (filters?.isActive !== undefined && filters?.isActive !== null) params = params.set('IsActive', filters.isActive.toString());
    if (filters?.engineerName) params = params.set('EngineerName', filters.engineerName);
    if (filters?.currentProjectName) params = params.set('CurrentProjectName', filters.currentProjectName);
    if (filters?.managerName) params = params.set('ManagerName', filters.managerName);
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

  exportToExcel(filters?: {
    vendorId?: number,
    disciplineId?: number,
    skillLevelId?: number,
    groupId?: number,
    isActive?: boolean,
    engineerName?: string,
    currentProjectName?: string,
    managerName?: string
  }): Observable<Blob> {
    let params = new HttpParams().set('PageSize', '10000');
    if (filters?.vendorId) params = params.set('VendorId', filters.vendorId.toString());
    if (filters?.disciplineId) params = params.set('DisciplineId', filters.disciplineId.toString());
    if (filters?.skillLevelId) params = params.set('SkillLevelId', filters.skillLevelId.toString());
    if (filters?.groupId) params = params.set('GroupId', filters.groupId.toString());
    if (filters?.isActive !== undefined && filters?.isActive !== null) params = params.set('IsActive', filters.isActive.toString());
    if (filters?.engineerName) params = params.set('EngineerName', filters.engineerName);
    if (filters?.currentProjectName) params = params.set('CurrentProjectName', filters.currentProjectName);
    if (filters?.managerName) params = params.set('ManagerName', filters.managerName);

    return this.http.get(`${this.baseUrl}/export`, { params, responseType: 'blob' });
  }

  exportHeadCountToExcel(filters?: {
    vendorId?: number,
    disciplineId?: number,
    skillLevelId?: number,
    groupId?: number,
    isActive?: boolean,
    engineerName?: string,
    currentProjectName?: string,
    managerName?: string
  }): Observable<Blob> {
    let params = new HttpParams().set('PageSize', '10000');
    if (filters?.vendorId) params = params.set('VendorId', filters.vendorId.toString());
    if (filters?.disciplineId) params = params.set('DisciplineId', filters.disciplineId.toString());
    if (filters?.skillLevelId) params = params.set('SkillLevelId', filters.skillLevelId.toString());
    if (filters?.groupId) params = params.set('GroupId', filters.groupId.toString());
    if (filters?.isActive !== undefined && filters?.isActive !== null) params = params.set('IsActive', filters.isActive.toString());
    if (filters?.engineerName) params = params.set('EngineerName', filters.engineerName);
    if (filters?.currentProjectName) params = params.set('CurrentProjectName', filters.currentProjectName);
    if (filters?.managerName) params = params.set('ManagerName', filters.managerName);

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
