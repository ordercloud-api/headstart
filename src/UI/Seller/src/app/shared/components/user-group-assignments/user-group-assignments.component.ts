import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnChanges,
  SimpleChanges,
  Inject,
  OnInit,
} from '@angular/core'
import {
  User,
  UserGroup,
  UserGroupAssignment,
  OcTokenService,
  ListPage,
} from '@ordercloud/angular-sdk'
import { faExclamationCircle } from '@fortawesome/free-solid-svg-icons'
import { REDIRECT_TO_FIRST_PARENT } from '@app-seller/layout/header/header.config'
import { GetDisplayText } from './user-group-assignments.constants'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http'
import {
  HeadStartSDK,
  HSLocationUserGroup,
  ListArgs,
} from '@ordercloud/headstart-sdk'
import {
  AssignmentsToAddUpdate,
  IUserPermissionsService,
} from '@app-seller/models/user.types'
import { AppConfig } from '@app-seller/shared'
import { Router } from '@angular/router'

@Component({
  selector: 'user-group-assignments',
  templateUrl: './user-group-assignments.component.html',
  styleUrls: ['./user-group-assignments.component.scss'],
})
export class UserGroupAssignments implements OnInit, OnChanges {
  @Input() user: User
  @Input() userGroupType: string
  @Input() isCreatingNew: boolean
  @Input() userPermissionsService: IUserPermissionsService
  @Input() homeCountry: string
  @Output() assignmentsToAdd = new EventEmitter<AssignmentsToAddUpdate>()
  @Output() hasAssignments = new EventEmitter<boolean>()

  userOrgID: string
  buyerID: string
  userID: string
  userGroups: ListPage<HSLocationUserGroup> | UserGroup[]
  add: UserGroupAssignment[]
  del: UserGroupAssignment[]
  _userUserGroupAssignmentsStatic: UserGroupAssignment[] = []
  _userUserGroupAssignmentsEditable: UserGroupAssignment[] = []
  areChanges = false
  requestedUserConfirmation = false
  faExclamationCircle = faExclamationCircle
  options = { filters: { 'xp.Type': '' } }
  displayText = ''
  searchTermInput: string
  searching: boolean
  viewAssignedUserGroups = false
  args: ListArgs = { pageSize: 100, filters: { assigned: 'false' } }
  retrievingAssignments: boolean

