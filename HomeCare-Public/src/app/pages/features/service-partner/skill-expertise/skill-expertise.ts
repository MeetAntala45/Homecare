import { Component, Input, Output, EventEmitter, OnChanges, SimpleChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormControl, Validators } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatSelectModule } from '@angular/material/select';
import { MatInputModule } from '@angular/material/input';
import {
  IDropdownOption,
  ISkillExpertise,
  IServiceOffered,
} from '../../../../core/models/service-partner/service-partner';
import { InputFields } from '../input-fields/input-fields';
import { IInputFieldConfig } from '../../../../core/models/service-partner/IInputFieldConfig';

@Component({
  selector: 'app-skill-expertise',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    MatInputModule,
    InputFields,
  ],
  templateUrl: './skill-expertise.html',
  styleUrl: './skill-expertise.css',
})
export class SkillExpertise implements OnChanges {
  @Input() subCategoryOptions: IDropdownOption[] = [];
  @Input() categoryOptions: IDropdownOption[] = [];
  @Output() categorySelected = new EventEmitter<string>();
  @Output() skillsChanged = new EventEmitter<ISkillExpertise[]>();
  @Output() servicesChanged = new EventEmitter<IServiceOffered[]>();

  categoryControl = new FormControl('');
  selectedCategories: ISkillExpertise[] = [];

  selectedServices: IServiceOffered[] = [];
  categorySubCategoryMap: Record<string, string[]> = {};
  allSubCategoryOptions: IDropdownOption[] = [];
  currentCategoryId: string = '';
  categoryTouched = false;
  servicesTouched = false;
  categoryFieldConfig: IInputFieldConfig = {};

  get categoryError(): boolean {
    return this.categoryTouched && this.selectedCategories.length === 0;
  }

  get servicesError(): boolean {
    return this.servicesTouched && this.selectedServices.length === 0;
  }

  markAsTouched(): void {
    this.categoryTouched = true;
    this.servicesTouched = true;
  }

  isValid(): boolean {
    return this.selectedCategories.length > 0 && this.selectedServices.length > 0;
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes['categoryOptions']) {
      this.categoryFieldConfig = {
        type: 'select',
        label: 'Select Category',
        placeholder: 'Select category',
        options: [...this.categoryOptions],
      };
    }

    if (changes['subCategoryOptions'] && this.currentCategoryId && this.subCategoryOptions.length > 0) {
      const newOptions = this.subCategoryOptions.filter(
        (opt) => !this.allSubCategoryOptions.some((existing) => existing.value === opt.value)
      );
      this.allSubCategoryOptions = [...this.allSubCategoryOptions, ...newOptions];
      this.categorySubCategoryMap[this.currentCategoryId] = this.subCategoryOptions.map(
        (o) => o.value
      );
    }
  }

  reset(): void {
    this.selectedCategories = [];
    this.selectedServices = [];
    this.allSubCategoryOptions = [];
    this.categorySubCategoryMap = {};
    this.currentCategoryId = '';
    this.categoryControl.setValue('', { emitEvent: false });
    this.categoryTouched = false;
    this.servicesTouched = false;
  }

  onCategorySelected(value: string): void {
    const option = this.categoryOptions.find((o) => o.value === value);
    if (!option) return;

    const alreadySelected = this.selectedCategories.some((s) => s.categoryId === value);
    if (!alreadySelected) {
      this.selectedCategories = [
        ...this.selectedCategories,
        { categoryId: option.value, categoryName: option.label, subCategories: [] },
      ];
      this.skillsChanged.emit(this.selectedCategories);
    }

    this.currentCategoryId = value;
    this.categorySelected.emit(value);
    this.categoryControl.setValue('', { emitEvent: false });

    this.categoryTouched = true;
  }

  removeCategory(categoryId: string): void {
    this.selectedCategories = this.selectedCategories.filter((s) => s.categoryId !== categoryId);

    const ownedSubIds = this.categorySubCategoryMap[categoryId] ?? [];
    this.allSubCategoryOptions = this.allSubCategoryOptions.filter(
      (o) => !ownedSubIds.includes(o.value)
    );
    this.selectedServices = this.selectedServices.filter(
      (s) => !ownedSubIds.includes(s.subCategoryId)
    );

    delete this.categorySubCategoryMap[categoryId];

    this.categoryTouched = true;

    this.skillsChanged.emit(this.selectedCategories);
    this.servicesChanged.emit(this.selectedServices);
  }

  onServiceChipClick(option: IDropdownOption): void {
    this.servicesTouched = true;

    const alreadySelected = this.selectedServices.some((s) => s.subCategoryId === option.value);
    if (alreadySelected) {
      this.selectedServices = this.selectedServices.filter((s) => s.subCategoryId !== option.value);
    } else {
      this.selectedServices = [
        ...this.selectedServices,
        { subCategoryId: option.value, subCategoryName: option.label },
      ];
    }
    this.servicesChanged.emit(this.selectedServices);
  }

  isServiceSelected(optionValue: string): boolean {
    return this.selectedServices.some((s) => s.subCategoryId === optionValue);
  }
}