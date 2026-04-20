import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { LucideAngularModule, ChevronLeft, Download } from 'lucide-angular';
import { ResourceResponseDto } from '../resources/resources';
import { ResourceService } from '../services/resource.service';
import { AuthService } from '../services/auth.service';
import { forkJoin } from 'rxjs';

interface HeadCountRow {
  vendorName: string;
  counts: { [discipline: string]: number };
  total: number;
}

@Component({
  selector: 'app-head-count',
  standalone: true,
  imports: [CommonModule, RouterModule, LucideAngularModule],
  templateUrl: './head-count.html',
  styles: [`
    .vertical-text {
      writing-mode: vertical-rl;
      transform: rotate(180deg);
      text-align: left;
      white-space: break-spaces;
      padding-top: 0.5rem;
      padding-bottom: 0.5rem;
    }
  `]
})
export class HeadCount implements OnInit {
  resources: ResourceResponseDto[] = [];

  disciplines: string[] = [];
  rows: HeadCountRow[] = [];
  totalsByDiscipline: { [discipline: string]: number } = {};
  grandTotal: number = 0;
  isLoading = true;
  isExporting = false;
  loggedInUsername = '';
  loggedInVendorName = '';
  readonly ChevronLeft = ChevronLeft;
  readonly Download = Download;

  constructor(private resourceService: ResourceService, private authService: AuthService) { }

  ngOnInit() {
    this.loadHeaderIdentity();
    this.fetchResources();
  }

  loadHeaderIdentity() {
    this.loggedInUsername = this.authService.getUsername() || '';
    this.loggedInVendorName = this.authService.getVendorName() || (this.authService.isAdmin() ? 'All Vendors' : '');
  }

  fetchResources() {
    this.isLoading = true;

    const obs$: any = {
      resources: this.resourceService.getResources(),
      disciplines: this.resourceService.getDisciplines()
    };

    if (this.authService.isAdmin()) {
      obs$.vendors = this.resourceService.getVendors();
    }

    forkJoin(obs$).subscribe({
      next: (responses: any) => {
        if (responses.resources.success && responses.resources.data) {
          this.resources = responses.resources.data.data;
        }

        let allDisciplines: string[] = [];
        if (responses.disciplines && responses.disciplines.data) {
          allDisciplines = responses.disciplines.data
            .map((d: any) => d.disciplineName)
            .filter((d: string) => d);
        }

        let allVendors: string[] = [];
        if (this.authService.isAdmin() && responses.vendors && responses.vendors.data) {
          allVendors = responses.vendors.data
            .map((v: any) => v.vendorName)
            .filter((v: string) => v);
        } else {
          // If vendor logic: restricted to 1 vendor essentially
          const vName = this.authService.getVendorNameFromToken();
          if (vName) {
            allVendors = [vName];
          }
        }

        this.calculateHeadCount(allDisciplines, allVendors);
        this.isLoading = false;
      },
      error: (err) => {
        console.error('Failed to fetch headcount data', err);
        this.isLoading = false;
      }
    });
  }

  calculateHeadCount(allDisciplines: string[], allVendors: string[]) {
    this.disciplines = Array.from(new Set(allDisciplines)).sort();
    this.rows = [];
    this.totalsByDiscipline = {};
    this.grandTotal = 0;

    const vendorMap = new Map<string, HeadCountRow>();

    allVendors.forEach(v => {
      vendorMap.set(v, { vendorName: v, counts: {}, total: 0 });
    });

    if (this.resources && this.resources.length > 0) {
      this.resources.forEach(res => {
        const vendorName = res.vendorName || 'Unknown';
        const disciplineName = res.disciplineName || 'Unknown';

        let row = vendorMap.get(vendorName);
        if (!row) {
          row = { vendorName, counts: {}, total: 0 };
          vendorMap.set(vendorName, row);
        }

        row.counts[disciplineName] = (row.counts[disciplineName] || 0) + 1;
        row.total++;
        this.grandTotal++;

        this.totalsByDiscipline[disciplineName] = (this.totalsByDiscipline[disciplineName] || 0) + 1;
      });
    }

    this.rows = Array.from(vendorMap.values()).sort((a, b) => a.vendorName.localeCompare(b.vendorName));
  }

  exportExcel() {
    this.isExporting = true;
    const vId = this.authService.getVendorIdFromToken() || undefined;
    this.resourceService.exportHeadCountToExcel({ vendorId: vId }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'HeadCount.xlsx';
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
}
