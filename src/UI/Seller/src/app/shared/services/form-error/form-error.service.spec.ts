import { TestBed, inject } from '@angular/core/testing';
import { AppFormErrorService } from '@app-seller/shared/services/form-error/form-error.service';
import { FormGroup, Validators, FormControl } from '@angular/forms';

describe('OcFormErrorService', () => {
  let service;
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [],
    });
  });

  it('should be created', inject(
    [AppFormErrorService],
    (_service: AppFormErrorService) => {
      service = _service;
      expect(service).toBeTruthy();
    }
  ));

  describe('displayFormErrors', () => {
    let form: FormGroup;
    beforeEach(() => {
      form = new FormGroup({
        first: new FormControl('crhistian', Validators.required),
        last: new FormControl('ramirez', Validators.required),
      });
    });
    beforeEach(() => {
      form.markAsPristine();
      Object.keys(form.controls).forEach((key) => {
        form.controls[key].markAsPristine();
      });
      service.displayFormErrors(form);
    });
    it('should mark all form controls as dirty', () => {
      Object.keys(form.controls).forEach((key) => {
        expect(form.controls[key].dirty).toBe(true);
      });
    });
  });

  describe('hasValidEmailError', () => {
    let formControl: FormControl;
    beforeEach(() => {
      formControl = new FormControl('email', [
        Validators.required,
        Validators.email,
      ]);
    });
    it('should return true if form control has required error and is dirty', () => {
      formControl.setValue('');
      formControl.markAsDirty();
      const hasError = service.hasValidEmailError(formControl);
      expect(hasError).toBe(true);
    });
    it('should return true if form control has email error', () => {
      formControl.setValue('totallynotanemail');
      formControl.markAsDirty();
      const hasError = service.hasValidEmailError(formControl);
      expect(hasError).toBe(true);
    });
    it('should return false if form control has required error but is pristine', () => {
      formControl.setValue('');
      formControl.markAsPristine();
      const hasError = service.hasValidEmailError(formControl);
      expect(hasError).toBe(false);
    });
  });

  describe('passwordMismatchError', () => {
    let form: FormGroup;
    beforeEach(() => {
      form = new FormGroup({
        password: new FormControl('crhistian', Validators.required),
        confirmPassword: new FormControl('ramirez', Validators.required),
      });
    });
    it('should return true if form has ocMatchFields error', () => {
      form.setErrors({ ocMatchFields: true });
      const hasError = service.hasPasswordMismatchError(form);
      expect(hasError).toBe(true);
    });
    it('should return false if does not have ocMatchFields error', () => {
      const hasError = service.hasPasswordMismatchError(form);
      expect(hasError).toBe(false);
    });
  });

  describe('hasRequiredError', () => {
    let form: FormGroup;
    beforeEach(() => {
      form = new FormGroup({
        password: new FormControl('', Validators.required),
      });
    });
    it('should return true if form control has required error and is dirty', () => {
      form.controls['password'].setValue('');
      form.controls['password'].markAsDirty();
      const hasError = service.hasRequiredError('password', form);
      expect(hasError).toBe(true);
    });
    it('should return true if form control has required error', () => {
      form.controls['password'].setValue('');
      form.controls['password'].markAsPristine();
      const hasError = service.hasRequiredError('password', form);
      expect(hasError).toBe(false);
    });
  });
});
