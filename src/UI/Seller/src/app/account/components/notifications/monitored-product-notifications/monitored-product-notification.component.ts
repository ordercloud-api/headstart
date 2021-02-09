import { MiddlewareAPIService } from '@app-seller/shared/services/middleware-api/middleware-api.service'
import { Component, EventEmitter, Input, Output } from '@angular/core'

import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import {
  MonitoredProductFieldModifiedNotificationDocument,
  NotificationStatus,
} from '@app-seller/models/notification.types'
@Component({
  selector: 'monitored-product-notification',
  templateUrl: './monitored-product-notification.component.html',
  styleUrls: ['./monitored-product-notification.component.scss'],
})
export class MonitoredProductNotificationComponent {
  @Input() notification: MonitoredProductFieldModifiedNotificationDocument
  @Output() onActionTaken = new EventEmitter<string>()
  @Output() goToProductPage = new EventEmitter<string>()
  hasPriceBreak = false

  constructor(
    private currentUserService: CurrentUserService,
    private middleware: MiddlewareAPIService
  ) {}

  async reviewMonitoredFieldChange(
    status: NotificationStatus,
    notification: MonitoredProductFieldModifiedNotificationDocument
  ): Promise<void> {
    const myContext = await this.currentUserService.getUserContext()
    notification.Doc.Status = status
    notification.Doc.History.ReviewedBy = {
      ID: myContext?.Me?.ID,
      Name: `${myContext?.Me?.FirstName} ${myContext?.Me?.LastName}`,
    }
    notification.Doc.History.DateReviewed = new Date().toISOString()

    const superProduct = await this.middleware.updateProductNotifications(
      notification
    )

    this.onActionTaken.emit('ACCEPTED')
  }
  navigateToProductPage(productId: string) {
    this.goToProductPage.emit(productId)
  }
}
