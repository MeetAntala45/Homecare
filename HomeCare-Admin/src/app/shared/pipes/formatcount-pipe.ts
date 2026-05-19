import { Pipe, PipeTransform } from '@angular/core';

@Pipe({ name: 'formatCount', standalone: true })
export class FormatCountPipe implements PipeTransform {
  transform(value: number): string {
    if (value >= 1_000_000) return (value / 1_000_000).toFixed(1).replace(/\.0$/, '') + 'M Times';
    if (value >= 1000) return (value / 1000).toFixed(1).replace(/\.0$/, '') + 'k Times';
    return value + ' Times';
  }
}