import {
  Component,
  Input,
  OnChanges,
  OnInit,
  SimpleChanges,
} from '@angular/core'

@Component({
  templateUrl: './loading-layout.component.html',
  styleUrls: ['./loading-layout.component.scss'],
})
export class OCMLoadingLayout implements OnInit, OnChanges {
  @Input() isLoading?: boolean
  @Input() resource: any // which resource you're basing loading off of (e.g. suppliers, products, etc)
  @Input() height: string // height of individual loading block
  @Input() width: string // width of individual loading block
  @Input() mb: string // bottom margin of individual loading block
  @Input() columns: string // number of columns for loading layout
  @Input() rows: string // number of rows to display in loading layout
  columnsToRender: Array<any> = [] // used to loop over in html to generate x number of rows
  divsToRender: Array<any> = [] // used to loop over in html to generate x number of divs (rows)
  bootstrapColumns: number // used to calculate the proper html class for bootstrap columns
  showLoadingIndicator: boolean

  ngOnInit(): void {
    this.columnsToRender = new Array(this.columns)
    this.divsToRender = new Array(this.rows)
    this.bootstrapColumns = Math.round(12 / this.columnsToRender.length)
    this.showLoadingIndicator = this.shouldShowLoadingIndicator()
  }

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.resource || changes.isLoading) {
      this.showLoadingIndicator = this.shouldShowLoadingIndicator()
    }
  }

  shouldShowLoadingIndicator(): boolean {
    return !this.resource || this.isLoading
  }
}
