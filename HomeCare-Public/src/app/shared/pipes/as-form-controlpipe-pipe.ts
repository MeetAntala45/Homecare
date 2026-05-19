import { Pipe, PipeTransform } from '@angular/core';
import { AbstractControl, FormControl } from '@angular/forms';
@Pipe({
  name: 'asFormControlpipe'
})
export class AsFormControlpipePipe implements PipeTransform {

  transform(control: AbstractControl | null): FormControl {
    return control as FormControl;
  }

}
