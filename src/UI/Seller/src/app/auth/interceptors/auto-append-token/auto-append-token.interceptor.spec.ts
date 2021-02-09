import { TestBed, inject } from '@angular/core/testing';
import { HttpClient, HTTP_INTERCEPTORS } from '@angular/common/http';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';

import { AutoAppendTokenInterceptor } from '@app-seller/auth/interceptors/auto-append-token/auto-append-token.interceptor';
import { applicationConfiguration } from '@app-seller/config/app.config';
import { OcTokenService } from '@ordercloud/angular-sdk';
import { CookieModule } from 'ngx-cookie';

describe('AutoAppendTokenInterceptor', () => {
  const mockToken = 'ABC123';
  const mockMiddlewareUrl = 'my-integration-path/api';
  const tokenService = {
    GetAccess: jasmine.createSpy('GetAccess').and.returnValue(mockToken),
  };
  const appConfig = { middlewareUrl: mockMiddlewareUrl };
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [CookieModule.forRoot(), HttpClientTestingModule],
      providers: [
        { provide: OcTokenService, useValue: tokenService },
        {
          provide: HTTP_INTERCEPTORS,
          useClass: AutoAppendTokenInterceptor,
          multi: true,
        },
        { provide: applicationConfiguration, useValue: appConfig },
      ],
    });
    httpClient = TestBed.get(HttpClient);
    httpMock = TestBed.get(HttpTestingController);
  });

  it('should be created', inject(
    [AutoAppendTokenInterceptor],
    (service: AutoAppendTokenInterceptor) => {
      expect(service).toBeTruthy();
    }
  ));

  describe('making http calls', () => {
    it('should add authorization headers to integration calls', () => {
      httpClient.get(`${mockMiddlewareUrl}/data`).subscribe((response) => {
        expect(response).toBeTruthy();
      });
      const req = httpMock.expectOne(`${mockMiddlewareUrl}/data`);
      expect(req.request.headers.get('Authorization')).toEqual(
        `Bearer ${mockToken}`
      );
      req.flush({ hello: 'World' });
      httpMock.verify();
    });
    it('should not add authorization headers if call is not to integrations url', () => {
      httpClient.get('/data').subscribe((response) => {
        expect(response).toBeTruthy();
      });
      const req = httpMock.expectOne('/data');
      expect(req.request.headers.get('Authorization')).toBe(null);
      req.flush({ hello: 'World' });
      httpMock.verify();
    });
  });
});
