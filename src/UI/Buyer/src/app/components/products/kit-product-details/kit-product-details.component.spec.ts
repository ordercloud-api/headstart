import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { KitProductDetailsComponent } from './kit-product-details.component';

describe('KitProductDetailsComponent', () => {
  let component: KitProductDetailsComponent;
  let fixture: ComponentFixture<KitProductDetailsComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ KitProductDetailsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(KitProductDetailsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
