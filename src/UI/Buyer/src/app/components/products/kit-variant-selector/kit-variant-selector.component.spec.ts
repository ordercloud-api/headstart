import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { KitVariantSelectorComponent } from './kit-variant-selector.component';

describe('KitVariantSelectorComponent', () => {
  let component: KitVariantSelectorComponent;
  let fixture: ComponentFixture<KitVariantSelectorComponent>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [ KitVariantSelectorComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(KitVariantSelectorComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
