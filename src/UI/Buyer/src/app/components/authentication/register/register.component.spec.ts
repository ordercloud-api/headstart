import { async, ComponentFixture, TestBed } from '@angular/core/testing';
import { RegisterComponent } from 'src/app/ocm-default-components/components/register/register.component';
import { ReactiveFormsModule } from '@angular/forms';
import { OcMeService, OcTokenService, MeUser } from '@ordercloud/angular-sdk';
import { applicationConfiguration, AppConfig } from 'src/app/config/app.config';
import { InjectionToken, NO_ERRORS_SCHEMA } from '@angular/core';
import { Router } from '@angular/router';
import { ToastrService } from 'ngx-toastr';
import { of, Subject } from 'rxjs';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;

  const appStateService = { userSubject: new Subject<any>() };
  const ocMeService = {
    Register: jasmine.createSpy('Register').and.returnValue(of(null)),
  };
  const toastrService = { success: jasmine.createSpy('success') };
  const tokenService = {
    GetAccess: jasmine.createSpy('GetAccess').and.returnValue('mockToken'),
  };
  const router = { navigate: jasmine.createSpy('navigate') };
  const formErrorService = {
    hasRequiredError: jasmine.createSpy('hasRequiredError'),
    hasInvalidEmailError: jasmine.createSpy('hasInvalidEmailError'),
    hasPasswordMismatchError: jasmine.createSpy('hasPasswordMismatchError'),
    hasStrongPasswordError: jasmine.createSpy('hasStrongPasswordError'),
    displayFormErrors: jasmine.createSpy('displayFormErrors'),
    hasPatternError: jasmine.createSpy('hasPatternError'),
  };

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [RegisterComponent],
      imports: [ReactiveFormsModule],
      providers: [
        { provide: Router, useValue: router },
        { provide: OcTokenService, useValue: tokenService },
        { provide: OcMeService, useValue: ocMeService },
        { provide: ToastrService, useValue: toastrService },
        {
          provide: applicationConfiguration,
          useValue: new InjectionToken<AppConfig>('app.config'),
        },
      ],
      schemas: [NO_ERRORS_SCHEMA],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    beforeEach(() => {
      spyOn(component as any, 'setForm');
    });
    it('should call setForm', () => {
      component.ngOnInit();
      expect(component['setForm']).toHaveBeenCalled();
    });
  });

  describe('setForm', () => {
    it('should initialize form', () => {
      component['setForm']();
      expect(component.form.value).toEqual({
        Username: '',
        FirstName: '',
        LastName: '',
        Email: '',
        Phone: '',
        Password: '',
        ConfirmPassword: '',
      });
    });
  });

  describe('onSubmit', () => {
    it('should call displayFormErrors if form is invalid', () => {
      component.form.controls.FirstName.setValue('');
      component['onSubmit']();
      expect(formErrorService.displayFormErrors).toHaveBeenCalled();
    });
    it('should call meService.Register', () => {
      component.form.controls.Username.setValue('crhistianr');
      component.form.controls.FirstName.setValue('Crhistian');
      component.form.controls.LastName.setValue('Ramirez');
      component.form.controls.Email.setValue('crhistian-rawks@my-little-pony.com');
      component.form.controls.Phone.setValue('555-555-5555');
      component.form.controls.Password.setValue('easyguess123');
      component.form.controls.ConfirmPassword.setValue('easyguess123');
      component['onSubmit']();
      const mockMe = <MeUser>component.form.value;
      mockMe.Active = true;
      expect(ocMeService.Register).toHaveBeenCalledWith('mockToken', mockMe);
    });
    it('should navigate to login', () => {
      component['onSubmit']();
      expect(router.navigate).toHaveBeenCalledWith(['/login']);
    });
  });
});
