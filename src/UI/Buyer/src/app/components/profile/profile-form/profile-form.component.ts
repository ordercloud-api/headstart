import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core'
import { UntypedFormGroup, Validators, UntypedFormControl } from '@angular/forms'
// 3rd party
import { MeUser } from 'ordercloud-javascript-sdk'
import {
  ValidateName,
  ValidateEmail,
  ValidatePhone,
} from '../../../validators/validators'

@Component({
  templateUrl: './profile-form.component.html',
  styleUrls: ['./profile-form.component.scss'],
})
export class OCMProfileForm implements OnInit {
  @Output() formDismissed = new EventEmitter()
  @Output()
  formSubmitted = new EventEmitter<{ me: MeUser }>()
  profileForm: UntypedFormGroup

  private _me: MeUser = {}

  ngOnInit(): void {
    this.setForm()
  }

  @Input() set me(me: MeUser) {
    this._me = me || {}
    this.setForm()
    this.profileForm.markAsPristine()
  }

  setForm(): void {
    this.profileForm = new UntypedFormGroup({
      FirstName: new UntypedFormControl(this._me.FirstName || '', [
        Validators.required,
        ValidateName,
      ]),
      LastName: new UntypedFormControl(this._me.LastName || '', [
        Validators.required,
        ValidateName,
      ]),
      Username: new UntypedFormControl(this._me.Username || '', Validators.required),
      Email: new UntypedFormControl(this._me.Email || '', ValidateEmail),
      Phone: new UntypedFormControl(this._me.Phone || '', ValidatePhone),
    })
  }

  onSubmit(): void {
    if (this.profileForm.status === 'INVALID') {
      return
    }
    this.formSubmitted.emit({ me: this.profileForm.value })
  }

  dismissForm(): void {
    this.formDismissed.emit()
  }
}
