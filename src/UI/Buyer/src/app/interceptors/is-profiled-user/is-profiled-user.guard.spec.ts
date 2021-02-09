import { TestBed } from '@angular/core/testing';
import { IsProfiledUserGuard } from './is-profiled-user.guard';
import { BehaviorSubject } from 'rxjs';

describe('IsProfiledUserGuard', () => {
  let service: IsProfiledUserGuard;
  const appStateService = {
    isAnonSubject: new BehaviorSubject(true),
  };
  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [],
      providers: [{ useValue: appStateService }],
    });
    service = TestBed.get(IsProfiledUserGuard);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
