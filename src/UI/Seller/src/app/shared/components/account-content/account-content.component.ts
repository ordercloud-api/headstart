import { Router, ActivatedRoute } from '@angular/router'
import {
  AfterViewChecked,
  ChangeDetectorRef,
  OnInit,
  Inject,
} from '@angular/core'
import { getPsHeight } from '@app-seller/shared/services/dom.helper'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { environment } from 'src/environments/environment.local'
import { ListPage, MeUser } from '@ordercloud/angular-sdk'
import { FormGroup, FormControl } from '@angular/forms'
import { isEqual as _isEqual, set as _set, get as _get } from 'lodash'
import { HeadStartSDK, Asset, AssetUpload } from '@ordercloud/headstart-sdk'
import { JDocument } from '@ordercloud/cms-sdk'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { NotificationStatus } from '@app-seller/models/notification.types'
import { ContentManagementClient } from '@ordercloud/cms-sdk'
import { UserContext } from '@app-seller/models/user.types'
import { AppConfig } from '@app-seller/models/environment.types'

export abstract class AccountContent implements AfterViewChecked, OnInit {
  activePage: string
  currentUserInitials: string
  hasProfileImg = false
  contentHeight: number
  userContext: UserContext
  myProfileImg: string
  profileImgLoading = false
  organizationName: string
  areChanges: boolean
  user: MeUser
  userForm: FormGroup
  userStatic: MeUser
  userEditable: MeUser
  notificationsToReview: JDocument[]

  constructor(
    private router: Router,
    activatedRoute: ActivatedRoute,
    private changeDetectorRef: ChangeDetectorRef,
    private currentUserService: CurrentUserService,
    @Inject(applicationConfiguration) private appConfig: AppConfig,
    private appAuthService: AppAuthService
  ) {
    this.setUpSubs()
  }

  ngAfterViewChecked(): void {
    this.contentHeight = getPsHeight(/* No additional class to pass */)
    this.changeDetectorRef.detectChanges()
  }

  async ngOnInit(): Promise<void> {
    await this.setUser()
    this.userContext.Me.Supplier
      ? this.getSupplierOrg()
      : (this.organizationName = this.appConfig.sellerName)
    this.refresh(this.userContext.Me)
    this.setProfileImgSrc()
    if (this.userContext?.UserType === 'SELLER') {
      this.retrieveNotifications()
    }
  }

  setUpSubs(): void {
    this.currentUserService.userSubject.subscribe((user) => {
      this.user = user
      this.setCurrentUserInitials(this.user)
    })
    this.currentUserService.profileImgSubject.subscribe((img) => {
      this.hasProfileImg = Object.keys(img).length > 0
    })
  }

  async retrieveNotifications(): Promise<void> {
    try {
      await ContentManagementClient.Documents.List(
        'MonitoredProductFieldModifiedNotification',
        {
          pageSize: 100,
          sortBy: ['!History.DateUpdated'],
        }
      ).then((results: ListPage<JDocument>) => {
        if (results?.Items?.length > 0) {
          this.notificationsToReview = results?.Items.filter(
            (i: { Doc: { Status: NotificationStatus } }) =>
              i?.Doc?.Status === NotificationStatus.SUBMITTED
          )
        }
      })
    } catch (error) {
      console.log(`No documents are found`)
    }
  }

  setCurrentUserInitials(user: MeUser): void {
    const firstFirst = user?.FirstName?.substr(0, 1)
    const firstLast = user?.LastName?.substr(0, 1)
    this.currentUserInitials = `${firstFirst}${firstLast}`
  }

  async getSupplierOrg(): Promise<void> {
    const mySupplier = await this.currentUserService.getMySupplier()
    this.organizationName = mySupplier.Name
  }

  async setUser(): Promise<void> {
    this.userContext = await this.currentUserService.getUserContext()
  }

  refresh(user: MeUser): void {
    this.userStatic = this.copyResource(user)
    this.userEditable = this.copyResource(user)
    this.createUserForm(user)
    this.areChanges = this.checkIfAreChanges(this.userStatic, this.userEditable)
  }

  navigateToPage(route: string) {
    this.router.navigateByUrl(route)
  }

  createUserForm(me: MeUser): void {
    this.userForm = new FormGroup({
      FirstName: new FormControl(me.FirstName),
      LastName: new FormControl(me.LastName),
      Email: new FormControl({ value: me.Email, disabled: true }),
      Username: new FormControl(me.Username),
      RequestInfoEmails: new FormControl(_get(me, 'xp.RequestInfoEmails')),
      OrderEmails: new FormControl(_get(me, 'xp.OrderEmails')),
      ProductEmails: new FormControl(_get(me, 'xp.ProductEmails')),
      AddtlRcpts: new FormControl(_get(me, 'xp.AddtlRcpts')),
    })
  }

  checkIfAreChanges = <T>(_static: T, _editable: T): boolean =>
    !_isEqual(_editable, _static)

  updateUserFromEvent(event: any, field: string): void {
    const value = event.target.value
    const userUpdate = { field, value }
    const updateUserCopy: MeUser = JSON.parse(JSON.stringify(this.userEditable))
    this.userEditable = _set(updateUserCopy, userUpdate.field, userUpdate.value)
    this.areChanges = this.checkIfAreChanges(this.userStatic, this.userEditable)
  }

