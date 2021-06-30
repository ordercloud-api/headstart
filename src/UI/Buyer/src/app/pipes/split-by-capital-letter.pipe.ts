import { Pipe, PipeTransform } from '@angular/core'

@Pipe({
  name: 'splitByCapitalLetter',
})
export class SplitByCapitalLetterPipe implements PipeTransform {
  transform(value: string): string {
    return value ? `${value.match(/[A-Z][a-z]+|[0-9]+/g).join(' ')}` : null
  }
}
