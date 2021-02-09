import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import {
  OrchestrationLog,
  ListPage,
  HeadStartSDK,
} from '@ordercloud/headstart-sdk'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { Buyers } from 'ordercloud-javascript-sdk'

// TODO - this service is only relevent if you're already on the product details page. How can we enforce/inidcate that?
@Injectable({
  providedIn: 'root',
})
export class OrchestrationLogsService extends ResourceCrudService<OrchestrationLog> {
  primaryResourceLevel = 'logs'

  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    currentUserService: CurrentUserService
  ) {
    super(
      router,
      activatedRoute,
      Buyers,
      currentUserService,
      '/reports/logs',
      'Logs'
    )
  }

  async list(args: any[]): Promise<ListPage<OrchestrationLog>> {
    const listArgs = args[0]
    listArgs.sortBy = listArgs.sortBy || '!timeStamp'
    return await HeadStartSDK.OrchestrationLogs.List(listArgs)
  }
}
