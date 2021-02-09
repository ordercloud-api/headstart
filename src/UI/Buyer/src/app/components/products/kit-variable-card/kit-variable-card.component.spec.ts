import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { KitVariableCardComponent } from './kit-variable-card.component';

describe('KitVariableCardComponent', () => {
  let component: KitVariableCardComponent;
  let fixture: ComponentFixture<KitVariableCardComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ KitVariableCardComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(KitVariableCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
