import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { ProfileComponent } from 'src/app/profile/containers/profile/profile.component';
import { SharedModule } from 'src/app/shared';
import { NO_ERRORS_SCHEMA, InjectionToken } from '@angular/core';
import { OcMeService, OcTokenService, Configuration } from '@ordercloud/angular-sdk';
import { applicationConfiguration, AppConfig } from 'src/app/config/app.config';
import { AuthService } from 'src/app/shared/services/auth/auth.service';

describe('ProfileComponent', () => {
  let component: ProfileComponent;
  let fixture: ComponentFixture<ProfileComponent>;
  const appAuthService = { logout: jasmine.createSpy('logout') };

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ProfileComponent],
      imports: [SharedModule],
      providers: [
        { provide: AuthService, useValue: appAuthService },
        {
          provide: applicationConfiguration,
          useValue: new InjectionToken<AppConfig>('app.config'),
        },
      ],
      schemas: [NO_ERRORS_SCHEMA], // Ignore template errors: remove if tests are added to test template
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ProfileComponent);
    component = fixture.componentInstance;
    component.tabs = [
      { display: 'Details', route: ['/profile', 'details'] },
      { display: 'Addresses', route: ['/profile', 'addresses'] },
      { display: 'Payment Methods', route: ['/profile', 'payment-methods'] },
      { display: 'My Orders', route: ['/profile', 'orders'] },
      {
        display: 'Orders To Approve',
        route: ['/profile', 'orders', 'approval'],
      },
    ];
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  describe('ngOnInit', () => {
    beforeEach(() => {
      spyOn(component, 'selectTab');
      component.ngOnInit();
    });
    it('should set tab to first item in tabs array', () => {
      expect(component.selectTab).toHaveBeenCalledWith({
        display: 'Details',
        route: ['/profile', 'details'],
      });
      expect(component.selectedTab).toBe('Details');
    });
  });

  describe('selectTab', () => {
    it('should set selectedTab to tab.display', () => {
      component['tabs'].forEach(tab => {
        component.selectTab(tab);
        expect(component.selectedTab).toBe(tab.display);
      });
    });
  });

  describe('logout', () => {
    it('should call appAuthService.logout', () => {
      component.logout();
      expect(appAuthService.logout).toHaveBeenCalled();
    });
  });
});
