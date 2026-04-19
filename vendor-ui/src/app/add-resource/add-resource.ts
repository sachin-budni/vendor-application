import { Component, EventEmitter, Output, OnInit, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { LucideAngularModule, X, ChevronDown, Calendar } from 'lucide-angular';
import { ResourceService } from '../services/resource.service';
import { AuthService } from '../services/auth.service';

@Component({
  selector: 'app-add-resource',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, LucideAngularModule],
  templateUrl: './add-resource.html'
})
export class AddResource implements OnInit {
  @Input() resourceData: any = null;
  @Output() close = new EventEmitter<void>();
  @Output() save = new EventEmitter<any>();

  resourceForm: FormGroup;
  isSaving = false;
  
  disciplines: any[] = [];
  groups: any[] = [];
  skillLevels: any[] = [];
  vendors: any[] = [];
  isAdmin = false;
  loggedInVendorName = '';

  readonly X = X;
  readonly ChevronDown = ChevronDown;
  readonly Calendar = Calendar;

  constructor(
    private fb: FormBuilder, 
    private resourceService: ResourceService,
    private authService: AuthService
  ) {
    this.resourceForm = this.fb.group({
      disciplineId: [''],
      rtuEngineerName: ['', Validators.required],
      dateAvailableForm: [''],
      totalExperienceInYears: [null, [Validators.min(0)]],
      totalExperienceInRTU: [null, [Validators.min(0)]],
      skillLevelId: [''],
      remarks: [''],
      currentProjectName: [''],
      hwlManager: [''],
      groupId: [''],
      vendorId: ['']
    });
  }

  ngOnInit() {
    this.checkRole();
    this.fetchLookups();
    if (this.resourceData) {
      this.resourceForm.patchValue({
        disciplineId: this.resourceData.disciplineId,
        rtuEngineerName: this.resourceData.engineerName,
        dateAvailableForm: this.resourceData.availableFromDate ? new Date(this.resourceData.availableFromDate).toISOString().split('T')[0] : '',
        totalExperienceInYears: this.resourceData.totalExperienceYears,
        totalExperienceInRTU: this.resourceData.relevantExperienceYears,
        skillLevelId: this.resourceData.skillLevelId,
        remarks: this.resourceData.remarks,
        currentProjectName: this.resourceData.currentProjectName,
        hwlManager: this.resourceData.managerName,
        groupId: this.resourceData.groupId,
        vendorId: this.resourceData.vendorId
      });
    }
  }

  checkRole() {
    this.isAdmin = this.authService.isAdmin();
    if (this.isAdmin) {
      this.resourceForm.get('vendorId')?.setValidators(Validators.required);
    } else {
      this.loggedInVendorName = this.authService.getVendorNameFromToken() || this.authService.getVendorName() || 'Unknown Vendor';
    }
  }

  fetchLookups() {
    this.resourceService.getDisciplines().subscribe({ next: (res) => this.disciplines = res.data || [] });
    this.resourceService.getGroups().subscribe({ next: (res) => this.groups = res.data || [] });
    this.resourceService.getSkillLevels().subscribe({ next: (res) => this.skillLevels = res.data || [] });
    if (this.isAdmin) {
      this.resourceService.getVendors().subscribe({ next: (res) => this.vendors = res.data || [] });
    }
  }

  onSubmit() {
    if (this.resourceForm.valid) {
      this.isSaving = true;
      const formValue = this.resourceForm.value;
      
      const vId = this.isAdmin ? parseInt(formValue.vendorId) : this.authService.getVendorIdFromToken();

      const payload = {
        vendorId: vId,
        disciplineId: formValue.disciplineId ? parseInt(formValue.disciplineId) : null,
        engineerName: formValue.rtuEngineerName,
        availableFromDate: formValue.dateAvailableForm ? new Date(formValue.dateAvailableForm).toISOString() : null,
        totalExperienceYears: Number(formValue.totalExperienceInYears) || 0,
        relevantExperienceYears: Number(formValue.totalExperienceInRTU) || 0,
        skillLevelId: parseInt(formValue.skillLevelId),
        remarks: formValue.remarks,
        currentProjectName: formValue.currentProjectName,
        managerName: formValue.hwlManager,
        groupId: parseInt(formValue.groupId)
      };

      const request = (this.resourceData && this.resourceData.resourceId) 
        ? this.resourceService.updateResource(this.resourceData.resourceId, payload)
        : this.resourceService.createResource(payload);

      request.subscribe({
        next: (res) => {
          this.isSaving = false;
          this.save.emit(res);
        },
        error: (err) => {
          console.error('Resource operation failed', err);
          this.isSaving = false;
        }
      });
    } else {
      this.resourceForm.markAllAsTouched();
    }
  }

  onClose() {
    this.close.emit();
  }
}
