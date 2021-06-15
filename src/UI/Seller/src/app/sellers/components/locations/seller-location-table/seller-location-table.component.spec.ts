import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { SellerLocationTableComponent } from './seller-location-table.component';

describe('SellerLocationTableComponent', () => {
  let component: SellerLocationTableComponent;
  let fixture: ComponentFixture<SellerLocationTableComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SellerLocationTableComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SellerLocationTableComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
