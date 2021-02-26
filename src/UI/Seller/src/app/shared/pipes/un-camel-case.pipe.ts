import { Pipe, PipeTransform } from '@angular/core'

@Pipe({
  name: 'uncamel',
})

//  transforms camel or pascal to words with spaces
export class UnCamelPipe implements PipeTransform {
    transform(camelStr: string): string {
        return camelStr
        .replace(/([A-Z]+)([A-Z][a-z])/g, ' $1 $2')
        // Look for lower-case letters followed by upper-case letters
        .replace(/([a-z\d])([A-Z])/g, '$1 $2')
        // Look for lower-case letters followed by numbers
        .replace(/([a-zA-Z])(\d)/g, '$1 $2')
        .replace(/^./, function(str){ return str.toUpperCase(); })
        // Remove any white space left around the word
        .trim();
    }
}