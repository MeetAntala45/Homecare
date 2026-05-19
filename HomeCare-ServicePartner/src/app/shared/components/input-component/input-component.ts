import { CommonModule } from '@angular/common';
import { Component, Input, signal } from '@angular/core';
import { ReactiveFormsModule, FormControl } from '@angular/forms';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatIconModule } from '@angular/material/icon';
import { MatInputModule } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-input-component',
  standalone: true,
  imports: [
    MatInputModule,
    MatIconModule,
    MatFormFieldModule,
    MatSelectModule,
    CommonModule,
    ReactiveFormsModule
  ],
  templateUrl: './input-component.html',
  styleUrl: './input-component.css',
})
export class InputComponent {

  @Input() label: string = '';
  @Input() type: string = 'text';
  @Input() icon?: string = '';
  @Input() control!: FormControl;
  @Input() options: { label: string; value: any }[] = [];
  @Input() maxLength?: number;

  hide = signal(true);

  togglePassword(event: MouseEvent) {
    event.preventDefault();
    event.stopPropagation();
    this.hide.update(value => !value);
  }

  get inputType(): string {
    if (this.type === 'password') {
      return this.hide() ? 'password' : 'text';
    }
    return this.type;
  }

  limitLength(event: any) {
    if (!this.maxLength) return;
    let value = event.target.value;
    if (value.length > this.maxLength) {
      value = value.substring(0, this.maxLength);
      event.target.value = value;
      this.control.setValue(value);
    }
  }
}