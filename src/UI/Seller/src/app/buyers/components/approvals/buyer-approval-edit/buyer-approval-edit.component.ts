import { Component, Input, OnInit, Output, EventEmitter, ChangeDetectorRef } from "@angular/core";
import { FormControl, FormGroup, Validators } from "@angular/forms";
import { ResourceFormUpdate } from "@app-seller/models/shared.types";
import { ValidateNoSpecialCharactersAndSpaces } from "@app-seller/validators/validators";
import { ApprovalRule } from "@ordercloud/angular-sdk";
import { ListPage, HeadStartSDK } from "@ordercloud/headstart-sdk";
import { UserGroup, UserGroups } from "ordercloud-javascript-sdk";


@Component({
    selector: 'app-approval-rule-edit',
    templateUrl: './buyer-approval-edit.component.html',
    styleUrls: ['./buyer-approval.component.scss']
})

export class BuyerApprovalEditComponent {
    @Input() existingLocations: ListPage<UserGroup>
    @Input()
    set resource(approval: ApprovalRule) {
        this.buildApprovalForm(approval)
        this.changeDetectorRef.detectChanges()
    }
    @Output()
    updateResource = new EventEmitter<ResourceFormUpdate>()

    resourceForm: FormGroup

    constructor(
        private changeDetectorRef: ChangeDetectorRef,
    ) {}

    handleUpdateResource(event: any, fieldType?: string) {
        const resourceupdate = {
            field: event.target.id,
            value: fieldType === 'boolean' ? event.target.checked : event.target.value,
            form: this.resourceForm
          };
          this.updateResource.emit(resourceupdate);
    }

    handleLocationSelect(event: any) {
        this.resourceForm.setValue({'ApprovingGroupID': event.target.value})
        this.handleUpdateResource(event)
    }

    async buildApprovalForm(approval?: ApprovalRule): Promise<void> {
        this.resourceForm = new FormGroup({
            ID: new FormControl(approval?.ID, ValidateNoSpecialCharactersAndSpaces),
            Name: new FormControl(approval?.Name),
            Description: new FormControl(approval?.Description),
            ApprovingGroupID: new FormControl(approval?.ApprovingGroupID, Validators.required),
            RuleExpression: new FormControl(approval?.RuleExpression, Validators.required)
        })
    }
}