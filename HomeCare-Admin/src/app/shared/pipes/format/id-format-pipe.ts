import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'idFormat',
  standalone: true
})
export class IdFormatPipe implements PipeTransform {

  transform(value: number): string {

    if (value == null) return '';

    return value.toString().padStart(3, '0');
  }

}
