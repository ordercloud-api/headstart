import { ComponentFixture, TestBed } from '@angular/core/testing';

import { OrderReturnCommentsComponent } from './order-return-comments.component';

describe('OrderReturnCommentsComponent', () => {
  let component: OrderReturnCommentsComponent;
  let fixture: ComponentFixture<OrderReturnCommentsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      declarations: [ OrderReturnCommentsComponent ]
    })
    .compileComponents();
  });

  beforeEach(() => {
    fixture = TestBed.createComponent(OrderReturnCommentsComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
