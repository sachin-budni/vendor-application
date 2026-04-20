import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { LucideAngularModule, Search, Filter, MoreHorizontal, Download, Plus, ChevronLeft, ChevronRight, User, Pencil, LogOut, ArrowUpDown, ChevronUp, ChevronDown, CheckCircle, Power } from 'lucide-angular';
import { AddResource } from '../add-resource/add-resource';
import { ResourceService } from '../services/resource.service';
import { AuthService } from '../services/auth.service';

export interface ResourceResponseDto {
  resourceId: number;
  vendorId: number;
  vendorName: string;
  disciplineId?: number;
  disciplineCode?: string;
  disciplineName?: string;
  engineerName: string;
  availableFromDate?: string;
  totalExperienceYears: number;
  relevantExperienceYears: number;
  skillLevelId: number;
  skillCode?: string;
  skillName?: string;
  remarks?: string;
  currentProjectName?: string;
  managerName?: string;
  groupId: number;
  groupCode?: string;
  groupName?: string;
  isActive: boolean;
}

interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

interface PagedResponse<T> {
  data: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
}

@Component({
  selector: 'app-resources',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, LucideAngularModule, AddResource],
  templateUrl: './resources.html',
})
export class Resources implements OnInit {
  resources: ResourceResponseDto[] = [];
  filteredResources: ResourceResponseDto[] = [];
  isLoading = false;
  isAddModalOpen = false;
  isExporting = false;
  selectedResource: ResourceResponseDto | null = null;

  // Lookup data for select dropdowns
  vendors: any[] = [];
  disciplines: any[] = [];
  skillLevels: any[] = [];
  groups: any[] = [];

  // API filter selections (by ID)
  selectedVendorId: number | null = null;
  selectedDisciplineId: number | null = null;
  selectedSkillLevelId: number | null = null;
  selectedGroupId: number | null = null;
  selectedIsActive: string = '';

  readonly Search = Search;
  readonly Filter = Filter;
  readonly MoreHorizontal = MoreHorizontal;
  readonly Download = Download;
  readonly Plus = Plus;
  readonly ChevronLeft = ChevronLeft;
  readonly ChevronRight = ChevronRight;
  readonly User = User;
  readonly Pencil = Pencil;
  readonly LogOut = LogOut;
  readonly ArrowUpDown = ArrowUpDown;
  readonly ChevronUp = ChevronUp;
  readonly ChevronDown = ChevronDown;
  readonly CheckCircle = CheckCircle;
  readonly Power = Power;

  sortColumn: string = '';
  sortDirection: 'asc' | 'desc' = 'asc';

  columns: { key: keyof ResourceResponseDto | any; label: string }[] = [
    { key: 'vendorName', label: 'Vendor' },
    { key: 'disciplineName', label: 'Discipline' },
    { key: 'engineerName', label: 'Engineer' },
    { key: 'availableFromDate', label: 'Avail. From' },
    { key: 'totalExperienceYears', label: 'Total Exp(Y)' },
    { key: 'relevantExperienceYears', label: 'Rel Exp(Y)' },
    { key: 'skillCode', label: 'Skill level' },
    { key: 'remarks', label: 'Remarks' },
    { key: 'currentProjectName', label: 'Current Proj' },
    { key: 'managerName', label: 'Manager' },
    { key: 'groupName', label: 'Group' },
    { key: 'isActive', label: 'Status' },
    { key: 'actions', label: 'Actions' }
  ];

  filters: Record<string, string> = {};

  constructor(private resourceService: ResourceService, public authService: AuthService) {
    this.columns.forEach(c => this.filters[c.key] = '');
  }

  ngOnInit() {
    this.loadLookups();
    this.fetchResources();
  }

  loadLookups() {
    this.resourceService.getVendors().subscribe({
      next: (res) => { if (res.success) this.vendors = res.data; }
    });
    this.resourceService.getDisciplines().subscribe({
      next: (res) => { if (res.success) this.disciplines = res.data; }
    });
    this.resourceService.getSkillLevels().subscribe({
      next: (res) => { if (res.success) this.skillLevels = res.data; }
    });
    this.resourceService.getGroups().subscribe({
      next: (res) => { if (res.success) this.groups = res.data; }
    });
  }

  fetchResources() {
    this.isLoading = true;

    // Build API filter params
    const apiFilters: any = {};
    if (this.selectedVendorId) apiFilters.vendorId = this.selectedVendorId;
    if (this.selectedDisciplineId) apiFilters.disciplineId = this.selectedDisciplineId;
    if (this.selectedSkillLevelId) apiFilters.skillLevelId = this.selectedSkillLevelId;
    if (this.selectedGroupId) apiFilters.groupId = this.selectedGroupId;
    if (this.selectedIsActive === 'true') apiFilters.isActive = true;
    if (this.selectedIsActive === 'false') apiFilters.isActive = false;

    // Text-based API filters
    if (this.filters['engineerName']) apiFilters.engineerName = this.filters['engineerName'];
    if (this.filters['currentProjectName']) apiFilters.currentProjectName = this.filters['currentProjectName'];
    if (this.filters['managerName']) apiFilters.managerName = this.filters['managerName'];

    this.resourceService.getResources(apiFilters).subscribe({
      next: (response) => {
        if (response.success && response.data) {
          this.resources = response.data.data;
          this.applyFiltersAndSort();
        }
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to fetch resources', err);
        this.isLoading = false;
      }
    });
  }

