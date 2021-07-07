import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';
import { applicationConfiguration } from '@app-seller/config/app.config';
import { HomeComponent } from './home.component';
import { InjectionToken } from '@angular/core';
import { AppConfig } from '@app-seller/shared';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [HomeComponent],
      providers: [
        {
          provide: applicationConfiguration,
          useValue: new InjectionToken<AppConfig>('app.config'),
        },
      ],
    })
      .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
