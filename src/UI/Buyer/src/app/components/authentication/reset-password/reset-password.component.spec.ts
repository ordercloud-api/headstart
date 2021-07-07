import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router, ActivatedRoute } from '@angular/router';
import { of } from 'rxjs';
import { HttpClientModule } from '@angular/common/http';

import { ToastrService } from 'ngx-toastr';
import { OCMResetPassword } from './reset-password.component';

describe('OCMResetPassword', () => {
  let component: OCMResetPassword;
  let fixture: ComponentFixture<OCMResetPassword>;

  const router = { navigateByUrl: jasmine.createSpy('navigateByUrl') };
  const ocPasswordService = {
    ResetPasswordByVerificationCode: jasmine.createSpy('ResetPasswordByVerificationCode').and.returnValue(of(true)),
  };
  const toastrService = { success: jasmine.createSpy('success') };
  const activatedRoute = {
    snapshot: { queryParams: { user: 'username', code: 'pwverificationcode' } },
  };
  const formErrorService = {
    hasPasswordMismatchError: jasmine.createSpy('hasPasswordMismatchError'),
    hasRequiredError: jasmine.createSpy('hasRequiredError'),
    hasStrongPasswordError: jasmine.createSpy('hasStrongPasswordError'),
  };

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [OCMResetPassword],
      imports: [ReactiveFormsModule, HttpClientModule],
      providers: [
        { provide: Router, useValue: router },
        { provide: ActivatedRoute, useValue: activatedRoute },
        { provide: ToastrService, useValue: toastrService },
      ],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OCMResetPassword);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
  describe('ngOnInit', () => {
    beforeEach(() => {
      component.ngOnInit();
    });
    it('should set the form values to empty strings, and the local vars to the matching query params', () => {
      expect(component.form.value).toEqual({
        password: '',
        passwordConfirm: '',
      });
      expect(component.username).toEqual(activatedRoute.snapshot.queryParams.user);
    });
  });
  describe('onSubmit', () => {
    beforeEach(() => {
      setValidForm();
      component['appConfig'].clientID = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';
      component.onSubmit();
    });
    it('should call ResetPasswordByVerificationCode', () => {
      expect(ocPasswordService.ResetPasswordByVerificationCode).toHaveBeenCalledWith('pwverificationcode', {
        ClientID: 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx',
        Password: component.form.value.password,
        Username: component.username,
      });
    });
    it('should call success toastr', () => {
      expect(toastrService.success).toHaveBeenCalledWith('Password Reset', 'Success');
    });
    it('should route user to login', () => {
      expect(router.navigateByUrl).toHaveBeenCalledWith('/login');
    });
  });

  function setValidForm() {
    component.form.controls['password'].setValue('fails345');
    component.form.controls['passwordConfirm'].setValue('fails345');
  }

  describe('hasRequiredError', () => {
    beforeEach(() => {
      component['hasRequiredError']('password');
    });
    it('should call formErrorService.hasRequiredError', () => {
      expect(formErrorService.hasRequiredError).toHaveBeenCalledWith('password', component.form);
    });
  });

  describe('hasPasswordMismatchError', () => {
    beforeEach(() => {
      component['hasPasswordMismatchError']();
    });
    it('should call formErrorService.hasRequiredError', () => {
      expect(formErrorService.hasPasswordMismatchError).toHaveBeenCalledWith(component.form);
    });
  });

  describe('hasStrongPasswordError', () => {
    beforeEach(() => {
      component['hasStrongPasswordError']('password');
    });
    it('should call formErrorService.hasStrongPasswordError', () => {
      expect(formErrorService.hasStrongPasswordError).toHaveBeenCalledWith('password', component.form);
    });
  });
});
