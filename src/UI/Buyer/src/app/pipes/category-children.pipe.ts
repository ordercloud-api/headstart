import { Pipe, PipeTransform } from '@angular/core'
import { Category } from 'ordercloud-javascript-sdk'

@Pipe({
  name: 'childCategoryFilter',
  pure: false,
})
export class ChildCategoryPipe implements PipeTransform {
  transform(categories: Category[], parentId: any): any {
    return categories.filter((category) => category.ParentID === parentId)
  }
}
