import { Component, Input, OnInit, Output, EventEmitter } from "@angular/core";
import { FormControl, FormGroup, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { ResourceFormUpdate } from "@app-seller/models/shared.types";
import { ValidateNoSpecialCharactersAndSpaces } from "@app-seller/validators/validators";
import { ApprovalRule } from "@ordercloud/angular-sdk";
import { ListPage, HeadStartSDK } from "@ordercloud/headstart-sdk";
import { UserGroup, UserGroups } from "ordercloud-javascript-sdk";


@Component({
    selector: 'app-approval-rule-edit',
    templateUrl: './buyer-approval-edit.component.html',
    styleUrls: ['./buyer-approval-edit.component.scss']
})

export class ApprovalEditComponent implements OnInit {
    @Input()
    set resourceInSelection(approval: ApprovalRule) {
        this.buildApprovalForm(approval)
    }
    @Output()
    updateResource = new EventEmitter<ResourceFormUpdate>()

    existingLocations: ListPage<UserGroup>
    resourceForm: FormGroup

    constructor(
        private router: Router
    ) {}

    async ngOnInit(): Promise<void> {
        const routeUrl = this.router.routerState.snapshot.url
        const buyerID = routeUrl.split('/')[2]
        this.existingLocations = await HeadStartSDK.Services.ListAll(UserGroups, UserGroups.List, buyerID)
    }

    async buildApprovalForm(approval?: ApprovalRule): Promise<void> {
        const routeUrl = this.router.routerState.snapshot.url
        const buyerID = routeUrl.split('/')[2]
        this.existingLocations = await HeadStartSDK.Services.ListAll(UserGroups, UserGroups.List, buyerID)
        this.resourceForm = new FormGroup({
            ID: new FormControl(approval?.ID, ValidateNoSpecialCharactersAndSpaces),
            Name: new FormControl(approval?.Name),
            Description: new FormControl(approval?.Description),
            ApprovingGroupID: new FormControl(approval?.ApprovingGroupID, Validators.required),
            RuleExpression: new FormControl(approval?.RuleExpression, Validators.required)
        })
    }
}