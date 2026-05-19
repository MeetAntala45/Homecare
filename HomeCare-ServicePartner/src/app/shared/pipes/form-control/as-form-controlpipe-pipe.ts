import { Pipe, PipeTransform } from '@angular/core';
import { AbstractControl, FormControl } from '@angular/forms';
@Pipe({
  name: 'asFormControlpipe',
  standalone: true 
})
export class AsFormControlpipePipe implements PipeTransform {

  transform(control: AbstractControl | null): FormControl {
    return control as FormControl;
  }

}
