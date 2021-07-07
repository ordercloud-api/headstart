import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { SellerLocationEditComponent } from './seller-location-edit.component';

describe('SellerLocationEditComponent', () => {
  let component: SellerLocationEditComponent;
  let fixture: ComponentFixture<SellerLocationEditComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ SellerLocationEditComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(SellerLocationEditComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
