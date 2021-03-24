import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { User } from '@ordercloud/angular-sdk'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { AdminUsers } from 'ordercloud-javascript-sdk'
import { ToastrService } from 'ngx-toastr'
import OrderCloudError from 'ordercloud-javascript-sdk/dist/utils/OrderCloudError'

// TODO - this service is only relevent if you're already on the supplier details page. How can we enforce/inidcate that?
@Injectable({
  providedIn: 'root',
})
export class SellerUserService extends ResourceCrudService<User> {
  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    currentUserService: CurrentUserService,
    private toastrService: ToastrService
  ) {
    super(
      router,
      activatedRoute,
      AdminUsers,
      currentUserService,
      '/seller-users',
      'users'
    )
  }

  emptyResource = {
    Username: '',
    FirstName: '',
    LastName: '',
    Email: '',
    Phone: '',
  }

  async createNewResource(resource: any): Promise<any> {
    try {
      const args = await this.createListArgs([resource])
      const newResource = await this.ocService.Create(...args)
      this.resourceSubject.value.Items = [
        ...this.resourceSubject.value.Items,
        newResource,
      ]
      this.resourceSubject.next(this.resourceSubject.value)
      return newResource
    } catch (err) {
      this.toastrService.error(
        err?.errors?.Errors?.[0]?.Message,
        (err as OrderCloudError)?.statusText
      )
      return
    }
  }
}
