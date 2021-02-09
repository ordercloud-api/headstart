import { Pipe, PipeTransform } from '@angular/core'
import { SafeHtml, DomSanitizer } from '@angular/platform-browser'

@Pipe({
  name: 'safeHTML',
  pure: false,
})
export class SafeHTMLPipe implements PipeTransform {
  constructor(protected _sanitizer: DomSanitizer) {}
  transform(string: string): SafeHtml {
    return this._sanitizer.bypassSecurityTrustHtml(string)
  }
}
