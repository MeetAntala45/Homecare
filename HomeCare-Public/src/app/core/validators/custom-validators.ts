
import { AbstractControl, ValidationErrors } from '@angular/forms';

export function passingYearValidator(control: AbstractControl): ValidationErrors | null {
  const value = control.value;
  if (!value) return null;
  const year = Number(value);
  const currentYear = new Date().getFullYear();
  if (year >= currentYear) return { futureYear: true };
  return null;
}

