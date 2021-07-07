import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { ProductAttachmentsComponent } from './product-attachments.component';

describe('ProductAttachmentsComponent', () => {
  let component: ProductAttachmentsComponent;
  let fixture: ComponentFixture<ProductAttachmentsComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ ProductAttachmentsComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(ProductAttachmentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
