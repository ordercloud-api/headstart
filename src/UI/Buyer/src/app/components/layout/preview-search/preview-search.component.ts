import {
  Component,
  Input,
  OnInit,
} from '@angular/core'
import { ReflektionSearchResponse } from 'src/app/services/reflektion/models/ReflektionSearchResponse'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service';

@Component({
  templateUrl: './preview-search.component.html',
  styleUrls: ['./preview-search.component.scss'],
})
export class OCMPreviewSearch implements OnInit {
  @Input() searchTerm?: string;
  @Input() results?: ReflektionSearchResponse;

  constructor(
    public context: ShopperContextService,
  ) {
  }

  ngOnInit(): void {
  }

  toProductDetails(productID: string): void {
    this.context.router.toProductDetails(productID);
  }

}
