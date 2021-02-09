import { TestBed } from '@angular/core/testing';
import { HasTokenGuard } from '@app-seller/shared/guards/has-token/has-token.guard';
import { OcTokenService } from '@ordercloud/angular-sdk';
import { Router } from '@angular/router';
import { AppAuthService } from '@app-seller/auth';
import { of } from 'rxjs';
import { applicationConfiguration } from '@app-seller/config/app.config';
import { AppStateService } from '@app-seller/shared/services/app-state/app-state.service';

describe('HasTokenGuard', () => {
  let guard: HasTokenGuard;
  let mockAccessToken = null;
  let rememberMe = false;
  // tslint:disable-next-line:max-line-length
  const validToken =
    'eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c3IiOiJhbm9uX3VzZXIiLCJjaWQiOiI4MDIxODkzNi0zNTBiLTQxMDUtYTFmYy05NjJhZjAyM2Q2NjYiLCJvcmRlcmlkIjoiSVlBSnFOWVVpRVdyTy1Lei1TalpqUSIsInVzcnR5cGUiOiJidXllciIsInJvbGUiOlsiQnV5ZXJSZWFkZXIiLCJNZUFkbWluIiwiTWVDcmVkaXRDYXJkQWRtaW4iLCJNZUFkZHJlc3NBZG1pbiIsIk1lWHBBZG1pbiIsIlBhc3N3b3JkUmVzZXQiLCJTaGlwbWVudFJlYWRlciIsIlNob3BwZXIiLCJBZGRyZXNzUmVhZGVyIl0sImlzcyI6Imh0dHBzOi8vYXV0aC5vcmRlcmNsb3VkLmlvIiwiYXVkIjoiaHR0cHM6Ly9hcGkub3JkZXJjbG91ZC5pbyIsImV4cCI6MTUyNzA5Nzc0MywibmJmIjoxNTI2NDkyOTQzfQ.MBV7dqBq8RXSZZ8vEGidcfH8vSwOR55yHzvAq1w2bLc';
  const mockRefreshToken = 'xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx';

  const appConfig = { anonymousShoppingEnabled: true };
  const tokenService = {
    GetAccess: jasmine
      .createSpy('GetAccess')
      .and.callFake(() => mockAccessToken),
    GetRefresh: jasmine
      .createSpy('GetRefresh')
      .and.returnValue(of(mockRefreshToken)),
  };
  const router = { navigate: jasmine.createSpy('navigate') };
  const appAuthService = {
    authAnonymous: jasmine.createSpy('authAnonymous').and.returnValue(of(null)),
    getRememberStatus: jasmine
      .createSpy('getRememberStatus')
      .and.callFake(() => rememberMe),
    refresh: jasmine.createSpy('refresh').and.returnValue(of(null)),
  };
  const appStateService = {
    isLoggedIn: { next: jasmine.createSpy('next').and.returnValue(null) },
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [],
      providers: [
        { provide: applicationConfiguration, useValue: appConfig },
        { provide: AppAuthService, useValue: appAuthService },
        { provide: Router, useValue: router },
        { provide: OcTokenService, useValue: tokenService },
        { provide: AppStateService, useValue: appStateService },
      ],
    });
    guard = TestBed.get(HasTokenGuard);
  });

  // set Date.now for consistent test results
  const originalDateNow = Date.now;
  beforeAll(() => {
    const mockDateNow = () => 1526497620725;
    Date.now = mockDateNow;
  });
  afterAll(() => {
    Date.now = originalDateNow;
  });

  it('should ...', () => {
    expect(guard).toBeTruthy();
  });

  describe('canActivate', () => {
    describe('user is logged in', () => {
      beforeEach(() => {
        appConfig.anonymousShoppingEnabled = false;
      });
      it('should return true if token is valid', () => {
        mockAccessToken = validToken;
        guard.canActivate().subscribe((isTokenValid) => {
          expect(appAuthService.authAnonymous).not.toHaveBeenCalled();
          expect(appStateService.isLoggedIn.next).toHaveBeenCalledWith(true);
          expect(isTokenValid).toBe(true);
        });
      });
      it('should return false if token is invalid', () => {
        mockAccessToken = null;
        guard.canActivate().subscribe((isTokenValid) => {
          expect(appAuthService.authAnonymous).not.toHaveBeenCalled();
          expect(isTokenValid).toBe(false);
        });
      });
    });
    describe('access token timed out but user has refresh token', () => {
      it('should call refresh', () => {
        mockAccessToken = null;
        rememberMe = true;
        guard.canActivate().subscribe((isTokenValid) => {
          expect(appAuthService.refresh).toHaveBeenCalled();
          expect(isTokenValid).toBe(true);
        });
      });
    });
  });

  describe('isTokenValid', () => {
    it('should return false if token does not exist', () => {
      mockAccessToken = null;
      const isTokenValid = guard['isTokenValid']();
      expect(isTokenValid).toBe(false);
    });
    it('it should return false if token can not be parsed', () => {
      mockAccessToken = 'cant_parse_this_hammertime';
      const isTokenValid = guard['isTokenValid']();
      expect(isTokenValid).toBe(false);
    });
    describe('expiration time', () => {
      it('should return false if expiresIn time is less than current time', () => {
        // decodedToken.exp set to 1526497620
        // tslint:disable-next-line:max-line-length
        const lessThanCurrentTime =
          'eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c3IiOiJhbm9uX3VzZXIiLCJjaWQiOiI4MDIxODkzNi0zNTBiLTQxMDUtYTFmYy05NjJhZjAyM2Q2NjYiLCJvcmRlcmlkIjoiSVlBSnFOWVVpRVdyTy1Lei1TalpqUSIsInVzcnR5cGUiOiJidXllciIsInJvbGUiOlsiQnV5ZXJSZWFkZXIiLCJNZUFkbWluIiwiTWVDcmVkaXRDYXJkQWRtaW4iLCJNZUFkZHJlc3NBZG1pbiIsIk1lWHBBZG1pbiIsIlBhc3N3b3JkUmVzZXQiLCJTaGlwbWVudFJlYWRlciIsIlNob3BwZXIiLCJBZGRyZXNzUmVhZGVyIl0sImlzcyI6Imh0dHBzOi8vYXV0aC5vcmRlcmNsb3VkLmlvIiwiYXVkIjoiaHR0cHM6Ly9hcGkub3JkZXJjbG91ZC5pbyIsImV4cCI6MTUyNjQ5NzYyMCwibmJmIjoxNTI2NDkyOTQzfQ.W1GyDrOUyRxs8GZSiW0jk__37Cv98t2A_u7AK2PaMtU';
        mockAccessToken = lessThanCurrentTime;
        const isTokenValid = guard['isTokenValid']();
        expect(isTokenValid).toBe(false);
      });
      it('should return true if expiresIn time is greater than current time', () => {
        // decodedToken.exp set to 1526497621
        // tslint:disable-next-line:max-line-length
        const greaterThanCurrentTime =
          'eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c3IiOiJhbm9uX3VzZXIiLCJjaWQiOiI4MDIxODkzNi0zNTBiLTQxMDUtYTFmYy05NjJhZjAyM2Q2NjYiLCJvcmRlcmlkIjoiSVlBSnFOWVVpRVdyTy1Lei1TalpqUSIsInVzcnR5cGUiOiJidXllciIsInJvbGUiOlsiQnV5ZXJSZWFkZXIiLCJNZUFkbWluIiwiTWVDcmVkaXRDYXJkQWRtaW4iLCJNZUFkZHJlc3NBZG1pbiIsIk1lWHBBZG1pbiIsIlBhc3N3b3JkUmVzZXQiLCJTaGlwbWVudFJlYWRlciIsIlNob3BwZXIiLCJBZGRyZXNzUmVhZGVyIl0sImlzcyI6Imh0dHBzOi8vYXV0aC5vcmRlcmNsb3VkLmlvIiwiYXVkIjoiaHR0cHM6Ly9hcGkub3JkZXJjbG91ZC5pbyIsImV4cCI6MTUyNjQ5NzYyMSwibmJmIjoxNTI2NDkyOTQzfQ.EQ587x_hiCLu0hW6zTp-XxcXDUZdJjB5wFYC_RYqsf0';
        mockAccessToken = greaterThanCurrentTime;
        const isTokenValid = guard['isTokenValid']();
        expect(isTokenValid).toBe(true);
      });
    });
  });
});
