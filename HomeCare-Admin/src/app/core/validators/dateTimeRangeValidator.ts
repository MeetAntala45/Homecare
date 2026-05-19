import { AbstractControl, ValidationErrors, ValidatorFn } from '@angular/forms';

export const dateTimeRangeValidator: ValidatorFn = (group: AbstractControl): ValidationErrors | null => {
  const valueFrom = group.get('valueFrom')?.value;
  const valueTo   = group.get('valueTo')?.value;

  if (!valueFrom || !valueTo) return null;

  if (typeof valueFrom === 'string' && valueFrom.includes('|')) {
    const fromMinutes = convertToMinutes(valueFrom);
    const toMinutes   = convertToMinutes(valueTo);

    if (fromMinutes >= toMinutes)
      return { invalidTimeRange: true };

    return null;
  }

  const from = valueFrom instanceof Date ? valueFrom : new Date(valueFrom);
  const to   = valueTo   instanceof Date ? valueTo   : new Date(valueTo);

  if (isNaN(from.getTime()) || isNaN(to.getTime())) return null;

  if (from > to)
    return { invalidDateRange: true };

  return null;
};

function convertToMinutes(val: string): number {
  const [time, period] = val.split('|');
  let [h, m] = time.split(':').map(Number);
  if (period === 'PM' && h < 12) h += 12;
  if (period === 'AM' && h === 12) h = 0;
  return h * 60 + m;
}