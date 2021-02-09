import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { KitStaticCardComponent } from './kit-static-card.component';

describe('KitStaticCardComponent', () => {
  let component: KitStaticCardComponent;
  let fixture: ComponentFixture<KitStaticCardComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ KitStaticCardComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(KitStaticCardComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
