import { HttpClient } from '@angular/common/http'
import { Component, Inject, OnInit } from '@angular/core'
import { AbstractControl, UntypedFormBuilder, Validators } from '@angular/forms'
import { DomSanitizer, SafeUrl } from '@angular/platform-browser'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { AppConfig, FileHandle } from '@app-seller/shared'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { MeUser, Suppliers, Supplier } from 'ordercloud-javascript-sdk'
import { ToastrService } from 'ngx-toastr'
import { takeWhile } from 'rxjs/operators'
import { faAsterisk } from '@fortawesome/free-solid-svg-icons'
import { TypedFormGroup } from 'ngx-forms-typed'
import { HeadStartSDK } from '@ordercloud/headstart-sdk'

interface CaseSubmissionForm {
  FirstName: string
  LastName: string
  Vendor: string
  Subject: string
  Message: string
  File: File
}

@Component({
  selector: 'support-case-submission',
  templateUrl: './case-submission.component.html',
  styleUrls: ['./case-submission.component.scss'],
})
export class CaseSubmissionComponent implements OnInit {
  alive = true
  caseSubmissionForm: TypedFormGroup<CaseSubmissionForm>
  subjectOptions: string[] = [
    'General Question',
    'Report an Error/Bug',
    'Payment, Billing, or Refunds',
  ]
  user: MeUser
  attachmentFile: File
  stagedAttachmentUrl: SafeUrl = null
  isImageFileType = false
  vendor: Supplier
  submitBtnDisabled = false
  faAsterisk = faAsterisk

  constructor(
    private currentUserService: CurrentUserService,
    private formBuilder: UntypedFormBuilder,
    private http: HttpClient,
    private sanitizer: DomSanitizer,
    private toastrService: ToastrService,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {}

  ngOnInit(): void {
    this.currentUserService.userSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe(async (user) => {
        this.user = user
        if (this.user?.Supplier?.ID) {
          this.vendor = await Suppliers.Get(this.user.Supplier.ID)
        }
        this.setForm()
      })
  }

  setForm(): void {
    this.caseSubmissionForm = this.formBuilder.group({
      FirstName: [this.user ? this.user.FirstName : '', Validators.required],
      LastName: [this.user ? this.user.LastName : '', Validators.required],
      Email: [this.user ? this.user.Email : '', Validators.required],
      Vendor: [
        this.vendor ? this.vendor.Name : '',
        this.vendor ? Validators.required : null,
      ],
      Subject: [null, Validators.required],
      Message: ['', Validators.required],
      File: [null],
    }) as TypedFormGroup<CaseSubmissionForm>
  }

  onAttachmentUpload(event: Event): void {
    this.attachmentFile = (event.target as HTMLInputElement).files[0]
    this.stageAttachment(this.attachmentFile)
  }

  onAttachmentDrop(event: FileHandle[]): void {
    this.attachmentFile = event[0].File
    this.stageAttachment(this.attachmentFile)
  }

  stageAttachment(file: File): void {
    this.isImageFileType = file.type.includes('image')
    this.caseSubmissionForm.controls.File.setValue(file)
    this.stagedAttachmentUrl = this.sanitizer.bypassSecurityTrustUrl(
      window.URL.createObjectURL(file)
    )
  }

  removeAttachment(): void {
    this.caseSubmissionForm.controls.File.setValue(null)
    this.stagedAttachmentUrl = null
    this.attachmentFile = null
  }

  isRequired(control: string): boolean {
    const theControl = this.caseSubmissionForm.get(control)
    if (theControl.validator === null) return false
    const validator = this.caseSubmissionForm
      .get(control)
      .validator({} as AbstractControl)

    // eslint-disable-next-line @typescript-eslint/no-unsafe-return
    return validator && validator.required
  }

  async sendCaseSubmission(): Promise<void> {
    this.submitBtnDisabled = true
    const form = new FormData()
    Object.keys(this.caseSubmissionForm.value).forEach((key) => {
      if (key === 'File') {
        form.append('file', this.caseSubmissionForm.value[key])
      } else {
        form.append(key, this.caseSubmissionForm.value[key])
      }
    })

    try {
      await HeadStartSDK.Support.SubmitCase(form)
      this.toastrService.success('Support case sent', 'Success')
      this.submitBtnDisabled = false
      this.setForm()
      this.removeAttachment()
    } catch {
      this.toastrService.error(
        'There was an issue sending your request',
        'Error'
      )
    } finally {
      this.submitBtnDisabled = false
    }
  }

  ngOnDestroy(): void {
    this.alive = false
  }
}
