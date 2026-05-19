import { Component, OnInit, OnDestroy } from '@angular/core';
import { GradientOverlay } from "../../../shared/components/gradient-overlay/gradient-overlay";
import { AbstractControl, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ContactUsService } from '../../../core/services/contact-us/contact-us';
import { IContact } from '../../../core/models/contact-us/IContact';
import { Toaster } from '../../../core/services/toaster/toaster';
import { MatFormField, MatFormFieldModule } from "@angular/material/form-field";
import { MatInputModule } from '@angular/material/input';
import { MatDialogModule } from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { CONTACT_US_MESSAGES } from '../../../core/constants/contact-us-message';

@Component({
  selector: 'app-contact-us',
  imports: [GradientOverlay, ReactiveFormsModule, CommonModule, MatInputModule, MatDialogModule, MatFormFieldModule],
  templateUrl: './contact-us.html',
  styleUrl: './contact-us.css',
})
export class ContactUs {

  private layoutEl: HTMLElement | null = null;

  constructor(private contactService: ContactUsService, private toaster: Toaster) { }

  ngOnInit(): void {
    this.layoutEl = document.querySelector('.layout-content');
    if (this.layoutEl) {
      this.layoutEl.classList.remove('p-5');
      this.layoutEl.classList.add('p-2', 'py-3', 'py-sm-5', 'p-lg-5');
    }
  }

  ngOnDestroy(): void {
    if (this.layoutEl) {
      this.layoutEl.classList.remove('p-2', 'py-3', 'py-sm-5', 'p-lg-5');
      this.layoutEl.classList.add('p-5');
    }
  }

  contactForm = new FormGroup({
    firstName: new FormControl('', [
      Validators.required,
      Validators.pattern(/^[a-zA-Z\s]+$/)
    ]),

    lastName: new FormControl('', [
      Validators.required,
      Validators.pattern(/^[a-zA-Z\s]+$/)
    ]),

    phoneNumber: new FormControl('', [
      Validators.required,
      Validators.pattern(/^[1-9]\d{9}$/)
    ]),

    email: new FormControl('', [
      Validators.required,
      Validators.pattern(/^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$/)
    ]),

    description: new FormControl('', [
      Validators.required,
      Validators.maxLength(300)
    ])
  });
  firstName = this.contactForm.get('firstName');
  lastName = this.contactForm.get('lastName');
  phoneNumber = this.contactForm.get('phoneNumber');
  email = this.contactForm.get('email');
  description = this.contactForm.get('description');

  get firstNameControl(): FormControl {
    return this.contactForm.get('firstName') as FormControl;
  }

  get lastNameControl(): FormControl {
    return this.contactForm.get('lastName') as FormControl;
  }

  get phoneControl(): FormControl {
    return this.contactForm.get('phoneNumber') as FormControl;
  }

  get emailControl(): FormControl {
    return this.contactForm.get('email') as FormControl;
  }

  get descriptionControl(): FormControl {
    return this.contactForm.get('description') as FormControl;
  }

  private trimFormValues() {
    Object.keys(this.contactForm.controls).forEach(key => {
      const control = this.contactForm.get(key);
      if (control?.value && typeof control.value === 'string') {
        control.setValue(control.value.trim());
      }
    });
  }

  onSubmit() {
    this.trimFormValues();
    if (this.contactForm.invalid) {
      this.contactForm.markAllAsTouched();
      return;
    }
    const contactObj: IContact = {
      FirstName: this.firstNameControl.value,
      LastName: this.lastNameControl.value,
      Mobile: this.phoneControl.value,
      Email: this.emailControl.value,
      Description: this.descriptionControl.value
    };

    this.contactService.addContact(contactObj).subscribe({
      next: () => {
        this.toaster.success(CONTACT_US_MESSAGES.success.SUBMITTED)
        this.resetForm();
      },
      error: (err) => {
        this.toaster.error(err.message);
      }
    });
  }

  private resetForm(): void {
    this.contactForm.reset();
    this.contactForm.markAsPristine();
    this.contactForm.markAsUntouched();
  }

  getErrorMessage(control: AbstractControl, fieldName: string): string {
    if (!control || !control.errors) return '';

    if (control.hasError('required')) return CONTACT_US_MESSAGES.error.REQUIRED(fieldName);

    if (control.hasError('email')) return CONTACT_US_MESSAGES.error.INVALID_EMAIL;

    if (control.hasError('pattern')) {
      if (control === this.phoneControl) return CONTACT_US_MESSAGES.error.INVALID_PHONE;
      if (control === this.firstNameControl || control === this.lastNameControl)
        return CONTACT_US_MESSAGES.error.INVALID_NAME(fieldName);
      return CONTACT_US_MESSAGES.error.GENERIC(fieldName);
    }

    if (control.hasError('maxlength')) return CONTACT_US_MESSAGES.error.MAX_LENGTH(fieldName);

    return CONTACT_US_MESSAGES.error.GENERIC(fieldName);
  }

}
