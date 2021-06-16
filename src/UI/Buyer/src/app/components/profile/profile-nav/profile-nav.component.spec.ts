import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ProfileNavComponent } from './profile-nav.component';

describe('ProfileNavComponent', () => {
  let component: ProfileNavComponent;
  let fixture: ComponentFixture<ProfileNavComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ProfileNavComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ProfileNavComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
