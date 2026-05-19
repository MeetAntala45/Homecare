import { Component, inject, OnInit, ViewChild } from '@angular/core';
import { CommonModule } from '@angular/common';
import { Router } from '@angular/router';
import { ReactiveFormsModule, FormBuilder, FormGroup, FormArray, Validators, FormControl } from '@angular/forms';
import { MatIcon, MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { PartnerService } from '../../../core/services/service-partner/partner-service';
import { IDropdownOption, ISkillExpertise, IServiceOffered, IAttachment } from '../../../core/models/service-partner/service-partner';
import { EGender, EProficiency, ELanguage } from '../../../core/enums/service-partner/service-partner';
import { InputFields } from './input-fields/input-fields';
import { EducationEntry } from './education-entry/education-entry';
import { SkillExpertise } from './skill-expertise/skill-expertise';
import { FileUpload } from './file-upload/file-upload';
import { LanguageEntry } from './language-entry/language-entry';
import { ProfessionalEntry } from './professional-entry/professional-entry';
import { Toaster } from '../../../core/services/toaster/toaster';
import { CustomerHeader } from '../../layouts/customer-layout/customer-header/customer-header';
import { passingYearValidator } from '../../../core/validators/custom-validators';
import { IInputFieldConfig } from '../../../core/models/service-partner/IInputFieldConfig';
import { SERVICE_PARTNER_MESSAGES } from '../../../core/constants/service-partner-messages';
import { AiChatbot } from '../ai-chatbot/ai-chatbot';

@Component({
  selector: 'app-service-partner',
  imports: [
    InputFields,
    MatIcon,
    ReactiveFormsModule,
    CommonModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
    EducationEntry,
    ProfessionalEntry,
    SkillExpertise,
    FileUpload,
    LanguageEntry,
    CustomerHeader, 
    AiChatbot
  ],
  templateUrl: './service-partner.html',
  styleUrl: './service-partner.css',
})
export class ServicePartner implements OnInit {
  toaster = inject(Toaster);

  onboardingForm!: FormGroup;

  isSubmitting = false;
  isLoadingCategories = false;

  serviceTypeOptions: IDropdownOption[] = [];
  categoryOptions: IDropdownOption[] = [];
  subCategoryOptions: IDropdownOption[] = [];

  genderOptions: IDropdownOption[] = [];
  languageOptions: IDropdownOption[] = [];
  proficiencyOptions: IDropdownOption[] = [];

  selectedSkills: ISkillExpertise[] = [];
  selectedServices: IServiceOffered[] = [];
  uploadedAttachments: IAttachment[] = [];

  personalFields: IInputFieldConfig[] = [];
  @ViewChild(SkillExpertise) skillExpertiseRef!: SkillExpertise;
  profileImagePreview: string | null = null;
  profileImageFile: File | null = null;

  constructor(
    private fb: FormBuilder,
    private router: Router,
    private partnerService: PartnerService
  ) { }

  ngOnInit(): void {
    this.initOnboardingForm();
    this.buildStaticDropdownOptions();
    this.loadServiceTypes();
    this.buildPersonalFields();
  }

  private buildPersonalFields(): void {
    this.personalFields = [
      { type: 'text', label: 'Full Name', placeholder: 'Enter full name' },
      { type: 'date', label: 'DOB', placeholder: 'Select date of birth' },
      { type: 'select', label: 'Gender', options: this.genderOptions },
      { type: 'tel', label: 'Mobile Number', placeholder: 'Enter 10-digit mobile', suffixIcon: 'phone' },
      { type: 'email', label: 'Email', placeholder: 'Enter email address', suffixIcon: 'mail_outline' },
      { type: 'select', label: 'Applying For', options: this.serviceTypeOptions },
      { type: 'textarea', label: 'Permanent Address', placeholder: 'Enter permanent address', rows: 3 },
      { type: 'textarea', label: 'Residential Address', placeholder: 'Enter residential address', rows: 3 },
    ];
  }

  controlNames = ['fullName', 'dateOfBirth', 'gender', 'mobileNumber', 'email', 'applyingFor', 'permanentAddress', 'residentialAddress'];
  colClasses = ['col-12 col-md-6', 'col-12 col-md-6', 'col-12 col-md-6', 'col-12 col-md-6', 'col-12 col-md-6', 'col-12 col-md-6', 'col-12', 'col-12'];

  private initOnboardingForm(): void {
    this.onboardingForm = this.fb.group({
      personalDetail: this.buildPersonalDetailGroup(),
      educationInfoList: this.fb.array(
        [this.buildEducationGroup()],
        [Validators.required]
      ),
      professionalInfoList: this.fb.array(
        [this.buildProfessionalGroup()]
      ),
      languageList: this.fb.array(
        [this.buildLanguageGroup()],
        [Validators.required]
      ),
      attachmentList: this.fb.control(null, Validators.required),
      skillList: this.fb.control(null, Validators.required),
      serviceList: this.fb.control(null, Validators.required),
    });
  }

  private buildPersonalDetailGroup(): FormGroup {
    return this.fb.group({
      fullName: ['', [Validators.required]],
      dateOfBirth: ['', [Validators.required]],
      gender: ['', [Validators.required]],
      mobileNumber: ['', [
        Validators.required,
        Validators.pattern(/^[1-9]\d{9}$/)
      ]],
      email: ['', [
        Validators.required,
        Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/)
      ]],
      applyingFor: ['', [Validators.required]],
      permanentAddress: [''],
      residentialAddress: [''],
    });
  }

  private buildEducationGroup(): FormGroup {
    return this.fb.group({
      schoolCollege: ['', [Validators.required]],
      passingYear: ['', [
        Validators.required,
        Validators.pattern(/^(19|20)\d{2}$/),
        passingYearValidator
      ]],
      marks: ['', [
        Validators.required,
        Validators.min(0),
        Validators.max(100)
      ]]
    });
  }

  private buildProfessionalGroup(): FormGroup {
    return this.fb.group({
      companyName: [''],
      role: [''],
      fromDate: [''],
      toDate: [''],
    });
  }

  private buildLanguageGroup(): FormGroup {
    return this.fb.group({
      language: ['', [Validators.required]],
      proficiency: ['', [Validators.required]],
    });
  }

  private buildStaticDropdownOptions(): void {
    this.genderOptions = Object.values(EGender).map((val) => ({ label: val, value: val }));
    this.languageOptions = Object.values(ELanguage).map((val) => ({ label: val, value: val }));
    this.proficiencyOptions = Object.values(EProficiency).map((val) => ({
      label: val,
      value: val,
    }));
  }

  private loadServiceTypes(): void {
    this.partnerService.getServiceTypes().subscribe({
      next: (res) => {
        this.serviceTypeOptions = res.data ?? [];
        const applyingForField = this.personalFields.find(f => f.label === 'Applying For');
        if (applyingForField) {
          applyingForField.options = this.serviceTypeOptions;
        }
      },
      error: () => this.toaster.error(SERVICE_PARTNER_MESSAGES.serviceType.LOAD_FAILED),
    });
  }

  private loadCategoriesByServiceType(serviceTypeId: string): void {
    this.isLoadingCategories = true;
    this.partnerService.getCategoriesByServiceType(serviceTypeId).subscribe({
      next: (res) => {
        this.categoryOptions = [...(res.data ?? [])];
        this.isLoadingCategories = false;
      },
      error: () => {
        this.isLoadingCategories = false;
        this.toaster.error(SERVICE_PARTNER_MESSAGES.category.LOAD_FAILED);
      },
    });
  }

  private loadSubCategoriesByCategory(categoryId: string): void {
    this.partnerService.getSubCategoriesByCategory(categoryId).subscribe({
      next: (res) => {
        this.subCategoryOptions = res.data ?? [];
      },
      error: () => this.toaster.error(SERVICE_PARTNER_MESSAGES.subCategory.LOAD_FAILED),
    });
  }

  get educationFormArray(): FormArray {
    return this.onboardingForm.get('educationInfoList') as FormArray;
  }

  get professionalFormArray(): FormArray {
    return this.onboardingForm.get('professionalInfoList') as FormArray;
  }

  get languageFormArray(): FormArray {
    return this.onboardingForm.get('languageList') as FormArray;
  }

  get personalDetailGroup(): FormGroup {
    return this.onboardingForm.get('personalDetail') as FormGroup;
  }

  getPersonalControl(controlName: string): FormControl {
    return this.personalDetailGroup.get(controlName) as FormControl;
  }

  addEducationEntry(): void {
    this.educationFormArray.push(this.buildEducationGroup());
  }

  addProfessionalEntry(): void {
    this.professionalFormArray.push(this.buildProfessionalGroup());
  }

  addLanguageEntry(): void {
    this.languageFormArray.push(this.buildLanguageGroup());
  }

  onProfileImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (!input.files?.length) return;

    const file = input.files[0];

    if (!file.type.startsWith('image/')) {
      this.toaster.error(SERVICE_PARTNER_MESSAGES.profileImage.INVALID_TYPE);
      return;
    }
    const allowedTypes = ['image/jpeg', 'image/png'];

    if (!allowedTypes.includes(file.type)) {
      this.toaster.error('Only JPG or PNG images are allowed');
      return;
    }
    const allowedExtensions = ['jpg', 'jpeg', 'png'];
    const fileExtension = file.name.split('.').pop()?.toLowerCase();

    if (!fileExtension || !allowedExtensions.includes(fileExtension)) {
      this.toaster.error('Invalid file format. Only JPG/PNG allowed');
      return;
    }


    if (file.size > 5 * 1024 * 1024) {
      this.toaster.error(SERVICE_PARTNER_MESSAGES.profileImage.SIZE_EXCEEDED);
      return;
    }

    this.profileImageFile = file;

    const reader = new FileReader();
    reader.onload = (e) => {
      this.profileImagePreview = e.target?.result as string;
    };
    reader.readAsDataURL(file);
  }

  onServiceTypeChange(selectedValue: string): void {
    if (selectedValue) {
      this.categoryOptions = [];
      this.subCategoryOptions = [];
      this.selectedSkills = [];
      this.selectedServices = [];
      this.skillExpertiseRef?.reset();
      this.loadCategoriesByServiceType(selectedValue);
    }
  }

  onCategorySelected(categoryId: string): void {
    this.loadSubCategoriesByCategory(categoryId);
  }

  onSkillsChanged(skills: ISkillExpertise[]): void {
    this.selectedSkills = skills;
    const control = this.onboardingForm.get('skillList');

    if (skills.length > 0) {
      control?.setValue(true);
      control?.setErrors(null);
    } else {
      control?.setValue(null);
      control?.setErrors({ required: true });

      this.selectedServices = [];
      const serviceControl = this.onboardingForm.get('serviceList');
      serviceControl?.setValue(null);
      serviceControl?.setErrors({ required: true });
    }
  }

  onServicesChanged(services: IServiceOffered[]): void {
    this.selectedServices = services;
    const control = this.onboardingForm.get('serviceList');

    if (this.selectedSkills.length > 0) {
      if (services.length > 0) {
        control?.setValue(true);
        control?.setErrors(null);
      } else {
        control?.setValue(null);
        control?.setErrors({ required: true });
      }
    }
  }

  onFilesChanged(attachments: IAttachment[]): void {
    this.uploadedAttachments = attachments;

    const control = this.onboardingForm.get('attachmentList');

    if (attachments.length > 0) {
      control?.setValue(true);
      control?.setErrors(null);
    } else {
      control?.setValue(null);
      control?.setErrors({ required: true });
    }
  }

  private isFormValid(): boolean {
    this.onboardingForm.markAllAsTouched();
    this.skillExpertiseRef?.markAsTouched();
    if (this.onboardingForm.invalid) return false;

    if (this.educationFormArray.length === 0) {
      this.toaster.error(SERVICE_PARTNER_MESSAGES.education.REQUIRED);
      return false;
    }
    if (!this.skillExpertiseRef?.isValid()) return false;

    if (this.languageFormArray.length === 0) {
      this.toaster.error(SERVICE_PARTNER_MESSAGES.language.REQUIRED);
      return false;
    }

    if (this.uploadedAttachments.length === 0) {
      this.toaster.error(SERVICE_PARTNER_MESSAGES.attachment.REQUIRED);
      return false;
    }

    return true;
  }

  private buildFormData(): FormData {
    const personal = this.personalDetailGroup.value;
    const educationList = this.educationFormArray.value;
    const professionalList = this.professionalFormArray.value;
    const languageList = this.languageFormArray.value;

    const skillExpertiseList: ISkillExpertise[] = this.selectedSkills.map((skill) => ({
      ...skill,
      subCategories: this.selectedServices.map((s) => s.subCategoryId),
    }));

    const form = new FormData();

    form.append(
      'personalDetail',
      JSON.stringify({
        fullName: personal.fullName,
        dateOfBirth: personal.dateOfBirth,
        gender: personal.gender,
        mobileNumber: personal.mobileNumber,
        email: personal.email,
        applyingFor: personal.applyingFor,
        permanentAddress: personal.permanentAddress,
        residentialAddress: personal.residentialAddress,
      })
    );

    if (this.profileImageFile instanceof File) {
      form.append('profileImage', this.profileImageFile, this.profileImageFile.name);
    }

    form.append('educationInfoList', JSON.stringify(educationList));
    form.append(
      'professionalInfoList',
      JSON.stringify(professionalList.filter((p: any) => p.companyName || p.role))
    );
    form.append('skillExpertiseList', JSON.stringify(skillExpertiseList));
    form.append('languageList', JSON.stringify(languageList));

    this.uploadedAttachments.forEach((attachment: IAttachment) => {
      form.append('attachmentFiles', attachment.file, attachment.fileName);
    });

    return form;
  }

  onApplyClick(): void {
    if (!this.isFormValid()) return;

    this.isSubmitting = true;
    const form = this.buildFormData();

    this.partnerService.submitApplication(form).subscribe({
      next: (res: any) => {
        this.isSubmitting = false;
        if (res.success) {
          this.toaster.success(res.message);
          this.router.navigate([SERVICE_PARTNER_MESSAGES.submit.SUCCESS_NAVIGATE]);
        } else {
          this.toaster.error(res.message);
        }
      },
      error: (err: any) => {
        this.isSubmitting = false;
        this.toaster.error(err?.error?.message ?? SERVICE_PARTNER_MESSAGES.submit.FAILED);
      },
    });
  }

  onCancelClick(): void {
    this.onboardingForm.reset();
    this.selectedSkills = [];
    this.selectedServices = [];
    this.uploadedAttachments = [];
    this.profileImageFile = null;
    this.profileImagePreview = null;
    this.categoryOptions = [];
    this.subCategoryOptions = [];
    this.skillExpertiseRef?.reset();
    this.router.navigate(['/']);
  }
}