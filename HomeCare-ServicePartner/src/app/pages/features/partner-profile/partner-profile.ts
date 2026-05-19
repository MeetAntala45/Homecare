
import { Component, inject, NgModule, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import {
  ReactiveFormsModule, FormBuilder, FormGroup,
  FormArray, Validators, FormControl
} from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { InputFields } from './input-fields/input-fields';
import { ProfessionalEntry } from './professional-entry/professional-entry';
import { LanguageEntry } from './language-entry/language-entry';
import { FileUpload } from './file-upload/file-upload';
import { Toaster } from '../../../core/services/toaster/toaster';
import { IAttachment, IDropdownOption, PartnerDocumentItem, PartnerProfileData } from '../../../core/models/service-partner/service-partner-profile';
import { IInputFieldConfig } from '../../../core/models/service-partner/IinputFiledConfig';
import { EGender, ELanguage, EProficiency } from '../../../core/enums/service-partner/service-partner';
import { ServicePartnerProfile } from '../../../core/services/service-partner-profile';
import { PARTNER_PROFILE_MESSAGES } from '../../../core/constants/service-partner-profile-messages';
import { EducationEntry } from './edication-entry/edication-entry';
import { ProfileStateService } from '../../../core/services/profile/profile-state-service';
import { environment } from '../../../../environments/environment';


@Component({
  selector: 'app-partner-profile',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    InputFields,
    EducationEntry,
    ProfessionalEntry,
    LanguageEntry,
    FileUpload
  ],
  templateUrl: './partner-profile.html',
  styleUrl: './partner-profile.css',
})
export class PartnerProfile  implements OnInit {
  private toaster = inject(Toaster);

  profileForm!: FormGroup;
  isLoading = true;
  isSubmitting = false;
  showDocError = false;

  profileData: PartnerProfileData | null = null;
  existingDocuments: PartnerDocumentItem[] = [];
  removedDocumentIds: number[] = [];
  newAttachments: IAttachment[] = [];

  profileImagePreview: string | null = null;
  profileImageFile: File | null = null;

  genderOptions: IDropdownOption[] = [];
  languageOptions: IDropdownOption[] = [];
  proficiencyOptions: IDropdownOption[] = [];

  personalFields: IInputFieldConfig[] = [];
  private readonly BASE_URL = environment.apiUrl; 

  controlNames = [
    'fullName', 'dateOfBirth', 'mobileNumber',
    'email', 'permanentAddress', 'residentialAddress', 'gender'
  ];

  colClasses = [
    'col-12 col-md-6', 'col-12 col-md-6', 'col-12 col-md-6',
    'col-12 col-md-6', 'col-12 col-md-6', 'col-12', 'col-12'
  ];



  constructor(
    private fb: FormBuilder,
    private router: Router,
    private profileState: ProfileStateService,
    private partnerProfileService: ServicePartnerProfile,
  ) {}

  ngOnInit(): void {
    this.buildStaticDropdownOptions();
    this.initForm();
    this.loadProfile();
    this.profileState.photoUrl$.subscribe(url => {
      if(url) this.profileImagePreview = url;
    })
  }

  private buildStaticDropdownOptions(): void {
    this.genderOptions = Object.values(EGender).map(v => ({ label: v, value: v }));
    this.languageOptions = Object.values(ELanguage).map(v => ({ label: v, value: v }));
    this.proficiencyOptions = Object.values(EProficiency).map(v => ({ label: v, value: v }));
  }

  private buildPersonalFields(): void {
    this.personalFields = [
      { type: 'text',     label: 'Full Name',           placeholder: 'Enter full name' },
      { type: 'date',     label: 'DOB',                 placeholder: 'Select date of birth' },
      { type: 'tel',      label: 'Mobile Number',       placeholder: 'Enter 10-digit mobile', suffixIcon: 'phone' },
      { type: 'email',    label: 'Email',               placeholder: 'Enter email address', suffixIcon: 'mail_outline' },
      { type: 'textarea', label: 'Permanent Address',   placeholder: 'Enter permanent address', rows: 3 },
      { type: 'textarea', label: 'Residential Address', placeholder: 'Enter residential address', rows: 3 },
      { type: 'select',   label: 'Gender',              options: this.genderOptions },
    ];
  }
  

