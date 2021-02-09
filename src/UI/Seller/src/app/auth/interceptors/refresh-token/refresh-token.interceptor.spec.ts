import { TestBed, inject } from '@angular/core/testing';
import { InjectionToken } from '@angular/core';
import { HttpClient, HTTP_INTERCEPTORS } from '@angular/common/http';
import {
  HttpClientTestingModule,
  HttpTestingController,
  TestRequest,
} from '@angular/common/http/testing';

import { RefreshTokenInterceptor } from '@app-seller/auth/interceptors/refresh-token/refresh-token.interceptor';
import {
  applicationConfiguration,
  AppConfig,
} from '@app-seller/config/app.config';
import { OcTokenService } from '@ordercloud/angular-sdk';
import { CookieModule } from 'ngx-cookie';
import { AppAuthService } from '@app-seller/auth/services/app-auth.service';
import { of, BehaviorSubject } from 'rxjs';
import { catchError } from 'rxjs/operators';

describe('RefreshTokenInterceptor', () => {
  const mockRefreshToken = 'RefreshABC123';
  const tokenService = {
    GetAccess: jasmine
      .createSpy('GetAccess')
      .and.returnValue(of(mockRefreshToken)),
  };
  const refreshToken = new BehaviorSubject('');
  const appAuthService = {
    refreshToken: refreshToken,
    fetchingRefreshToken: false,
    failedRefreshAttempt: false,
    refresh: jasmine.createSpy('refresh').and.returnValue(of(mockRefreshToken)),
  };
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [CookieModule.forRoot(), HttpClientTestingModule],
      providers: [
        { provide: AppAuthService, useValue: appAuthService },
        { provide: OcTokenService, useValue: tokenService },
        {
          provide: HTTP_INTERCEPTORS,
          useClass: RefreshTokenInterceptor,
          multi: true,
        },
        {
          provide: applicationConfiguration,
          useValue: new InjectionToken<AppConfig>('app.config'),
        },
      ],
    });
    httpClient = TestBed.get(HttpClient);
    httpMock = TestBed.get(HttpTestingController);
  });

  it('should be created', inject(
    [RefreshTokenInterceptor],
    (service: RefreshTokenInterceptor) => {
      expect(service).toBeTruthy();
    }
  ));

  describe('making http calls', () => {
    let thrownError = '';
    it('should rethrow error on non-ordercloud http calls', () => {
      httpClient
        .get('/data')
        .pipe(
          catchError((ex) => {
            thrownError = ex.error;
            return 'GoToSubscribe';
          })
        )
        .subscribe(() => {
          expect(thrownError).toBe('mockError');
        });
      const req = httpMock.expectOne('/data');
      req.flush('mockError', { status: 401, statusText: 'Unauthorized' });
      httpMock.verify();
    });
    it('should rethrow error if status code is not 401', () => {
      httpClient
        .get('https://api.ordercloud.io/v1/me')
        .pipe(
          catchError((ex) => {
            thrownError = ex.error;
            return 'GoToSubscribe';
          })
        )
        .subscribe(() => {
          expect(thrownError).toBe('mockError');
        });
      const req = httpMock.expectOne('https://api.ordercloud.io/v1/me');
      req.flush('mockError', { status: 500, statusText: 'Unauthorized' });
      httpMock.verify();
    });
    describe('refresh operation', () => {
      let firstRequest: TestRequest;
      let secondRequest: TestRequest;
      const setupMockCalls = () => {
        // call http client
        httpClient.get('https://api.ordercloud.io/v1/me').subscribe();

        // first request that "fails" but is caught
        firstRequest = httpMock.expectOne('https://api.ordercloud.io/v1/me');
        firstRequest.flush('mockBody', {
          status: 401,
          statusText: 'Unauthorized',
        });

        // second request that goes out as a consequence of the refresh operation
        secondRequest = httpMock.expectOne('https://api.ordercloud.io/v1/me');
        secondRequest.flush('mockBody');
      };
      it('should call appAuthService.refresh', () => {
        setupMockCalls();
        expect(appAuthService.refresh).toHaveBeenCalled();
      });
      it('should set refresh token on second call', () => {
        setupMockCalls();
        expect(firstRequest.request.headers.get('Authorization')).toBe(null);
        expect(secondRequest.request.headers.get('Authorization')).toBe(
          `Bearer ${mockRefreshToken}`
        );
        appAuthService.failedRefreshAttempt = true;
      });
      afterEach(() => {
        httpMock.verify();
      });
    });
  });
});
