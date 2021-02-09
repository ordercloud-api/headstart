import { Component, Input, ViewEncapsulation } from '@angular/core'
import { ListPage } from '@ordercloud/angular-sdk'

@Component({
  selector: 'resource-list-meta',
  templateUrl: './resource-list-meta.component.html',
  styleUrls: ['./resource-list-meta.component.scss'],
  encapsulation: ViewEncapsulation.None,
})
export class ResourceListMeta {
  _selectedResourceID: string
  _listPage: ListPage<any>
  @Input()
  set selectedResourceID(id: string) {
    this._selectedResourceID = id
  }
  @Input()
  set listPage(listPage: ListPage<any>) {
    this._listPage = listPage
  }

  getResultsDisplayText(ListPage: ListPage<any>): string {
    if (ListPage.Meta.ItemRange) {
      return `1-${ListPage.Meta.ItemRange[1]} of ${ListPage.Meta.TotalCount} results`
    }
    return ''
  }
}
