import { TestBed } from '@angular/core/testing';
import { HttpClient, HTTP_INTERCEPTORS } from '@angular/common/http';
import {
  HttpClientTestingModule,
  HttpTestingController,
} from '@angular/common/http/testing';
import { CacheInterceptor } from '@app-seller/auth/interceptors/cache/cache-interceptor';

describe('CacheInterceptor', () => {
  let interceptor;
  let httpClient: HttpClient;
  let httpMock: HttpTestingController;
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        {
          provide: HTTP_INTERCEPTORS,
          useClass: CacheInterceptor,
          multi: true,
        },
      ],
    });
    interceptor = TestBed.get(CacheInterceptor);
    httpClient = TestBed.get(HttpClient);
    httpMock = TestBed.get(HttpTestingController);
  });

  it('should be created', () => {
    expect(interceptor).toBeTruthy();
  });

  describe('non-get requests', () => {
    const nonGetHttpVerbs = ['post', 'put', 'patch', 'delete'];
    beforeEach(() => {
      // mocking user agent to be IE11
      window.navigator['__defineGetter__']('userAgent', () => {
        return 'Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko';
      });
    });
    nonGetHttpVerbs.forEach((verb) => {
      it('should not be cached', () => {
        // create a request for each non-get verb and assert that no cache headers are added
        httpClient[verb]('some-mock-url').subscribe((response) => {
          expect(response).toBeTruthy();
        });
        const req = httpMock.expectOne('some-mock-url');
        expect(req.request.headers.get('Cache-Control')).toBeNull();
        expect(req.request.headers.get('Pragma')).toBeNull();
        expect(req.request.headers.get('Expires')).toBeNull();
        req.flush({ hello: 'world' });
        httpMock.verify();
      });
    });
  });

  it('should not cache non-ie requests', () => {
    // mocking user agent to be chrome (something other than IE11)
    window.navigator['__defineGetter__']('userAgent', () => {
      return 'Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/68.0.3440.106 Safari/537.36';
    });
    // create a get request and assert
    httpClient.get('some-mock-url').subscribe((response) => {
      expect(response).toBeTruthy();
    });
    const req = httpMock.expectOne('some-mock-url');
    expect(req.request.headers.get('Cache-Control')).toBeNull();
    expect(req.request.headers.get('Pragma')).toBeNull();
    expect(req.request.headers.get('Expires')).toBeNull();
    req.flush({ hello: 'world' });
    httpMock.verify();
  });

  it('IE11 get requests should be cached', () => {
    // mocking user agent to be IE11
    window.navigator['__defineGetter__']('userAgent', () => {
      return 'Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko';
    });
    // create a request for each non-get verb and assert that no cache headers are added
    httpClient.get('some-mock-url').subscribe((response) => {
      expect(response).toBeTruthy();
    });
    const req = httpMock.expectOne('some-mock-url');
    expect(req.request.headers.get('Cache-Control')).toBe('no-cache');
    expect(req.request.headers.get('Pragma')).toBe('no-cache');
    expect(req.request.headers.get('Expires')).toBe(
      'Sat, 01 Jan 2000 00:00:00 GMT'
    );
    req.flush({ hello: 'world' });
    httpMock.verify();
  });
});