  constructor(
    private http: HttpClient,
    private ocTokenService: OcTokenService,
    private router: Router,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {}

  ngOnInit() {
    var url = this.router?.routerState?.snapshot?.url
    if(url && url.split('/').length) {
      this.buyerID = url.split('/')[2]
    }
  }

  async ngOnChanges(changes: SimpleChanges): Promise<void> {
    this.updateForUserGroupAssignmentType()
    this.userOrgID = await this.userPermissionsService.getParentResourceID()
    this.searchTermInput = ''
    this.args.search = null
    if (this.userOrgID !== REDIRECT_TO_FIRST_PARENT) {
      await this.getUserGroups(this.userOrgID)
    }
    if (changes.user?.currentValue.ID && !this.userID) {
      this.userID = this.user.ID
      if (this.userOrgID && this.userOrgID !== REDIRECT_TO_FIRST_PARENT) {
        this.getUserGroupAssignments(this.user.ID, this.userOrgID)
      }
    }
    if (
      this.userID &&
      changes.user?.currentValue?.ID !== changes.user?.previousValue?.ID
    ) {
      this.userID = this.user.ID
      this.getUserGroupAssignments(this.user.ID, this.userOrgID)
    }
  }

  updateForUserGroupAssignmentType(): void {
    this.options.filters['xp.Type'] = this.userGroupType
    this.displayText = GetDisplayText(this.userGroupType)
  }

  requestUserConfirmation(): void {
    this.requestedUserConfirmation = true
  }

  async getUserGroups(ID: string): Promise<void> {
    if (this.userGroupType === 'UserPermissions') {
      this.userGroups = await this.userPermissionsService.getUserGroups(
        ID,
        this.options
      )
    } else if (
      this.userGroupType === 'BuyerLocation' &&
      this.user.xp?.Country
    ) {
      const groups = await this.getUserGroupsByCountry(
        this.userOrgID,
        this.user.ID
      )
      this.userGroups = groups
    } else {
      const url = `${this.appConfig.middlewareUrl}/buyerlocations/${ID}/${this.homeCountry}/usergroups`
      //TO-DO - Replace with SDK
      this.userGroups = await this.http
        .get<ListPage<HSLocationUserGroup>>(url, {
          headers: this.buildHeaders(),
          params: this.createHttpParams(this.args),
        })
        .toPromise()
    }
  }

  async getUserGroupAssignments(userID: any, userOrgID: any): Promise<void> {
    let userGroupAssignments
    if (this.userGroupType === 'UserPermissions') {
      userGroupAssignments = await (
        await this.userPermissionsService.listUserAssignments(userID, userOrgID)
      ).Items
    } else {
      const url = `${this.appConfig.middlewareUrl}/buyerlocations/${this.userGroupType}/${userOrgID}/usergroupassignments/${userID}`
      //TO-DO - Replace with SDK (1.8.4 or later for updated arguments)
      userGroupAssignments = await this.http
        .get<UserGroupAssignment[]>(url, { headers: this.buildHeaders() })
        .toPromise()
    }
    this._userUserGroupAssignmentsStatic = userGroupAssignments
    this._userUserGroupAssignmentsEditable = userGroupAssignments
    const match = this._userUserGroupAssignmentsStatic?.length
      ? this._userUserGroupAssignmentsStatic.some((assignedUG) =>
          (this.userGroups as any).Items?.find(
            (ug) => ug.ID === assignedUG.UserGroupID
          )
        )
      : false
    this.hasAssignments.emit(match)
  }

  toggleUserUserGroupAssignment(userGroup: UserGroup): void {
    if (this.isAssigned(userGroup)) {
      this._userUserGroupAssignmentsEditable = this._userUserGroupAssignmentsEditable.filter(
        (groupAssignment) => groupAssignment.UserGroupID !== userGroup.ID
      )
    } else {
      const newUserUserGroupAssignment = {
        UserID: this.userID,
        UserGroupID: userGroup.ID,
      }
      this._userUserGroupAssignmentsEditable = [
        ...this._userUserGroupAssignmentsEditable,
        newUserUserGroupAssignment,
      ]
    }
    this.checkForUserUserGroupAssignmentChanges()
  }

  addUserUserGroupAssignment(userGroup: UserGroup): void {
    if (this.isAssigned(userGroup)) {
      this._userUserGroupAssignmentsEditable = this._userUserGroupAssignmentsEditable.filter(
        (groupAssignment) => groupAssignment.UserGroupID !== userGroup.ID
      )
    } else {
      const newUserUserGroupAssignment = {
        UserID: 'PENDING',
        UserGroupID: userGroup.ID,
      }
      this._userUserGroupAssignmentsEditable = [
        ...this._userUserGroupAssignmentsEditable,
        newUserUserGroupAssignment,
      ]
    }
    this.checkForUserUserGroupAssignmentChanges()
  }

  isAssigned(userGroup: UserGroup): boolean {
    return this._userUserGroupAssignmentsEditable?.some(
      (groupAssignment) => groupAssignment.UserGroupID === userGroup.ID
    )
  }

  checkForUserUserGroupAssignmentChanges(): void {
    this.add = this._userUserGroupAssignmentsEditable.filter(
      (editableAssignment) =>
        !this._userUserGroupAssignmentsStatic.some(
          (staticAssignment) =>
            staticAssignment.UserGroupID === editableAssignment.UserGroupID
        )
    )
    this.del = this._userUserGroupAssignmentsStatic.filter(
      (staticAssignment) =>
        !this._userUserGroupAssignmentsEditable.some(
          (editableAssignment) =>
            editableAssignment.UserGroupID === staticAssignment.UserGroupID
        )
    )
    this.areChanges = this.add.length > 0 || this.del.length > 0
    if (!this.areChanges) this.requestedUserConfirmation = false
    if (this.isCreatingNew)
      this.assignmentsToAdd.emit({
        UserGroupType: this.userGroupType,
        Assignments: this.add,
      })
  }

  discardUserUserGroupAssignmentChanges(): void {
    this._userUserGroupAssignmentsEditable = this._userUserGroupAssignmentsStatic
    this.checkForUserUserGroupAssignmentChanges()
  }

  async executeUserUserGroupAssignmentRequests(): Promise<void> {
    this.requestedUserConfirmation = false
    await this.userPermissionsService.updateUserUserGroupAssignments(
      this.userOrgID,
      this.add,
      this.del,
      this.userGroupType === 'BuyerLocation'
    )
    await this.getUserGroupAssignments(this.userID, this.userOrgID)
    this.checkForUserUserGroupAssignmentChanges()
  }

  async getUserGroupsByCountry(
    buyerID: string,
    userID: string
  ): Promise<ListPage<HSLocationUserGroup>> {
    const userGroups = await HeadStartSDK.BuyerLocations.ListUserGroupsByCountry(
      buyerID,
      userID,
      this.args
    )
    userGroups.Items.sort((a, b) =>
      a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0
    )
    return userGroups
  }

  private buildHeaders(): HttpHeaders {
    return new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${this.ocTokenService.GetAccess()}`,
    })
  }

  private createHttpParams(args: ListArgs): HttpParams {
    let params = new HttpParams()
    Object.entries(args).forEach(([key, value]) => {
      if (key !== 'filters' && value) {
        params = params.append(key, value.toString())
      }
    })
    Object.entries(args.filters).forEach(([key, value]) => {
      params = params.append(key, value.toString())
    })
    return params
  }

  async changePage(page: number): Promise<void> {
    this.args = { ...this.args, page }
    await this.getUserGroups(this.userOrgID)
  }

  async searchedResources(searchText: any): Promise<void> {
    this.searching = true
    this.searchTermInput = searchText
    this.args = { ...this.args, search: searchText, page: 1 }
    await this.getUserGroups(this.userOrgID)
    this.searching = false
  }

  async toggleUserGroupAssignmentView(value: boolean): Promise<void> {
    this.viewAssignedUserGroups = value
    this.userGroups = []
    this.searchTermInput = ''
    this.args = {
      pageSize: 100,
      filters: { assigned: this.viewAssignedUserGroups.toString() },
    }
    this.retrievingAssignments = true
    this.userGroups = await this.getUserGroupsByCountry(
      this.userOrgID,
      this.user.ID
    )
    this.retrievingAssignments = false
  }
}
