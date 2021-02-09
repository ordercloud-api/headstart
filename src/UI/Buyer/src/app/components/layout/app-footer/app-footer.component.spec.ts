import { async, ComponentFixture, TestBed } from '@angular/core/testing';

import { OCMAppFooter } from '../app-footer/app-footer.component';
import { FaIconComponent } from '@fortawesome/angular-fontawesome';

describe('OCMAppFooter', () => {
  let component: OCMAppFooter;
  let fixture: ComponentFixture<OCMAppFooter>;

  beforeEach(async(() => {
    TestBed.configureTestingModule({
      declarations: [OCMAppFooter, FaIconComponent],
    }).compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(OCMAppFooter);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
