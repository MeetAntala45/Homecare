import { Component, Input } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormArray, FormGroup, FormControl } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { MatTooltipModule } from '@angular/material/tooltip';
import { InputFields } from '../input-fields/input-fields';
import { IInputFieldConfig } from '../../../../core/models/service-partner/IInputFieldConfig';

@Component({
  selector: 'app-education-entry',
  imports: [
    CommonModule,
    InputFields,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    MatTooltipModule,
  ]
  ,
  templateUrl: './education-entry.html',
  styleUrl: './education-entry.css',
})
export class EducationEntry {
  @Input() educationFormArray!: FormArray;

  educationFields: IInputFieldConfig[] = [
    { type: 'text', label: 'School / College', placeholder: 'Enter school or college name' },
    { type: 'text', label: 'Passing Year', placeholder: 'YYYY', requiredMessage: 'Year is required' },
    { type: 'number', label: 'Marks', placeholder: '0', requiredMessage: 'Marks are required', suffixText: '%' },
  ];

  controlNames = ['schoolCollege', 'passingYear', 'marks'];

  getEducationGroup(index: number): FormGroup {
    return this.educationFormArray.at(index) as FormGroup;
  }


  getControl(group: FormGroup, controlName: string): FormControl {
    return group.get(controlName) as FormControl;
  }
}