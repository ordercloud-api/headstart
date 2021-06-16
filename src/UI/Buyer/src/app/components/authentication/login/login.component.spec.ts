import { CurrentUserService } from './../../../shared/services/current-user/current-user.service';
import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { InjectionToken, DebugElement } from '@angular/core';
import { HttpClientModule } from '@angular/common/http';
import { of, BehaviorSubject } from 'rxjs';

import { LoginComponent } from '@app-seller/auth/containers/login/login.component';
import { applicationConfiguration, AppConfig } from '@app-seller/config/app.config';

import { CookieModule } from 'ngx-cookie';
import { ToastrService } from 'ngx-toastr';
import { TranslateModule, TranslatePipe, TranslateService } from '@ngx-translate/core';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let debugElement: DebugElement;

  const router = { navigateByUrl: jasmine.createSpy('navigateByUrl') };
  const ocTokenService = {
    SetAccess: jasmine.createSpy('SetAccess'),
    SetRefresh: jasmine.createSpy('Refresh'),
  };
  const response = { access_token: '123456', refresh_token: 'refresh123456' };
  const ocAuthService = {
    Login: jasmine.createSpy('Login').and.returnValue(of(response)),
  };
  const appAuthService = {
    setRememberStatus: jasmine.createSpy('setRememberStatus'),
  };

  const currentUserService = { login: jasmine.createSpy('Login').and.returnValue(of(response)) };
  const toastrService = {};

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [LoginComponent],
      imports: [ReactiveFormsModule, CookieModule.forRoot(), HttpClientModule, TranslateModule.forRoot()],
      providers: [
        { provide: Router, useValue: router },
        { provide: CurrentUserService, useValue: currentUserService },
        { provide: ToastrService, useValue: toastrService },
        {
          provide: applicationConfiguration,
          useValue: new InjectionToken<AppConfig>('app.config'),
        },
      ],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    debugElement = fixture.debugElement;
    fixture.detectChanges();
  });

  describe('ngOnInit', () => {
    beforeEach(() => {
      component.ngOnInit();
    });
    it('should set the form values to empty strings', () => {
      expect(component.form.value).toEqual({
        username: '',
        password: '',
        rememberMe: false,
      });
    });
  });
  describe('onSubmit', () => {
    beforeEach(() => {
      component['appConfig'].clientID = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';
      component['appConfig'].scope = ['ApiClientAdmin'];
    });
    it('should call the OcAuthService Login method, OcTokenService SetAccess method, and route to home', () => {
      component.onSubmit();
      expect(currentUserService.login).toHaveBeenCalledWith('', '', false);
      expect(router.navigateByUrl).toHaveBeenCalledWith('/home');
    });
  });
});