  copyResource<T>(resource: T): T {
    return JSON.parse(JSON.stringify(resource))
  }

  discardChanges(): void {
    this.refresh(this.userStatic)
    this.areChanges = this.checkIfAreChanges(this.userStatic, this.userEditable)
  }

  async patchUser(fieldsToPatch: string[]): Promise<void> {
    const patch: any = {}
    fieldsToPatch.map((f) => {
      patch[f] = this.userEditable[f]
    })
    const patchedUser = await this.currentUserService.patchUser(patch)
    this.refresh(patchedUser)
  }

  async manualFileUpload(event): Promise<void> {
    this.profileImgLoading = true
    const file: File = event?.target?.files[0]
    if (this.userContext.UserType === 'SELLER') {
      // seller stuff
      if (
        Object.keys(this.currentUserService.profileImgSubject.value).length > 0
      ) {
        // If logo exists, remove the assignment, then the logo itself
        await ContentManagementClient.Assets.DeleteAssetAssignment(
          this.currentUserService.profileImgSubject.value.ID,
          this.userContext?.Me?.ID,
          'AdminUsers',
          null,
          null
        )
        await ContentManagementClient.Assets.Delete(
          this.currentUserService.profileImgSubject.value.ID
        )
      }
    } else {
      // supplier stuff
      if (
        Object.keys(this.currentUserService.profileImgSubject.value).length > 0
      ) {
        // If logo exists, remove the assignment, then the logo itself
        await ContentManagementClient.Assets.DeleteAssetAssignment(
          this.currentUserService.profileImgSubject.value.ID,
          this.userContext?.Me?.ID,
          'SupplierUsers',
          this.userContext?.Me?.Supplier?.ID,
          'Suppliers'
        )
        await ContentManagementClient.Assets.Delete(
          this.currentUserService.profileImgSubject.value.ID
        )
      }
    }
    // Then upload logo asset
    try {
      await this.uploadProfileImg(this.userContext?.Me?.ID, file).then(
        (img) => {
          this.currentUserService.profileImgSubject.next(img)
        }
      )
    } catch (err) {
      this.hasProfileImg = false
      this.profileImgLoading = false
      throw err
    } finally {
      this.hasProfileImg = true
      this.profileImgLoading = false
      // Reset the img src for profileImg
      this.setProfileImgSrc()
    }
  }

  async uploadProfileImg(userID: string, file: File): Promise<Asset> {
    const accessToken = await this.appAuthService.fetchToken().toPromise()
    const asset: AssetUpload = {
      Active: true,
      File: file,
      FileName: file.name,
      Tags: ['ProfileImg'],
    }
    // Upload the asset, then make the asset assignment to Suppliers
    const newAsset: Asset = await HeadStartSDK.Upload.UploadAsset(
      asset,
      accessToken
    )
    if (this.userContext.UserType === 'SELLER') {
      await ContentManagementClient.Assets.SaveAssetAssignment(
        {
          ResourceType: 'AdminUsers',
          ResourceID: userID,
          AssetID: newAsset.ID,
        },
        accessToken
      )
    } else {
      await ContentManagementClient.Assets.SaveAssetAssignment(
        {
          ParentResourceType: 'Suppliers',
          ParentResourceID: this.userContext.Me.Supplier.ID,
          ResourceType: 'SupplierUsers',
          ResourceID: userID,
          AssetID: newAsset.ID,
        },
        accessToken
      )
    }
    return newAsset
  }

  async removeProfileImg(): Promise<void> {
    this.profileImgLoading = true
    try {
      if (this.userContext.UserType === 'SELLER') {
        // Remove the profile img asset assignment
        await ContentManagementClient.Assets.DeleteAssetAssignment(
          this.currentUserService.profileImgSubject.value.ID,
          this.userContext?.Me?.ID,
          'AdminUsers',
          null,
          null
        )
        // Remove the profile img asset
        await ContentManagementClient.Assets.Delete(
          this.currentUserService.profileImgSubject.value.ID
        )
      } else {
        // Remove the profile img asset assignment
        await ContentManagementClient.Assets.DeleteAssetAssignment(
          this.currentUserService.profileImgSubject.value.ID,
          this.userContext?.Me?.ID,
          'SupplierUsers',
          this.userContext?.Me?.Supplier?.ID,
          'Suppliers'
        )
        // Remove the profile img asset
        await ContentManagementClient.Assets.Delete(
          this.currentUserService.profileImgSubject.value.ID
        )
      }
      this.currentUserService.profileImgSubject.next({})
    } catch (err) {
      throw err
    } finally {
      this.hasProfileImg = false
      this.profileImgLoading = false
    }
  }

  setProfileImgSrc(): void {
    if (this.userContext.UserType === 'SELLER') {
      const url = `${environment.cmsUrl}/assets/${this.appConfig.sellerID}/AdminUsers/${this.userContext.Me.ID}/thumbnail?size=m`
      this.myProfileImg = url
    } else {
      const url = `${environment.cmsUrl}/assets/${this.appConfig.sellerID}/Suppliers/${this.userContext.Me.Supplier.ID}/SupplierUsers/${this.userContext.Me.ID}/thumbnail?size=m`
      this.myProfileImg = url
    }
  }
}
