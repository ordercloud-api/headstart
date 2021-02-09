import { Component, Input } from '@angular/core'
import { faAngleLeft, faAngleRight } from '@fortawesome/free-solid-svg-icons'
import { HSMeProduct } from '@ordercloud/headstart-sdk'

@Component({
  templateUrl: './product-carousel.component.html',
  styleUrls: ['./product-carousel.component.scss'],
})
export class OCMProductCarousel {
  @Input() products: HSMeProduct[] = []
  @Input() displayTitle: string

  index = 0
  rowLength = 4
  faAngleLeft = faAngleLeft
  faAngleRight = faAngleRight

  left(): void {
    this.index -= this.rowLength
  }

  right(): void {
    this.index += this.rowLength
  }

  getProducts(): HSMeProduct[] {
    return this.products.slice(this.index, this.index + this.rowLength)
  }
}