  onApiFilterChange() {
    this.fetchResources();
  }

  toggleSort(column: string) {
    if (column === 'actions') return;

    if (this.sortColumn === column) {
      this.sortDirection = this.sortDirection === 'asc' ? 'desc' : 'asc';
    } else {
      this.sortColumn = column;
      this.sortDirection = 'asc';
    }
    this.applyFiltersAndSort();
  }

  applyFiltersAndSort() {
    let result = this.resources.filter(res => {
      let matches = true;
      for (const col of this.columns) {
        // Skip columns handled by API
        if (['vendorName', 'disciplineName', 'skillCode', 'groupName', 'isActive', 'engineerName', 'currentProjectName', 'managerName'].includes(col.key)) continue;

        const filterVal = this.filters[col.key]?.toLowerCase();
        if (filterVal) {
          const val = (res as any)[col.key];
          const cellStr = (val !== null && val !== undefined) ? String(val).toLowerCase() : '';

          if (!cellStr.includes(filterVal)) {
            matches = false;
            break;
          }
        }
      }
      return matches;
    });

    if (this.sortColumn) {
      result.sort((a, b) => {
        const valA = (a as any)[this.sortColumn];
        const valB = (b as any)[this.sortColumn];

        if (valA === valB) return 0;
        if (valA === null || valA === undefined) return 1;
        if (valB === null || valB === undefined) return -1;

        const comparison = valA < valB ? -1 : 1;
        return this.sortDirection === 'asc' ? comparison : -comparison;
      });
    }

    this.filteredResources = result;
  }

  onFilterChange(columnKey: string) {
    if (['engineerName', 'currentProjectName', 'managerName'].includes(columnKey)) {
      this.fetchResources();
    } else {
      this.applyFiltersAndSort();
    }
  }

  clearFilters() {
    // Clear client-side text filters
    Object.keys(this.filters).forEach(k => this.filters[k] = '');
    // Clear API dropdown filters
    this.selectedVendorId = null;
    this.selectedDisciplineId = null;
    this.selectedSkillLevelId = null;
    this.selectedGroupId = null;
    this.selectedIsActive = '';
    this.sortColumn = '';
    this.sortDirection = 'asc';
    this.fetchResources();
  }

  openAddModal() {
    this.selectedResource = null;
    this.isAddModalOpen = true;
  }

  openEditModal(resource: ResourceResponseDto) {
    this.selectedResource = resource;
    this.isAddModalOpen = true;
  }

  exportExcel() {
    this.isExporting = true;

    // Build current filter object
    const apiFilters: any = {};
    const vId = this.authService.getVendorIdFromToken();
    if (vId) apiFilters.vendorId = vId;
    else if (this.selectedVendorId) apiFilters.vendorId = this.selectedVendorId;

    if (this.selectedDisciplineId) apiFilters.disciplineId = this.selectedDisciplineId;
    if (this.selectedSkillLevelId) apiFilters.skillLevelId = this.selectedSkillLevelId;
    if (this.selectedGroupId) apiFilters.groupId = this.selectedGroupId;
    if (this.selectedIsActive === 'true') apiFilters.isActive = true;
    if (this.selectedIsActive === 'false') apiFilters.isActive = false;

    if (this.filters['engineerName']) apiFilters.engineerName = this.filters['engineerName'];
    if (this.filters['currentProjectName']) apiFilters.currentProjectName = this.filters['currentProjectName'];
    if (this.filters['managerName']) apiFilters.managerName = this.filters['managerName'];

    this.resourceService.exportToExcel(apiFilters).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = `Resources_Export_${new Date().toISOString().slice(0, 10)}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
        this.isExporting = false;
      },
      error: (err) => {
        console.error('Export failed', err);
        this.isExporting = false;
      }
    });
  }

  closeAddModal() {
    this.isAddModalOpen = false;
    this.selectedResource = null;
  }

  logout() {
    this.authService.logout();
  }

  onSaveResource(data: any) {
    this.closeAddModal();
    this.fetchResources();
  }

  toggleResourceStatus(id: number, currentStatus: boolean) {
    if (!this.authService.isAdmin()) return;

    this.resourceService.updateResourceStatus(id, !currentStatus).subscribe({
      next: (response) => {
        if (response.success) {
          this.fetchResources();
        }
      },
      error: (err) => console.error('Status toggle failed', err)
    });
  }
}
