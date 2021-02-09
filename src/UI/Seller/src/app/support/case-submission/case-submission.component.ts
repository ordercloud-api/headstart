import { HttpClient } from '@angular/common/http'
import { Component, Inject, OnInit } from '@angular/core'
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms'
import { DomSanitizer, SafeUrl } from '@angular/platform-browser'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { AppConfig, FileHandle } from '@app-seller/shared'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { MeUser, OcSupplierService, Supplier } from '@ordercloud/angular-sdk'
import { ToastrService } from 'ngx-toastr'
import { takeWhile } from 'rxjs/operators'
import {
  faAsterisk
} from '@fortawesome/free-solid-svg-icons'

@Component({
  selector: 'support-case-submission',
  templateUrl: './case-submission.component.html',
  styleUrls: ['./case-submission.component.scss'],
})
export class CaseSubmissionComponent implements OnInit {
  alive = true
  caseSubmissionForm: FormGroup
  subjectOptions: string[] = [
    'General Question',
    'Report an Error/Bug',
    'Payment, Billing, or Refunds'
  ]
  user: MeUser
  attachmentFile
  stagedAttachmentUrl: SafeUrl = null
  isImageFileType: boolean = false
  vendor: Supplier
  submitBtnDisabled: boolean = false
  faAsterisk = faAsterisk
  
  constructor(
    private currentUserService: CurrentUserService,
    private formBuilder: FormBuilder,
    private http: HttpClient,
    private ocSupplierService: OcSupplierService,
    private sanitizer: DomSanitizer,
    private toastrService: ToastrService,
    @Inject(applicationConfiguration) private appConfig: AppConfig,
  ) { }

  ngOnInit(): void {
    this.currentUserService.userSubject.pipe(takeWhile(() => this.alive)).subscribe(async (user) => {
      this.user = user
      if (this.user?.Supplier?.ID) {
        this.vendor = await this.ocSupplierService.Get(this.user.Supplier.ID).toPromise()
      }
      this.setForm()
    })
  }

  setForm(): void {
    this.caseSubmissionForm = this.formBuilder.group({
      FirstName: [this.user ? this.user.FirstName : '', Validators.required],
      LastName: [this.user ? this.user.LastName : '', Validators.required],
      Email: [this.user ? this.user.Email : '', Validators.required],
      Vendor: [this.vendor ? this.vendor.Name : '', this.vendor ? Validators.required : null],
      Subject: [null, Validators.required],
      Message: ['', Validators.required],
      File: [null]
    })
  }

  onAttachmentUpload(event: any): void {
    this.attachmentFile = event.target.files[0]
    this.stageAttachment(this.attachmentFile)
  }

  onAttachmentDrop(event: FileHandle[]): void {
    this.attachmentFile = event[0].File
    this.stageAttachment(this.attachmentFile)
  }

  stageAttachment(file: any): void {
    this.isImageFileType = file.type.includes('image')
    this.caseSubmissionForm.controls['File'].setValue(file)
    this.stagedAttachmentUrl = this.sanitizer.bypassSecurityTrustUrl(
      window.URL.createObjectURL(file)
    )
  }

  removeAttachment(): void {
    this.caseSubmissionForm.controls['File'].setValue(null)
    this.stagedAttachmentUrl = null
    this.attachmentFile = null
  }

  isRequired(control: string): boolean {
    const theControl = this.caseSubmissionForm.get(control)
    if (theControl.validator === null) return false
    const validator = this.caseSubmissionForm
      .get(control)
      .validator({} as AbstractControl)
    return validator && validator.required
  }

  sendCaseSubmission() {
    this.submitBtnDisabled = true
    const form = new FormData()
    Object.keys(this.caseSubmissionForm.value).forEach(key => {
      if (key === 'File') {
        form.append('file', this.caseSubmissionForm.value[key])
      } else {
        form.append(key, this.caseSubmissionForm.value[key])
      }
    })
    return this.http.post(`${this.appConfig.middlewareUrl}/support/submitcase`, form)
      .subscribe(
        () => {
          this.toastrService.success('Support case sent', 'Success')
          this.submitBtnDisabled = false
          this.setForm()
          this.removeAttachment()
        }, 
        () => {
          this.toastrService.error('There was an issue sending your request', 'Error')
          this.submitBtnDisabled = false
        }
      )
  }

  ngOnDestroy(): void {
    this.alive = false
  }

}
