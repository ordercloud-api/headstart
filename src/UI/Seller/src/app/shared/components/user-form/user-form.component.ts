import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core'
import { User } from '@ordercloud/angular-sdk'
import { FormGroup, FormBuilder, Validators } from '@angular/forms'
import { AppFormErrorService } from '@app-seller/shared/services/form-error/form-error.service'
import { RegexService } from '@app-seller/shared/services/regex/regex.service'

@Component({
  selector: 'user-form',
  templateUrl: './user-form.component.html',
  styleUrls: ['./user-form.component.scss'],
})
export class UserFormComponent implements OnInit {
  protected _existingUser = {} as User
  @Input()
  btnText: string
  @Output()
  formSubmitted = new EventEmitter<{ user: User; prevID: string }>()
  userForm: FormGroup

  constructor(
    private formBuilder: FormBuilder,
    private formErrorService: AppFormErrorService,
    private regexService: RegexService
  ) {}

  ngOnInit() {
    this.setForm()
  }

  @Input()
  set existingUser(user: User) {
    this._existingUser = user || ({} as User)
    if (!this.userForm) {
      this.setForm()
      return
    }

    this.userForm.setValue({
      ID: this._existingUser.ID || '',
      Username: this._existingUser.Username || '',
      FirstName: this._existingUser.FirstName || '',
      LastName: this._existingUser.LastName || '',
      Phone: this._existingUser.Phone || '',
      Email: this._existingUser.Email || '',
      Active: !!this._existingUser.Active,
    })
  }

  setForm() {
    this.userForm = this.formBuilder.group({
      ID: [
        this._existingUser.ID || '',
        Validators.pattern(this.regexService.ID),
      ],
      Username: [this._existingUser.Username || '', Validators.required],
      FirstName: [
        this._existingUser.FirstName || '',
        [Validators.required, Validators.pattern(this.regexService.HumanName)],
      ],
      LastName: [
        this._existingUser.LastName || '',
        [Validators.required, Validators.pattern(this.regexService.HumanName)],
      ],
      Phone: [
        this._existingUser.Phone || '',
        Validators.pattern(this.regexService.Phone),
      ],
      Email: [
        this._existingUser.Email || '',
        [Validators.required, Validators.email],
      ],
      Active: [!!this._existingUser.Active],
    })
  }

  protected onSubmit() {
    if (this.userForm.status === 'INVALID') {
      return this.formErrorService.displayFormErrors(this.userForm)
    }

    this.formSubmitted.emit({
      user: this.userForm.value,
      prevID: this._existingUser.ID,
    })
  }

  // control display of error messages
  protected hasRequiredError = (controlName: string) =>
    this.formErrorService.hasRequiredError(controlName, this.userForm)
  protected hasPatternError = (controlName: string) =>
    this.formErrorService.hasPatternError(controlName, this.userForm)
  protected hasEmailError = () =>
    this.formErrorService.hasValidEmailError(this.userForm.get('Email'))
}
