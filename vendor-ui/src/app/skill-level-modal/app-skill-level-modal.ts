import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { LucideAngularModule, X } from 'lucide-angular';
import { ResourceService } from '../services/resource.service';

@Component({
    selector: 'app-skill-level-modal',
    standalone: true,
    imports: [CommonModule, LucideAngularModule],
    template: `
    <div class="fixed inset-0 z-[60] flex items-center justify-center p-4 bg-slate-900/60 backdrop-blur-sm animate-in fade-in duration-200">
      <div class="bg-white rounded-3xl shadow-2xl w-full max-w-2xl overflow-hidden border border-slate-100 animate-in zoom-in-95 duration-200">
        <!-- Header -->
        <div class="px-8 py-6 border-b border-slate-100 flex items-center justify-between bg-white">
          <div>
            <h2 class="text-2xl font-bold text-slate-900">Skill Level</h2>
            <p class="text-sm text-slate-500 mt-1">Assessment criteria for each proficiency level.</p>
          </div>
          <button (click)="close.emit()" class="p-2 hover:bg-slate-50 rounded-xl transition-colors text-slate-400 hover:text-slate-600">
            <lucide-angular [img]="X" class="w-6 h-6"></lucide-angular>
          </button>
        </div>

        <!-- Content (Fetching Skills) -->
        <div class="p-8 max-h-[60vh] overflow-y-auto">
          <div class="overflow-hidden border border-slate-200 rounded-2xl shadow-sm">
            <table class="w-full border-collapse bg-white">
              <thead>
                <tr class="bg-slate-50">
                  <th class="px-6 py-4 text-left text-xs font-bold text-slate-500 uppercase tracking-wider border-b border-slate-200 w-24">Id</th>
                  <th class="px-6 py-4 text-left text-xs font-bold text-slate-500 uppercase tracking-wider border-b border-slate-200 w-24">Code</th>
                  <th class="px-6 py-4 text-left text-xs font-bold text-slate-500 uppercase tracking-wider border-b border-slate-200">Definition</th>
                </tr>
              </thead>
              <tbody class="divide-y divide-slate-100">
                <tr *ngFor="let skill of skillLevels" class="hover:bg-indigo-50/30 transition-colors">
                  <td class="px-6 py-4">
                    <span class="inline-flex items-center px-3 py-1 rounded-lg text-sm font-bold bg-indigo-50 text-indigo-700">
                      {{ skill.skillLevelId }}
                    </span>
                  </td>
                  <td class="px-6 py-4">
                    <span class="inline-flex items-center px-3 py-1 rounded-lg text-sm font-bold bg-indigo-50 text-indigo-700">
                      {{ skill.skillCode }}
                    </span>
                  </td>
                  <td class="px-6 py-4 text-sm text-slate-700 font-medium leading-relaxed">
                    {{ skill.skillName }}
                  </td>
                </tr>
                <tr *ngIf="isLoading" class="animate-pulse">
                  <td colspan="3" class="px-6 py-12 text-center text-slate-400 italic">Fetching skill definitions...</td>
                </tr>
              </tbody>
            </table>
          </div>
        </div>

        <!-- <div class="px-8 py-5 bg-slate-50 border-t border-slate-100 flex justify-end">
          <button (click)="close.emit()" class="px-6 py-2.5 bg-white border border-slate-200 rounded-xl text-sm font-bold text-slate-700 hover:bg-slate-50 transition-all shadow-sm">
            Close
          </button>
        </div> -->
      </div>
    </div>
  `
})
export class SkillLevelModal implements OnInit {
    @Output() close = new EventEmitter<void>();
    skillLevels: any[] = [];
    isLoading = true;
    readonly X = X;

    constructor(private resourceService: ResourceService) { }

    ngOnInit() {
        this.resourceService.getSkillLevels().subscribe({
            next: (res) => {
                if (res.success) this.skillLevels = res.data;
                this.isLoading = false;
            },
            error: () => this.isLoading = false
        });
    }
}
