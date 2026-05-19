import { Component, Input, OnChanges } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormArray, FormGroup } from '@angular/forms';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { InputFields } from '../input-fields/input-fields';
import { AsFormControlpipePipe } from '../../../../shared/pipes/as-form-controlpipe-pipe';
import { IInputFieldConfig } from '../../../../core/models/service-partner/IinputFiledConfig';
import { IDropdownOption } from '../../../../core/models/service-partner/dropdown-option';


@Component({
  selector: 'app-language-entry',
  imports: [
    InputFields,
    CommonModule,
    ReactiveFormsModule,
    MatIconModule,
    MatButtonModule,
    AsFormControlpipePipe,
  ],
  templateUrl: './language-entry.html',
  styleUrl: './language-entry.css',
})
export class LanguageEntry implements OnChanges {
  @Input() languageFormArray!: FormArray;
  @Input() languageOptions: IDropdownOption[] = [];
  @Input() proficiencyOptions: IDropdownOption[] = [];

  languageFields: IInputFieldConfig[] = [];
  controlNames = ['language', 'proficiency'];

  ngOnChanges(): void {
    this.languageFields = [
      { type: 'select', label: 'Language', options: this.languageOptions },
      { type: 'select', label: 'Proficiency', options: this.proficiencyOptions },
    ];
  }

  getLanguageGroup(index: number): FormGroup {
    return this.languageFormArray.at(index) as FormGroup;
  }
}