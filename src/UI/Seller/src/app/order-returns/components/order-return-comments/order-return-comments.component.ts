import { Component, Input } from '@angular/core'
import { OrderReturnApproval } from 'ordercloud-javascript-sdk'

@Component({
  selector: 'order-return-comments',
  templateUrl: './order-return-comments.component.html',
  styleUrls: ['./order-return-comments.component.scss'],
})
export class OrderReturnCommentsComponent {
  @Input() orderReturnApproval: OrderReturnApproval
}
