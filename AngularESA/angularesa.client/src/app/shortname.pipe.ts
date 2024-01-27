import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'shortname'
})
export class ShortnamePipe implements PipeTransform {

  transform(value: string): string {
    let fullName: string = value.trim();

    if (fullName.length == 0)
      return '';

    let firstName = fullName.split(' ')[0];
    if (fullName.split(' ').length <= 1)
      return firstName;

    let lastName = fullName.split(' ').splice(-1);

    return firstName + ' ' + lastName;
  }
}
