import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'countFormat'
})
export class CountFormatPipe implements PipeTransform {

  transform(count: number): string {
    if (count >= 1_000_000) return Math.floor(count / 1_000_000) + 'M+';
    if (count >= 1000) return Math.floor(count / 1000) + 'k+';
    if (count >= 50) return Math.floor(count / 50) * 50 + '+';
    return count.toString();
  }

}