  private initForm(): void {
    this.profileForm = this.fb.group({
      personalDetail: this.fb.group({
        fullName:           ['', Validators.required],
        dateOfBirth:        ['', Validators.required],
        mobileNumber:       ['', [Validators.required, Validators.pattern(/^[1-9]\d{9}$/)]],
        email:              ['', [Validators.required, Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/)]],
        permanentAddress:   [''],
        residentialAddress: [''],
        gender:             ['', Validators.required],
      }),
      educationInfoList:   this.fb.array([this.buildEducationGroup()], Validators.required),
      professionalInfoList: this.fb.array([this.buildProfessionalGroup()]),
      languageList:        this.fb.array([this.buildLanguageGroup()], Validators.required),
    });

    this.buildPersonalFields();
  }

  private buildEducationGroup(): FormGroup {
    return this.fb.group({
      schoolCollege: ['', Validators.required],
      passingYear:   ['', [Validators.required, Validators.pattern(/^(19|20)\d{2}$/)]],
      marks:         ['', [Validators.required, Validators.min(0), Validators.max(100)]],
    });
  }

  private buildProfessionalGroup(): FormGroup {
    return this.fb.group({
      companyName: [''], role: [''], fromDate: [''], toDate: [''],
    });
  }

  private buildLanguageGroup(): FormGroup {
    return this.fb.group({
      language:    ['', Validators.required],
      proficiency: ['', Validators.required],
    });
  }

  private loadProfile(): void {
    this.isLoading = true;
    this.partnerProfileService.getProfile().subscribe({
      next: (res) => {
        this.isLoading = false;
        if (res.success) {
          this.profileData = res.data;
          this.existingDocuments = [...res.data.documents];
          this.patchForm(res.data);
          if (res.data.profileImage) {
            const url = this.resolveImageUrl(res.data.profileImage);
            const img = new Image();
            img.onload = () => { this.profileImagePreview = url; };
            img.onerror = () => { this.profileImagePreview = null; };
            img.src = url!;
          }
        } else {
          this.toaster.error(PARTNER_PROFILE_MESSAGES.load.FAILED);
        }
      },
      error: () => {
        this.isLoading = false;
        this.toaster.error(PARTNER_PROFILE_MESSAGES.load.FAILED);
      },
    });
  }

  private patchForm(data: PartnerProfileData): void {
    this.personalDetailGroup.patchValue({
      fullName:           data.fullName,
      dateOfBirth:        data.dateOfBirth ? new Date(data.dateOfBirth) : null,
      mobileNumber:       data.mobileNumber,
      gender:             data.gender,
      permanentAddress:   data.permanentAddress,
      residentialAddress: data.residentialAddress,
      email:              data.email,
    });

    this.educationFormArray.clear();
    if (data.educations.length > 0) {
      data.educations.forEach(edu => {
        const g = this.buildEducationGroup();
        g.patchValue(edu);
        this.educationFormArray.push(g);
      });
    } else {
      this.educationFormArray.push(this.buildEducationGroup());
    }

    this.professionalFormArray.clear();
    if (data.professionalExperiences.length > 0) {
      data.professionalExperiences.forEach(exp => {
        const g = this.buildProfessionalGroup();
        g.patchValue({
          companyName: exp.companyName,
          role:        exp.role,
          fromDate:    exp.fromDate ? new Date(exp.fromDate) : null,
          toDate:      exp.toDate   ? new Date(exp.toDate)   : null,
        });
        this.professionalFormArray.push(g);
      });
    } else {
      this.professionalFormArray.push(this.buildProfessionalGroup());
    }

    this.languageFormArray.clear();
    if (data.languages.length > 0) {
      data.languages.forEach(lang => {
        const g = this.buildLanguageGroup();
        g.patchValue(lang);
        this.languageFormArray.push(g);
      });
    } else {
      this.languageFormArray.push(this.buildLanguageGroup());
    }
  }

  get educationFormArray():    FormArray { return this.profileForm.get('educationInfoList') as FormArray; }
  get professionalFormArray(): FormArray { return this.profileForm.get('professionalInfoList') as FormArray; }
  get languageFormArray():     FormArray { return this.profileForm.get('languageList') as FormArray; }
  get personalDetailGroup():   FormGroup { return this.profileForm.get('personalDetail') as FormGroup; }

  getPersonalControl(name: string): FormControl {
    return this.personalDetailGroup.get(name) as FormControl;
  }

  addEducationEntry():    void { this.educationFormArray.push(this.buildEducationGroup()); }
  addProfessionalEntry(): void { this.professionalFormArray.push(this.buildProfessionalGroup()); }
  addLanguageEntry():     void { this.languageFormArray.push(this.buildLanguageGroup()); }

  onProfileImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;
    
    const file = input.files[0];
    
    if (!['image/jpeg', 'image/png'].includes(file.type)) {
      this.toaster.error(PARTNER_PROFILE_MESSAGES.profileImage.INVALID_TYPE);
      input.value = '';
      return; 
    }
    
    if (file.size > 5 * 1024 * 1024) {
      this.toaster.error(PARTNER_PROFILE_MESSAGES.profileImage.SIZE_EXCEEDED);
      input.value = '';
      return;
    }
  
    this.profileImageFile = file;
    const reader = new FileReader();
    
    reader.onload = (e) => {
      this.profileImagePreview = e.target?.result as string;
    };
    
    reader.onerror = () => {
      this.toaster.error(PARTNER_PROFILE_MESSAGES.profileImage.INVALID_TYPE);
      this.profileImageFile = null;
      input.value = '';
    };
    
    reader.readAsDataURL(file);
  }

  removeExistingDoc(docId: number): void {
    const remaining = this.existingDocuments.length - 1 + this.newAttachments.length;
    if (remaining < 1) {
      this.toaster.error(PARTNER_PROFILE_MESSAGES.document.MIN_REQUIRED);
      return;
    }
    this.existingDocuments = this.existingDocuments.filter(d => d.id !== docId);
    this.removedDocumentIds.push(docId);
  }

  onFilesChanged(attachments: IAttachment[]): void {
    this.newAttachments = attachments;
    this.showDocError = false;
  }

  private isFormValid(): boolean {
    this.profileForm.markAllAsTouched();
    if (this.profileForm.invalid) return false;

    const totalDocs = this.existingDocuments.length + this.newAttachments.length;
    if (totalDocs < 1) {
      this.showDocError = true;
      return false;
    }

    return true;
  }

  private buildFormData(): FormData {
    const personal      = this.personalDetailGroup.value;
    const educationList = this.educationFormArray.value;
    const professionalList = this.professionalFormArray.value;
    const languageList  = this.languageFormArray.value;

    const form = new FormData();
    form.append('personalDetail', JSON.stringify({
      fullName:           personal.fullName,
      dateOfBirth:        personal.dateOfBirth,
      mobileNumber:       personal.mobileNumber,
      email:              personal.email,
      permanentAddress:   personal.permanentAddress,
      residentialAddress: personal.residentialAddress,
      gender:             personal.gender,
    }));

    form.append('educationInfoList',    JSON.stringify(educationList));
    form.append('professionalInfoList', JSON.stringify(
      professionalList.filter((p: any) => p.companyName || p.role)
    ));
    form.append('languageList',         JSON.stringify(languageList));
    form.append('removedDocumentIds',   JSON.stringify(this.removedDocumentIds));

    if (this.profileImageFile instanceof File)
      form.append('profileImage', this.profileImageFile, this.profileImageFile.name);

    this.newAttachments.forEach(a => form.append('attachmentFiles', a.file, a.fileName));

    return form;
  }

  onSaveClick(): void {
    if (!this.isFormValid()) return;

    this.isSubmitting = true;
    const form = this.buildFormData();

    this.partnerProfileService.updateProfile(form).subscribe({
      next: (res) => {
        this.isSubmitting = false;
        if (res.success) {
          this.toaster.success(PARTNER_PROFILE_MESSAGES.save.SUCCESS);
          this.profileData = res.data;
          this.existingDocuments = [...res.data.documents];
          this.removedDocumentIds = [];
          this.newAttachments = [];
          const updatedProfileUrl = this.resolveImageUrl(res.data.profileImage);
          this.profileState.updatePhoto(updatedProfileUrl!);
          this.profileState.updateName(res.data.fullName);          
        } else {
          this.toaster.error(res.message ?? PARTNER_PROFILE_MESSAGES.save.FAILED);
        }
      },
      error: (err) => {
        this.isSubmitting = false;
        this.toaster.error(err?.error?.message ?? PARTNER_PROFILE_MESSAGES.save.FAILED);
      },
    });
  }

  onCancelClick(): void {
    this.router.navigate(['/partner/dashboard']);
  }
  private resolveImageUrl(path: string | null | undefined): string | null {
    if (!path) return null;
    if (path.startsWith('http')) return path;
    return `${this.BASE_URL}${path}`;
  }
}
