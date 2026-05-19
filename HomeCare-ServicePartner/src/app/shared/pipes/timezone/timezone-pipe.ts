import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ 
  name: 'Timezone',
  standalone: true 
})

export class TimezonePipe implements PipeTransform {
  transform(value: string): Date {
    return new Date(value + 'Z');
  }
}