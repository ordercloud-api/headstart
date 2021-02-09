import { Component, OnInit } from '@angular/core'
import { MeUser } from 'ordercloud-javascript-sdk'
import { ToastrService } from 'ngx-toastr'
import {
  faEdit,
  faUser,
  faPhone,
  faEnvelope,
} from '@fortawesome/free-solid-svg-icons'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
@Component({
  templateUrl: './profile.component.html',
  styleUrls: ['./profile.component.scss'],
})
export class OCMProfile implements OnInit {
  me: MeUser
  alive = true
  faEdit = faEdit
  faUser = faUser
  faPhone = faPhone
  faEnvelope = faEnvelope
  showEditProfileForm = false

  constructor(
    private toasterService: ToastrService,
    private context: ShopperContextService
  ) {}

  ngOnInit(): void {
    this.context.currentUser.onChange(this.handleUserChange)
  }

  showEditProfile(): void {
    this.showEditProfileForm = true
  }

  dismissProfileEditForm(): void {
    this.showEditProfileForm = false
  }

  handleUserChange = (user: MeUser): void => {
    if (user) {
      this.me = user
    }
  }

  async profileFormSubmitted(me: MeUser): Promise<void> {
    this.showEditProfileForm = false
    await this.context.currentUser.patch(me)
    this.toasterService.success('Profile Updated')
  }
}
